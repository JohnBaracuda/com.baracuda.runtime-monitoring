// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Types;
using Baracuda.Monitoring.Source.Units;

namespace Baracuda.Monitoring.Source.Systems
{
    internal class MonitoringSystemDummy : 
        IMonitoringManager,
        IMonitoringUI, 
        IMonitoringUtility,
        IMonitoringLogger,
        IMonitoringManagerInternal, 
        IMonitoringUtilityInternal,
        IMonitoringTicker,
        IValueProcessorFactory,
        IValidatorFactory,
        IMonitoringProfiler
    {
        #region --- Pragma ---
        
#pragma warning disable
        
        #endregion
        
        #region --- MonitoringManager ---

        /// <summary>
        /// Value indicated whether or not monitoring profiling has completed and monitoring is fully initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public bool IsInitialized { get; } = false;

        /// <summary>
        /// Event is invoked when profiling process for the current system has been completed.
        /// Subscribing to this event will instantly invoke a callback if profiling has already completed.
        /// </summary>
        public event ProfilingCompletedListener ProfilingCompleted;

        /// <summary>
        /// Event is called when a new <see cref="MonitorUnit"/> was created.
        /// </summary>
        public event Action<IMonitorUnit> UnitCreated;

        /// <summary>
        /// Event is called when a <see cref="MonitorUnit"/> was disposed.
        /// </summary>
        public event Action<IMonitorUnit> UnitDisposed;

        /// <summary>
        /// Register an object that is monitored during runtime.
        /// </summary>
        public void RegisterTarget<T>(T target) where T : class
        {
        }

        /// <summary>
        /// Unregister an object that is monitored during runtime.
        /// </summary>
        public void UnregisterTarget<T>(T target) where T : class
        {
        }

        /// <summary>
        /// Get a list of monitoring units for static targets.
        /// </summary>
        public IReadOnlyList<IMonitorUnit> GetStaticUnits()
        {
            return Array.Empty<IMonitorUnit>();
        }

        /// <summary>
        /// Get a list of monitoring units for instance targets.
        /// </summary>
        public IReadOnlyList<IMonitorUnit> GetInstanceUnits()
        {
            return Array.Empty<IMonitorUnit>();
        }

        /// <summary>
        /// Get a list of all monitoring units.
        /// </summary>
        public IReadOnlyList<IMonitorUnit> GetAllMonitoringUnits()
        {
            return Array.Empty<IMonitorUnit>();
        }

        #endregion
        
        #region --- IMonitoringUI ---

        /// <summary>
        /// Set the active monitoring display visible.
        /// </summary>
        public void Show()
        {
        }

        /// <summary>
        /// Hide the active monitoring display.
        /// </summary>
        public void Hide()
        {
        }

        /// <summary>
        /// Toggle the visibility of the active monitoring display.
        /// This method returns a value indicating the new visibility state.
        /// </summary>
        public bool ToggleDisplay()
        {
            return default;
        }

        /// <summary>
        /// Event is invoked when the monitoring UI became visible/invisible
        /// </summary>
        public event Action<bool> VisibleStateChanged;

        /// <summary>
        /// Returns true if the there is an active monitoring display that is also visible.
        /// </summary>
        public bool IsVisible()
        {
            return default;
        }

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/>
        /// </summary>
        public MonitoringUIController GetActiveUIController()
        {
            return default;
        }

        /// <summary>
        /// Get the current <see cref="MonitoringUIController"/> as a concrete implementation of T.
        /// </summary>
        public TUIController GetActiveUIController<TUIController>() where TUIController : MonitoringUIController
        {
            return default;
        }

        /// <summary>
        /// Create a MonitoringUIController instance if there is none already. Disable 'Auto Instantiate UI' in the
        /// Monitoring Settings and use this method for more control over the timing in which the MonitoringUIController
        /// is instantiated.
        /// </summary>
        public void CreateMonitoringUI()
        {
        }

        /// <summary>
        /// ApplyFilter displayed units by their name, tags etc. 
        /// </summary>
        public void ApplyFilter(string filter)
        {
        }

        /// <summary>
        /// Reset active filter.
        /// </summary>
        public void ResetFilter()
        {
        }

        public void Filter(string filter)
        {
        }
        
        public void Initialize()
        {
            
        }
        
        #endregion

        #region --- IMonitoringUtility ---

        /// <summary>
        /// Method returns true if the passed hash from the name of a font asset is used by a MFontNameAttribute and therefore
        /// required by a monitoring unit. Used to dynamically load/unload required fonts.
        /// </summary>
        /// <param name="fontHash">The hash of the fonts name (string)</param>
        public bool IsFontHashUsed(int fontHash)
        {
            return default;
        }

        /// <summary>
        /// Get a list of <see cref="IMonitorUnit"/>s registered to the passed target object. 
        /// </summary>
        public IMonitorUnit[] GetMonitorUnitsForTarget(object target)
        {
            return Array.Empty<IMonitorUnit>();
        }

        /// <summary>
        /// Get a list of all custom tags, applied by [MTag] attributes that can be used for filtering. 
        /// </summary>
        public IReadOnlyCollection<string> GetAllTags()
        {
            return Array.Empty<string>();
        }

        public IReadOnlyCollection<string> GetAllTypeStrings()
        {
            return Array.Empty<string>();
        }

        #endregion
        
        /*
         * Internal Interfaces   
         */
        
        #region --- IMonitoringTicker ---

        public bool ValidationTickEnabled { get; set; }

        public void AddUpdateTicker(IMonitorUnit unit)
        {
        }

        public void RemoveUpdateTicker(IMonitorUnit unit)
        {
        }

        public void AddValidationTicker(Action tickAction)
        {
        }

        public void RemoveValidationTicker(Action tickAction)
        {
        }

        #endregion
        
        #region --- IMonitoringManagerInternal ---

        public Task CompleteProfilingAsync(List<MonitorProfile> staticProfiles, Dictionary<Type, List<MonitorProfile>> instanceProfiles, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
        
        #endregion
        
        #region --- IMonimtoringUtilityInternal ---

        public void AddFontHash(int fontHash)
        {
        }

        public void AddTag(string tag)
        {
        }

        public void AddTypeString(string typeString)
        {
        }

        #endregion

        #region --- IValueProcessorFactory ---

        /// <summary>
        /// Creates a default type specific processor to format the <see cref="TValue"/> depending on its exact type.
        /// </summary>
        /// <typeparam name="TValue">The type of the value that should be parsed/formatted</typeparam>
        /// <returns></returns>
        public Func<TValue, string> CreateProcessorForType<TValue>(IFormatData formatData)
        {
            return default;
        }

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="formatData"></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        public Func<TValue, string> FindCustomStaticProcessor<TTarget, TValue>(string processor, IFormatData formatData)
            where TTarget : class
        {
            return default;
        }

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="formatData"></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        public Func<TTarget, TValue, string> FindCustomInstanceProcessor<TTarget, TValue>(string processor,
            IFormatData formatData) where TTarget : class
        {
            return default;
        }

        /// <summary>
        /// Add a global value processor.
        /// </summary>
        /// <param name="methodInfo"></param>
        public void AddGlobalValueProcessor(MethodInfo methodInfo)
        {
        }

        #endregion
        
        #region --- IValidatorFactory ---

        public Func<bool> CreateStaticValidator(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return default;
        }

        public Func<TTarget, bool> CreateInstanceValidator<TTarget>(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return default;
        }

        public Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MShowIfAttribute attribute,
            MemberInfo memberInfo)
        {
            return default;
        }

        public ValidationEvent CreateEventValidator(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return default;
        }

        #endregion
        
        #region --- IMonitoringLogger ---

        public void LogException(Exception exception)
        {
        }

        public void LogBadImageFormatException(BadImageFormatException exception)
        {
        }

        public void LogThreadAbortedException(ThreadAbortException exception)
        {
        }

        public void LogOperationCancelledException(OperationCanceledException exception)
        {
        }

        public void LogValueProcessNotFound(string processor, Type type)
        {
        }

        public void LogInvalidProcessorSignature(string processor, Type type)
        {
        }

        #endregion

        #region --- IMonitoringProfiler ---

        public void BeginProfiling(CancellationToken ct)
        {
        }

        #endregion
    }
}