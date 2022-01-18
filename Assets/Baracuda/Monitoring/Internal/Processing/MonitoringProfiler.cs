using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Attributes;
using Baracuda.Attributes.Monitoring;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Profiles;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Utils;
using Baracuda.Monitoring.Management;
using Baracuda.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Baracuda.Monitoring.Internal.Processing
{
    /// <summary>
    /// Class responsible for creating <see cref="MonitorProfile"/>s for member found in custom assemblies that were
    /// flagged to be monitored by the use of a <see cref="MonitorAttribute"/>. 
    /// </summary>
    internal static class MonitoringProfiler
    {
        #region --- [FIELDS] ---

        internal static readonly List<MonitorProfile> StaticProfiles = new List<MonitorProfile>();

        internal static readonly Dictionary<Type, List<MonitorProfile>> InstanceProfiles =
            new Dictionary<Type, List<MonitorProfile>>();

        private static readonly List<(FieldInfo fieldInfo, MonitorValueAttribute attribute, bool isStatic)>
            _genericFieldBaseTypes = new List<(FieldInfo fieldInfo, MonitorValueAttribute attribute, bool isStatic)>();

        private static readonly List<(PropertyInfo fieldInfo, MonitorValueAttribute attribute, bool isStatic)>
            _genericPropertyBaseTypes = new List<(PropertyInfo fieldInfo, MonitorValueAttribute attribute, bool isStatic)>();

        private static readonly List<(EventInfo fieldInfo, MonitorEventAttribute attribute, bool isStatic)>
            _genericEventBaseTypes = new List<(EventInfo fieldInfo, MonitorEventAttribute attribute, bool isStatic)>();

        
        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static MonitoringSettings _settings;
        
        private static readonly string[] _bannedAssemblyPrefixes = new string[]
        {
            "Newtonsoft",
            "netstandard",
            "System",
            "Unity",
            "Microsoft",
            "Mono.",
            "mscorlib",
            "NSubstitute",
            "JetBrains",
            "nunit.",
            "GeNa."
        };

        private static readonly string[] _bannedAssemblyNames = new string[]
        {
            "mcs",
            "AssetStoreTools",
            "PPv2URPConverters"
        };
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [PROFILING TASK] ---

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitializeRuntimeReflection()
            => Task.Run(() => InitializeProfilingAsync(Dispatcher.RuntimeToken), Dispatcher.RuntimeToken);

        private static async Task InitializeProfilingAsync(CancellationToken ct)
        {
            try
            {
                _settings = await Dispatcher.InvokeAsync(MonitoringSettings.Instance, ct);
                
                var types = await CreateAssemblyProfile(ct);
                await CreateMonitoringProfile(types, ct);
            }
            catch (OperationCanceledException oce)
            {
                ExceptionLogging.LogException(oce, _settings.logOperationCanceledException);
            }
            catch (ThreadAbortException tae)
            {
                ExceptionLogging.LogException(tae, _settings.logThreadAbortException);
            }
            catch (Exception exception)
            {
                ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
            }
            finally
            {
                _genericFieldBaseTypes.Clear();
                _genericPropertyBaseTypes.Clear();
                _genericEventBaseTypes.Clear();
            }
        }

        private static Task<Type[]> CreateAssemblyProfile(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var typeCache = new List<Type>(short.MaxValue);
            var assemblies = GetFilteredAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var types = assembly.GetTypes();
                for (var j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type.HasAttribute<CompilerGeneratedAttribute>()) continue;
                    if (type.HasAttribute<DisableMonitoringAttribute>()) continue;
                    typeCache.Add(type);
                }
            }

            return Task.FromResult(typeCache.ToArray());
        }

        private static async Task CreateMonitoringProfile(Type[] types, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            // inspect static member
            for (var i = 0; i < types.Length; i++)
            {
                var staticFields = types[i].GetFields(STATIC_FLAGS);
                var staticProperties = types[i].GetProperties(STATIC_FLAGS);
                var staticEvents = types[i].GetEvents(STATIC_FLAGS);

                ProfileDefaultTypeFormatter(staticFields);
                InspectStaticFields(staticFields);
                InspectStaticProperties(staticProperties);
                InspectStaticEvents(staticEvents);
            }

            ct.ThrowIfCancellationRequested();

            // inspect instance member
            for (var i = 0; i < types.Length; i++)
            {
                var instanceFields = types[i].GetFields(INSTANCE_FLAGS);
                var instanceProperties = types[i].GetProperties(INSTANCE_FLAGS);
                var instanceEvents = types[i].GetEvents(INSTANCE_FLAGS);

                InspectInstanceFields(instanceFields);
                InspectInstanceProperties(instanceProperties);
                InspectInstanceEvents(instanceEvents);
            }

            ct.ThrowIfCancellationRequested();


            // post profile to find concrete implementations of cached generic base types that contain members
            // flagged to be monitored. 
            var postProfileAction = default(Action<Type>);
            if (_genericFieldBaseTypes.Any())
            {
                postProfileAction += PostProfileGenericTypeFieldInfo;
            }

            if (_genericPropertyBaseTypes.Any())
            {
                postProfileAction += PostProfileGenericTypePropertyInfo;
            }

            if (_genericEventBaseTypes.Any())
            {
                postProfileAction += PostProfileGenericTypeEventInfo;
            }

            if (postProfileAction != null)
            {
                for (var i = 0; i < types.Length; i++)
                {
                    postProfileAction(types[i]);
                }
            }

            ct.ThrowIfCancellationRequested();

            await MonitoringManager.CompleteProfiling(ct);
        }

        #endregion

        #region --- [ASSEMBLY REFLECTION] ---

        /// <summary>
        /// Method will initialize and filter all available assemblies only leaving custom unity assemblies.
        /// </summary>
        private static Assembly[] GetFilteredAssemblies()
        {
            var filteredAssemblies = new List<Assembly>(30);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                if (AssemblyReflectionRequired(assemblies[i]))
                {
                    filteredAssemblies.Add(assemblies[i]);
                }
            }
            return filteredAssemblies.ToArray();
        }
        
        
        private static bool AssemblyReflectionRequired(Assembly assembly)
        {
            Debug.Assert((bool) _settings, "Settings should be loaded!");
            
            if (assembly.HasAttribute<DisableMonitoringAttribute>())
            {
                return false;
            }

            var assemblyFullName = assembly.FullName;
            for (var i = 0; i < _bannedAssemblyPrefixes.Length; i++)
            {
                var prefix = _bannedAssemblyPrefixes[i];
                if (assemblyFullName.StartsWith(prefix))
                {
                    return false;
                }
            }
            for (var i = 0; i < _settings.BannedAssemblyPrefixes.Length; i++)
            {
                var prefix = _settings.BannedAssemblyPrefixes[i];
                if (assemblyFullName.StartsWith(prefix))
                {
                    return false;
                }
            }

            var assemblyShortName = assembly.GetName().Name;
            for (var i = 0; i < _bannedAssemblyNames.Length; i++)
            {
                var name = _bannedAssemblyNames[i];
                if (assemblyShortName == name)
                {
                    return false;
                }
            }
            for (var i = 0; i < _settings.BannedAssemblyNames.Length; i++)
            {
                var name = _settings.BannedAssemblyNames[i];
                if (assemblyShortName == name)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [INSTANCE: INSPECTION] ---

        private static void InspectInstanceFields(FieldInfo[] fieldInfos)
        {
            for (var i = 0; i < fieldInfos.Length; i++)
            {
                try
                {
                    if (fieldInfos[i].TryGetCustomAttribute<MonitorValueAttribute>(out var attribute, true))
                    {
                        CreateInstanceFieldProfile(fieldInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, _settings.logBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        private static void InspectInstanceProperties(PropertyInfo[] propertyInfos)
        {
            for (var i = 0; i < propertyInfos.Length; i++)
            {
                try
                {
                    if (propertyInfos[i].TryGetCustomAttribute<MonitorValueAttribute>(out var attribute, true))
                    {
                        CreateInstancePropertyProfile(propertyInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, _settings.logBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        private static void InspectInstanceEvents(EventInfo[] eventInfos)
        {
            for (var i = 0; i < eventInfos.Length; i++)
            {
                try
                {
                    if (eventInfos[i].TryGetCustomAttribute<MonitorEventAttribute>(out var attribute, true))
                    {
                        CreateInstanceEventProfile(eventInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, _settings.logBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        #endregion

        //--------- 

        #region --- [INSTANCE: PROFILING] ---

        private static void CreateInstanceFieldProfile(FieldInfo fieldInfo, MonitorValueAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (fieldInfo!.DeclaringType!.IsGenericType)
                {
                    _genericFieldBaseTypes.Add((fieldInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType = typeof(FieldProfile<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) InstanceFactory.CreateInstance(genericType, fieldInfo, attribute, args);

                // cache the profile
                if (InstanceProfiles.TryGetValue(fieldInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    InstanceProfiles.Add(fieldInfo.DeclaringType, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void CreateInstancePropertyProfile(PropertyInfo propertyInfo, MonitorValueAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (propertyInfo!.DeclaringType!.IsGenericType)
                {
                    _genericPropertyBaseTypes.Add((propertyInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType =
                    typeof(PropertyProfile<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS);

                // create a profile for the property using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(genericType, propertyInfo, attribute, args);

                // cache the profile
                if (InstanceProfiles.TryGetValue(propertyInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    InstanceProfiles.Add(propertyInfo.DeclaringType, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void CreateInstanceEventProfile(EventInfo eventInfo, MonitorEventAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (eventInfo!.DeclaringType!.IsGenericType)
                {
                    _genericEventBaseTypes.Add((eventInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType =
                    typeof(EventProfile<,>).MakeGenericType(eventInfo.DeclaringType, eventInfo.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS);

                // create a profile for the event. First parameter is the generic type definition.
                var profile = (MonitorProfile) InstanceFactory.CreateInstance(genericType, eventInfo, attribute, args);

                // cache the profile
                if (InstanceProfiles.TryGetValue(eventInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    InstanceProfiles.Add(eventInfo.DeclaringType, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        #region --- [INSTANCE: PROFILING GENERIC BASETYPE CONCRETIONS] ---

        // Methods in this region are called after every type has been profiled.
        // If a member of a generic type was flagged to be monitored, we have to again iterate over every type in the
        // solution, to see if we we have concrete subtypes of that generic type that we now can use as a definition, to
        // monitor the values of the generic base type.

        private static void CreateInstanceFieldProfileForGenericBaseType(FieldInfo fieldInfo,
            MonitorValueAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteFieldInfo = concreteSubtype.GetFieldInBaseType(fieldInfo.Name, INSTANCE_FLAGS);

                // create a generic type definition.
                var concreteGenericType =
                    typeof(FieldProfile<,>).MakeGenericType(concreteSubtype, concreteFieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concreteFieldInfo, attribute,
                        args);

                // cache the profile
                if (InstanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    InstanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }


        private static void CreateInstancePropertyProfileForGenericBaseType(PropertyInfo propertyInfo,
            MonitorValueAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concretePropertyInfo = concreteSubtype.GetPropertyInBaseType(propertyInfo.Name, INSTANCE_FLAGS);

                // REVIEW: 
                // If an object is registered that inherits from multiple generic types, multiple profiles with the same
                // property are used by this object. I think that this might be fixed either at this point, by creating a guid based on
                // property, base types etc. or when registering targets by double checking profiles.
                // I will admit that this might take some try and error to fix.
                // Edit1: The problem is fixed for now by double checking the attributes of MonitoringProfiles when creating
                // an instance of a unit and disallowing the same attribute to be used twice. This is not a very elegant
                // solution but it works.
                // Edit2: using the attribute to compare had some issues that prevented this method from being accurate. 
                // Now the memberInfo of the profiles are being compared with success (so far)

                // create a generic type definition.
                var concreteGenericType =
                    typeof(PropertyProfile<,>).MakeGenericType(concreteSubtype, concretePropertyInfo.PropertyType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS);

                // create a profile for the property using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concretePropertyInfo,
                        attribute, args);

                // cache the profile
                if (InstanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    InstanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void CreateInstanceEventProfileForGenericBaseType(EventInfo eventInfo,
            MonitorEventAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteEventInfo = concreteSubtype.GetEventInBaseType(eventInfo.Name, INSTANCE_FLAGS);

                // create a generic type definition.
                var concreteGenericType =
                    typeof(EventProfile<,>).MakeGenericType(concreteSubtype, concreteEventInfo!.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concreteEventInfo, attribute,
                        args);

                // cache the profile
                if (InstanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    InstanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [STATIC: INSPECTION] ---

        private static void InspectStaticFields(FieldInfo[] staticFields)
        {
            for (var i = 0; i < staticFields.Length; i++)
            {
                try
                {
                    if (staticFields[i].TryGetCustomAttribute<MonitorValueAttribute>(out var attribute, true))
                    {
                        CreateStaticFieldProfile(staticFields[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, _settings.logBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        private static void InspectStaticProperties(PropertyInfo[] staticProperties)
        {
            for (var i = 0; i < staticProperties.Length; i++)
            {
                try
                {
                    if (staticProperties[i].TryGetCustomAttribute<MonitorValueAttribute>(out var attribute, true))
                    {
                        CreateStaticPropertyProfile(staticProperties[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, _settings.logBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        private static void InspectStaticEvents(EventInfo[] staticEvents)
        {
            for (var i = 0; i < staticEvents.Length; i++)
            {
                try
                {
                    if (staticEvents[i].TryGetCustomAttribute<MonitorEventAttribute>(out var attribute, true))
                    {
                        CreateStaticEventProfile(staticEvents[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, _settings.logBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        #endregion

        //---------

        #region --- [STATIC: PROFILING] ---

        private static void CreateStaticFieldProfile(FieldInfo fieldInfo, MonitorValueAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (fieldInfo!.DeclaringType!.IsGenericType)
                {
                    _genericFieldBaseTypes.Add((fieldInfo, attribute, true));
                    return;
                }

                // create a generic type definition.
                var genericType = typeof(FieldProfile<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) InstanceFactory.CreateInstance(genericType, fieldInfo, attribute, args);

                // cache the profile and create an instance of with the static profile.
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void CreateStaticPropertyProfile(PropertyInfo propertyInfo, MonitorValueAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (propertyInfo!.DeclaringType!.IsGenericType)
                {
                    _genericPropertyBaseTypes.Add((propertyInfo, attribute, true));
                    return;
                }

                var genericType = typeof(PropertyProfile<,>).MakeGenericType(propertyInfo.DeclaringType,
                    propertyInfo.GetMethod.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS);

                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(genericType, propertyInfo, attribute, args);
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void CreateStaticEventProfile(EventInfo eventInfo, MonitorEventAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (eventInfo!.DeclaringType!.IsGenericType)
                {
                    _genericEventBaseTypes.Add((eventInfo, attribute, true));
                    return;
                }

                var genericType =
                    typeof(EventProfile<,>).MakeGenericType(eventInfo.DeclaringType, eventInfo.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS);

                var profile = (MonitorProfile) InstanceFactory.CreateInstance(genericType, eventInfo, attribute, args);
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        #region --- [STATIC: PROFILING GENERIC BASETYPE CONCRETIONS] ---

        // Methods in this region are called after every type has been profiled.
        // If a member of a generic type was flagged to be monitored, we have to again iterate over every type in the
        // solution, to see if we we have concrete subtypes of that generic type that we now can use as a definition, to
        // monitor the values of the generic base type.

        private static void CreateStaticFieldProfileForGenericBaseType(FieldInfo fieldInfo,
            MonitorValueAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                var concreteFieldInfo = concreteBaseType!.GetField(fieldInfo.Name, STATIC_FLAGS);

                // create a generic type definition.
                var concreteGenericType =
                    typeof(FieldProfile<,>).MakeGenericType(concreteSubtype, concreteFieldInfo!.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concreteFieldInfo, attribute,
                        args);

                // cache the profile and create an instance of with the static profile.
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void CreateStaticPropertyProfileForGenericBaseType(PropertyInfo propertyInfo,
            MonitorValueAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                var concretePropertyInfo = concreteBaseType!.GetProperty(propertyInfo.Name, STATIC_FLAGS);

                // create a generic type definition.
                var concreteGenericType =
                    typeof(PropertyProfile<,>).MakeGenericType(concreteSubtype,
                        concretePropertyInfo!.GetMethod.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concretePropertyInfo,
                        attribute, args);

                // cache the profile and create an instance of with the static profile.
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private static void CreateStaticEventProfileForGenericBaseType(EventInfo eventInfo,
            MonitorEventAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                var concreteEventInfo = concreteBaseType!.GetEvent(eventInfo.Name, STATIC_FLAGS);


                // create a generic type definition.
                var concreteGenericType =
                    typeof(EventProfile<,>).MakeGenericType(concreteSubtype, concreteEventInfo!.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concreteEventInfo, attribute,
                        args);

                // cache the profile and create an instance of with the static profile.
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [POST PROFINING] ---

        private static void PostProfileGenericTypeFieldInfo(Type type)
        {
            foreach (var (fieldInfo, attribute, isStatic) in _genericFieldBaseTypes)
            {
                if (!type.IsGenericType && type.IsSubclassOfRawGeneric(fieldInfo.DeclaringType, false))
                {
                    if (isStatic)
                    {
                        CreateStaticFieldProfileForGenericBaseType(fieldInfo, attribute, type);
                    }
                    else
                    {
                        CreateInstanceFieldProfileForGenericBaseType(fieldInfo, attribute, type);
                    }
                }
            }
        }

        private static void PostProfileGenericTypePropertyInfo(Type type)
        {
            foreach (var (propertyInfo, attribute, isStatic) in _genericPropertyBaseTypes)
            {
                if (!type.IsGenericType && type.IsSubclassOfRawGeneric(propertyInfo.DeclaringType, false))
                {
                    if (isStatic)
                    {
                        CreateStaticPropertyProfileForGenericBaseType(propertyInfo, attribute, type);
                    }
                    else
                    {
                        CreateInstancePropertyProfileForGenericBaseType(propertyInfo, attribute, type);
                    }
                }
            }
        }

        private static void PostProfileGenericTypeEventInfo(Type type)
        {
            foreach (var (eventInfo, attribute, isStatic) in _genericEventBaseTypes)
            {
                if (!type.IsGenericType && type.IsSubclassOfRawGeneric(eventInfo.DeclaringType, false))
                {
                    if (isStatic)
                    {
                        CreateStaticEventProfileForGenericBaseType(eventInfo, attribute, type);
                    }
                    else
                    {
                        CreateInstanceEventProfileForGenericBaseType(eventInfo, attribute, type);
                    }
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [TYPE FORMATTER] ---

        internal static readonly Dictionary<Type, string> DefaultTypeFormatter = new Dictionary<Type, string>();

        private static void ProfileDefaultTypeFormatter(FieldInfo[] staticFieldInfos)
        {
            for (var i = 0; i < staticFieldInfos.Length; i++)
            {
                try
                {
                    if (staticFieldInfos[i]
                        .TryGetCustomAttribute<DefaultTypeFormatterAttribute>(out var attribute, true))
                    {
                        if (staticFieldInfos[i].FieldType == typeof(string))
                        {
                            if (!DefaultTypeFormatter.ContainsKey(attribute.Type))
                            {
                                DefaultTypeFormatter.Add(attribute.Type, staticFieldInfos[i].GetValue(null).ToString());
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Only String Is Valid DefaultTypeFormatter");
                        }
                    }
                }
                catch (BadImageFormatException exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logBadImageFormatException);
                }
                catch (ThreadAbortException threadAbortException)
                {
                    ExceptionLogging.LogException(threadAbortException, _settings.logThreadAbortException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, _settings.logUnknownExceptions);
                }
            }
        }

        #endregion
    }
}