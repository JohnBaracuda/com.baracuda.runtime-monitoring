using System;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiling;

namespace Baracuda.Monitoring.Internal.Units
{
    public sealed class PropertyUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
        
        #region --- [PROPERTIES] ---

        public override IMonitorProfile Profile => _propertyProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---

        private readonly PropertyProfile<TTarget, TValue> _propertyProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CTR] ---

        internal PropertyUnit(TTarget target, 
            Func<TTarget, TValue> getValue, 
            Action<TTarget, TValue> setValue, 
            Func<TValue, string> valueProcessor,
            PropertyProfile<TTarget, TValue> propertyProfile) 
            : base (target, getValue, setValue, valueProcessor, propertyProfile)
        {
            _propertyProfile = propertyProfile;
        }
       

        #endregion
      
    }
}