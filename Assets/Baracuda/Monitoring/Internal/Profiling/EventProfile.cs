// Copyright (c) 2022 Jonathan Lang
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Pooling.Concretions;
using Baracuda.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public class EventProfile<TTarget, TDelegate> : MonitorProfile where TDelegate : Delegate where TTarget : class
    {
        #region --- Fields & Properties ---

        public bool ShowSignature { get; } = true;
        public bool ShowSubscriber { get;  } = true;
        public bool ShowTrueCount { get; } = false;
        
        public delegate string StateFormatDelegate(TTarget target, int invokeCount);

        private readonly EventInfo _eventInfo;
        private readonly StateFormatDelegate _formatState;
        private readonly Action<TTarget, Delegate> _subscribe;
        private readonly Action<TTarget, Delegate> _remove;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Ctor & Factory ---

        /// <summary>
        /// Create a new <see cref="EventUnit{TTarget, TValue}"/> based on this profile.
        /// </summary>
        /// <param name="target">Target object for the unit. Null if it is a static unit.</param>
        internal override MonitorUnit CreateUnit(object target)
        {
            return new EventUnit<TTarget, TDelegate>((TTarget) target, _formatState, this);
        }
       
        private EventProfile(EventInfo eventInfo, MonitorAttribute attribute, MonitorProfileCtorArgs args) 
            : base(eventInfo, attribute, typeof(TTarget), typeof(TDelegate), UnitType.Event, args)
        {
            _eventInfo = eventInfo;

            if (attribute is MonitorEventAttribute eventAttribute)
            {
                ShowTrueCount = eventAttribute.ShowTrueCount;
                ShowSubscriber = eventAttribute.ShowSubscriber;
                ShowSignature = eventAttribute.ShowSignature;
            }
            
            var addMethod = eventInfo.GetAddMethod(true);
            var removeMethod = eventInfo.GetRemoveMethod(true);
            var getterDelegate = eventInfo.AsFieldInfo().CreateGetter<TTarget, Delegate>();
            
            _subscribe = CreateExpression(addMethod);
            _remove = CreateExpression(removeMethod);
            var counterDelegate = CreateCounterExpression(getterDelegate, ShowTrueCount);
            
            _formatState = CreateStateFormatter(counterDelegate, args.Settings);
        }
        
        private static Action<TTarget, Delegate> CreateExpression(MethodInfo methodInfo)
        {
            return (target, @delegate) => methodInfo.Invoke(target, new object[]{@delegate});
        }
        
        private static Func<TTarget, int> CreateCounterExpression(Func<TTarget, Delegate> func, bool trueCount)
        {
#if ENABLE_IL2CPP
            if (trueCount)
            {
                return (TTarget target) => func(target)?.GetInvocationList().Length ?? 0;
            }
            else
            {
                return (TTarget target) => func(target)?.GetInvocationList().Length - 1 ?? 0;
            }
#else
            if (trueCount)
            {
                return (TTarget target) => func(target).GetInvocationList().Length;
            }
            else
            {
                return (TTarget target) => func(target).GetInvocationList().Length - 1;
            }
#endif
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------   

        #region --- State Foramtting ---
        

        private StateFormatDelegate CreateStateFormatter(Func<TTarget, int> counterDelegate, MonitoringSettings settings)
        {
            if (ShowSignature)
            {
                var signatureString = _eventInfo.GetEventSignatureString().Colorize(settings.EventColor);

                if (ShowSubscriber)
                {
                    return (target, count) =>
                    {
                        var sb = StringBuilderPool.Get();
                        sb.Append(signatureString);
                        sb.Append(" Subscriber:");
                        sb.Append(counterDelegate(target));
                        sb.Append(" Invocations: ");
                        sb.Append(count);
                        return StringBuilderPool.Release(sb);
                    };
                }

                return (target, count) =>
                {
                    var sb = StringBuilderPool.Get();
                    sb.Append(signatureString);
                    sb.Append(" Invocations: ");
                    sb.Append(count);
                    return StringBuilderPool.Release(sb);
                };
            }

            if (ShowSubscriber)
            {
                return (target, count) =>
                {
                    var sb = StringBuilderPool.Get();
                    sb.Append(" Subscriber:");
                    sb.Append(counterDelegate(target));
                    sb.Append(" Invocations: ");
                    sb.Append(count);
                    return StringBuilderPool.Release(sb);
                };
            }

            return (target, count) =>
            {
                var sb = StringBuilderPool.Get();
                sb.Append(" Invocations: ");
                sb.Append(count);
                return StringBuilderPool.Release(sb);
            };
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------   
        
        #region --- Event Handler ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SubscribeToEvent(TTarget target, Delegate eventHandler)
        {
#if ENABLE_IL2CPP
            if (eventHandler == null)
            {
                return;
            }
#endif
            _subscribe(target, eventHandler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnsubscribeFromEvent(TTarget target, Delegate eventHandler)
        {
#if ENABLE_IL2CPP
            if (eventHandler == null)
            {
                return;
            }
#endif
            _remove(target, eventHandler);
        }

        /*
         * Matching Delegate    
         */

        /// <summary>
        /// Returns a delegate with 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        internal TDelegate CreateMatchingDelegate(Action action)
        {
#if ENABLE_IL2CPP
            return CreateEventHandlerForIL2CPP(action) as TDelegate;
#else // MONO
            return CreateEventHandlerForMono(action) as TDelegate;
#endif
        }
         
#if ENABLE_IL2CPP

        /// <summary>
        /// We cannot create a event handler method dynamically when using IL2CPP so we will only check for the
        /// most common types and create concrete expressions. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Delegate CreateEventHandlerForIL2CPP(Action action)
        {
            var handlerType = _eventInfo.EventHandlerType;
            
            if (handlerType.IsAssignableFrom(typeof(Action)))
            {
                return action;
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<bool>)))
            {
                return new Action<bool>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<int>)))
            {
                return new Action<int>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<float>)))
            {
                return new Action<float>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<Vector2>)))
            {
                return new Action<Vector2>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<Vector3>)))
            {
                return new Action<Vector3>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<Vector4>)))
            {
                return new Action<Vector4>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<Quaternion>)))
            {
                return new Action<Quaternion>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<Color>)))
            {
                return new Action<Color>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<string>)))
            {
                return new Action<string>(_ => action());
            }
            
            if (handlerType.IsAssignableFrom(typeof(Action<object>)))
            {
                return new Action<object>(_ => action());
            }

            return null;
        }
        
#else // MONO

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Delegate CreateEventHandlerForMono(Action action)
        {
            var handlerType = _eventInfo.EventHandlerType;
            var eventParams = handlerType.GetInvokeMethod().GetParameters();
            var parameters = eventParams.Select(p=> Expression.Parameter(p.ParameterType,"x"));
            var body = Expression.Call(Expression.Constant(action), action.GetType().GetInvokeMethod());
            var lambda = Expression.Lambda(body,parameters.ToArray());
            return Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);
        }
        
#endif
        
        #endregion
    }
}