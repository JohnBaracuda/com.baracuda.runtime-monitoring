using System;
using Baracuda.Monitoring.Interface;
using Baracuda.Monitoring.Internal.Profiles;

namespace Baracuda.Monitoring.Internal.Units
{
    public sealed class FieldUnit<TTarget, TValue> : ValueUnit<TTarget, TValue> where TTarget : class
    {
        
        #region --- [PROPERTIES] ---

        public override IMonitorProfile Profile => _fieldProfile;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [FIELDS] ---

        private readonly FieldProfile<TTarget, TValue> _fieldProfile;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CTR] ---

        internal FieldUnit(
            TTarget target,
            Func<TTarget, TValue> getValue,
            Action<TTarget, TValue> setValue,
            Func<TValue, string> customValueProcessor,
            FieldProfile<TTarget, TValue> fieldProfile) 
            : base(target, getValue, setValue, customValueProcessor, fieldProfile)
        {
            _fieldProfile = fieldProfile;
        }
       
        #endregion

    }
}