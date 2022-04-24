using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Pooling.Concretions;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;

namespace Baracuda.Monitoring.Internal.Profiling
{
    public class EventProfile<TTarget, TDelegate> : MonitorProfile where TDelegate : Delegate where TTarget : class
    {
        #region --- Fields & Properties ---

        public bool Refresh { get; } = true;
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

        internal override MonitorUnit CreateUnit(object target)
        {
            return new EventUnit<TTarget, TDelegate>((TTarget) target, _formatState, this);
        }
       
        public EventProfile(EventInfo eventInfo, MonitorAttribute attribute, MonitorProfileCtorArgs args) 
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

            _subscribe = CreateExpression(addMethod).Compile();
            _remove = CreateExpression(removeMethod).Compile();
            
            var getterDelegate = eventInfo.AsFieldInfo().CreateGetter<TTarget, Delegate>();
            var counterDelegate = CreateCounterExpression(getterDelegate, ShowTrueCount).Compile();
            _formatState = CreateStateFormatter(counterDelegate);
        }
        
        private static Expression<Action<TTarget, Delegate>> CreateExpression(MethodInfo methodInfo)
        {
            return (target, @delegate) => methodInfo.Invoke(target, new object[]{@delegate});
        }
        
        private static Expression<Func<TTarget, int>> CreateCounterExpression(Func<TTarget, Delegate> func, bool trueCount)
        {
            if(trueCount)
            {
                return  target => func(target).GetInvocationList().Length;
            }

            return target => func(target).GetInvocationList().Length - 1;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------   

        #region --- State Foramtting ---

        private StateFormatDelegate CreateStateFormatter(Func<TTarget, int> counterDelegate)
        {
            if (ShowSignature)
            {
                var parameterString = _eventInfo.GetEventSignatureString();
                return (target, count) =>
                {
                    var sb = StringBuilderPool.Get();
                    sb.Append(parameterString);
                    sb.Append(" Subscriber:");
                    sb.Append(counterDelegate(target));
                    sb.Append(" Invokes: ");
                    sb.Append(count);
                    return StringBuilderPool.Release(sb);
                };
            }

            return (target, count) =>
            {
                var sb = StringBuilderPool.Get();
                sb.Append(FormatData.Label);
                sb.Append(" Subscriber:");
                sb.Append(counterDelegate(target));
                sb.Append(" Invokes: ");
                sb.Append(count);
                return StringBuilderPool.Release(sb);
            };
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------   
        
        #region --- Event Handler ---

        internal void SubscribeEventHandler(TTarget target, Delegate eventHandler)
        {
            _subscribe(target, eventHandler);
        }

        internal void RemoveEventHandler(TTarget target, Delegate eventHandler)
        {
            _remove(target, eventHandler);
        }
        
        internal Delegate CreateEventHandler(Action action)
        {
            var handlerType = _eventInfo.EventHandlerType;
            var eventParams = handlerType.GetMethod("Invoke")!.GetParameters();
            var parameters = eventParams.Select(p=>Expression.Parameter(p.ParameterType,"x"));
            var body = Expression.Call(Expression.Constant(action),action.GetType().GetMethod("Invoke")!);
            var lambda = Expression.Lambda(body,parameters.ToArray());
            return Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);
        }
        
        #endregion
    }
}