// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using Baracuda.Monitoring.Units;
using Baracuda.Monitoring.Utilities.Extensions;
using Baracuda.Monitoring.Utilities.Pooling;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Baracuda.Monitoring.Profiles
{
    internal sealed class EventProfile<TTarget, TDelegate> : MonitorProfile where TDelegate : Delegate where TTarget : class
    {
        #region Fields & Properties ---

        public bool ShowSubscriberCount { get; } = true;
        public bool ShowInvokeCounter { get; } = true;
        public bool ShowTrueCount { get; } = true;
        public bool ShowSubscriberInfo { get; } = true;
        public bool ShowSignature { get; } = true;

        public delegate string StateFormatDelegate(TTarget target, int invokeCount);

        private readonly EventInfo _eventInfo;
        private readonly StateFormatDelegate _formatState;
        private readonly Action<TTarget, Delegate> _subscribe;
        private readonly Action<TTarget, Delegate> _remove;

        /// <summary>
        /// Create a new <see cref="EventHandle{TTarget,TDelegate}"/> based on this profile.
        /// </summary>
        /// <param name="target">Target object for the unit. Null if it is a static unit.</param>
        internal override MonitorHandle CreateUnit(object target)
        {
            return new EventHandle<TTarget, TDelegate>((TTarget) target, _formatState, this);
        }

        private EventProfile(EventInfo eventInfo, MonitorAttribute attribute, MonitorProfileCtorArgs args)
            : base(eventInfo, attribute, typeof(TTarget), typeof(TDelegate), MemberType.Event, args)
        {
            if (eventInfo.EventHandlerType.GetInvokeMethod().ReturnType != typeof(void))
            {
                throw new ArgumentException($"Monitored event must not return a value! [{eventInfo.DeclaringType?.Name}.{eventInfo.Name}]");
            }

            _eventInfo = eventInfo;

            if (attribute is MonitorEventAttribute eventAttribute)
            {
                ShowTrueCount = eventAttribute.ShowTrueCount;
                ShowSubscriberCount = eventAttribute.ShowSubscriberCount;
                ShowInvokeCounter = eventAttribute.ShowInvokeCounter;
                ShowSubscriberInfo = eventAttribute.ShowSubscriberInfo;
                ShowSignature = eventAttribute.ShowSignature;
            }

            var addMethod = eventInfo.GetAddMethod(true);
            var removeMethod = eventInfo.GetRemoveMethod(true);
            var getterDelegate = eventInfo.AsFieldInfo().CreateGetter<TTarget, Delegate>();

            _subscribe = CreateExpression(addMethod);
            _remove = CreateExpression(removeMethod);

            var elementIndent = CalculateElementIndent();

            var counterDelegate = CreateCounterExpression(getterDelegate, ShowTrueCount);
            var subDelegate = ShowSubscriberInfo ? CreateSubscriberDataExpression(getterDelegate, args.Settings, elementIndent) : null;
            _formatState = CreateDisplayEventStateDelegate(counterDelegate, subDelegate, args.Settings);
        }

        private static Action<TTarget, Delegate> CreateExpression(MethodInfo methodInfo)
        {
            return (target, @delegate) => methodInfo.Invoke(target, new object[] {@delegate});
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

        private static Func<TTarget, string> CreateSubscriberDataExpression(Func<TTarget, Delegate> func, IMonitoringSettings settings, int elementIndent)
        {
            var sb = new StringBuilder();
            var indentString = new string(' ', elementIndent);
            return target =>
            {
                try
                {
                    var delegates = func(target)?.GetInvocationList();
                    if (delegates == null)
                    {
                        return string.Empty;
                    }
                    sb.Clear();
                    for (var i = 0; i < delegates.Length; i++)
                    {
                        var item = delegates[i];
                        var delegateTarget = item.Target;
                        sb.Append('\n');
                        sb.Append(indentString);
                        sb.Append('[');
                        sb.Append(i);
                        sb.Append(']');
                        sb.Append(' ');
                        sb.Append(item.Method.DeclaringType?.HumanizedName().ColorizeString(settings.ClassColor) ?? "NULL".ColorizeString(Color.red));
                        sb.Append(settings.AppendSymbol);
                        sb.Append(item.Method.Name.ColorizeString(settings.MethodColor));

                        if (!(delegateTarget is Component component))
                        {
                            continue;
                        }

                        if (component != null)
                        {
                            sb.Append(' ');
                            sb.Append('(');
                            sb.Append(component.gameObject.scene.name.ColorizeString(settings.SceneNameColor));
                            sb.Append(' ');
                            sb.Append(component.name.ColorizeString(settings.TargetObjectColor));
                            sb.Append(')');
                        }
                        else
                        {
                            sb.Append(' ');
                            sb.Append("NULL! Target was destroyed!".ColorizeString(Color.red));
                        }
                    }

                    return sb.ToString();
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                    return exception.Message;
                }
            };
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region State Foramtting ---

        private StateFormatDelegate CreateDisplayEventStateDelegate(Func<TTarget, int> counterDelegate, Func<TTarget, string> subInfoDelegate, IMonitoringSettings settings)
        {
            var csb = ConcurrentStringBuilderPool.Get();
            if (settings.AddClassName)
            {
                csb.Append(DeclaringType.Name.ColorizeString(settings.ClassColor));
                csb.Append(settings.AppendSymbol);
            }

            csb.Append(_eventInfo.Name);
            csb.Append(':');
            if (ShowSignature)
            {
                csb.Append(' ');
                csb.Append(_eventInfo.GetEventSignatureString().ColorizeString(settings.EventColor));
            }

            var signatureString = ConcurrentStringBuilderPool.Release(csb);

            if (ShowSubscriberCount)
            {
                if (ShowInvokeCounter)
                {
                    if (subInfoDelegate != null)
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            sb.Append(" Subscriber:");
                            sb.Append(counterDelegate(target));
                            sb.Append(" Invocations: ");
                            sb.Append(count);
                            sb.Append(subInfoDelegate?.Invoke(target));
                            return StringBuilderPool.Release(sb);
                        };
                    }
                    else
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
                }
                else //!ShowInvokeCounter
                {
                    if (subInfoDelegate != null)
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            sb.Append(" Subscriber:");
                            sb.Append(counterDelegate(target));
                            sb.Append(subInfoDelegate?.Invoke(target));
                            return StringBuilderPool.Release(sb);
                        };
                    }
                    else
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            sb.Append(" Subscriber:");
                            sb.Append(counterDelegate(target));
                            return StringBuilderPool.Release(sb);
                        };
                    }
                }
            }
            else //!ShowSubscriberCount
            {
                if (ShowInvokeCounter)
                {
                    if (subInfoDelegate != null)
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            sb.Append(" Invocations: ");
                            sb.Append(count);
                            sb.Append(subInfoDelegate?.Invoke(target));
                            return StringBuilderPool.Release(sb);
                        };
                    }
                    else
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            sb.Append(" Invocations: ");
                            sb.Append(count);
                            return StringBuilderPool.Release(sb);
                        };
                    }
                }
                else //!ShowInvokeCounter
                {
                    if (subInfoDelegate != null)
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            sb.Append(subInfoDelegate?.Invoke(target));
                            return StringBuilderPool.Release(sb);
                        };
                    }
                    else
                    {
                        return (target, count) =>
                        {
                            var sb = StringBuilderPool.Get();
                            sb.Append(signatureString);
                            return StringBuilderPool.Release(sb);
                        };
                    }
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Event Handler ---

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
            var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, "x"));
            var body = Expression.Call(Expression.Constant(action), action.GetType().GetInvokeMethod());
            var lambda = Expression.Lambda(body, parameters.ToArray());
            return Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);
        }

#endif

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Misc ---

        private int CalculateElementIndent()
        {
            return TryGetMetaAttribute<MOptionsAttribute>(out var formatAttribute)
                ? formatAttribute.ElementIndent > -1 ? formatAttribute.ElementIndent : 2
                : 2;
        }

        #endregion
    }
}