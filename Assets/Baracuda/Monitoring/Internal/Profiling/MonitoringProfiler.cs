using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Monitoring.Management;
using Baracuda.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <summary>
    /// Class responsible for creating <see cref="MonitorProfile"/>s for member found in custom assemblies that were
    /// flagged to be monitored by the use of a <see cref="MonitorAttribute"/>. 
    /// </summary>
    internal static class MonitoringProfiler
    {
        #region --- Fields ---

        internal static readonly List<MonitorProfile> StaticProfiles = new List<MonitorProfile>();

        internal static readonly Dictionary<Type, List<MonitorProfile>> InstanceProfiles =
            new Dictionary<Type, List<MonitorProfile>>();

        private static readonly List<(FieldInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            genericFieldBaseTypes = new List<(FieldInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();

        private static readonly List<(PropertyInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            genericPropertyBaseTypes = new List<(PropertyInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();

        private static readonly List<(EventInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            genericEventBaseTypes = new List<(EventInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();

        
        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static MonitoringSettings settings;
        
        private static readonly string[] bannedAssemblyPrefixes = new string[]
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

        private static readonly string[] bannedAssemblyNames = new string[]
        {
            "mcs",
            "AssetStoreTools",
            "PPv2URPConverters"
        };
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Profiling Task ---

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitializeRuntimeReflection()
        {
            Task.Run(() => InitializeProfilingAsync(Dispatcher.RuntimeToken), Dispatcher.RuntimeToken);
        }

        private static async Task InitializeProfilingAsync(CancellationToken ct)
        {
            try
            {
                settings = await Dispatcher.InvokeAsync(() => MonitoringSettings.Instance(), ct);
                
                var types = await CreateAssemblyProfileAsync(ct);
                await CreateMonitoringProfileAsync(types, ct);
            }
            catch (OperationCanceledException oce)
            {
                ExceptionLogging.LogException(oce, settings.LogOperationCanceledException);
            }
            catch (ThreadAbortException tae)
            {
                ExceptionLogging.LogException(tae, settings.LogThreadAbortException);
            }
            catch (Exception exception)
            {
                ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
            }
            finally
            {
                genericFieldBaseTypes.Clear();
                genericPropertyBaseTypes.Clear();
                genericEventBaseTypes.Clear();
            }
        }

        private static Task<Type[]> CreateAssemblyProfileAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var typeCache = new List<Type>(short.MaxValue);
            var assemblies =
                AssemblyManagement.GetFilteredAssemblies(
                    settings.BannedAssemblyNames,
                    settings.BannedAssemblyPrefixes);

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (assembly.HasAttribute<DisableMonitoringAttribute>())
                {
                    continue;
                }
                
                var types = assembly.GetTypes();
                
                for (var j = 0; j < types.Length; j++)
                {
                    var type = types[j];
                    if (type.HasAttribute<CompilerGeneratedAttribute>())
                    {
                        continue;
                    }

                    if (type.HasAttribute<DisableMonitoringAttribute>())
                    {
                        continue;
                    }

                    typeCache.Add(type);
                }
            }

            return Task.FromResult(typeCache.ToArray());
        }

        private static async Task CreateMonitoringProfileAsync(Type[] types, CancellationToken ct)
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
            if (genericFieldBaseTypes.Any())
            {
                postProfileAction += PostProfileGenericTypeFieldInfo;
            }

            if (genericPropertyBaseTypes.Any())
            {
                postProfileAction += PostProfileGenericTypePropertyInfo;
            }

            if (genericEventBaseTypes.Any())
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

        //--------------------------------------------------------------------------------------------------------------

        #region --- Instance: Inspection ---

        private static void InspectInstanceFields(FieldInfo[] fieldInfos)
        {
            for (var i = 0; i < fieldInfos.Length; i++)
            {
                try
                {
                    if (fieldInfos[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateInstanceFieldProfile(fieldInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, settings.LogBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        private static void InspectInstanceProperties(PropertyInfo[] propertyInfos)
        {
            for (var i = 0; i < propertyInfos.Length; i++)
            {
                try
                {
                    if (propertyInfos[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateInstancePropertyProfile(propertyInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, settings.LogBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        private static void InspectInstanceEvents(EventInfo[] eventInfos)
        {
            for (var i = 0; i < eventInfos.Length; i++)
            {
                try
                {
                    if (eventInfos[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateInstanceEventProfile(eventInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, settings.LogBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        #endregion

        //--------- 

        #region --- Instance: Profiling ---

        private static void CreateInstanceFieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (fieldInfo!.DeclaringType!.IsGenericType)
                {
                    genericFieldBaseTypes.Add((fieldInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType = 
#if ENALBE_IL2CPP && ALLOW_ALLOC
                    typeof(FieldProfile<object,object>);
#else
                    typeof(FieldProfile<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);
#endif

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, settings);

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

        private static void CreateInstancePropertyProfile(PropertyInfo propertyInfo, MonitorAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (propertyInfo!.DeclaringType!.IsGenericType)
                {
                    genericPropertyBaseTypes.Add((propertyInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType =
#if ENALBE_IL2CPP && ALLOW_ALLOC
                    typeof(PropertyProfile<object,object>);
#else
                    typeof(PropertyProfile<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
#endif

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, settings);

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

        private static void CreateInstanceEventProfile(EventInfo eventInfo, MonitorAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (eventInfo!.DeclaringType!.IsGenericType)
                {
                    genericEventBaseTypes.Add((eventInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType =
#if ENALBE_IL2CPP && ALLOW_ALLOC
                    typeof(EventProfile<object,object>);
#else
                    typeof(EventProfile<,>).MakeGenericType(eventInfo.DeclaringType, eventInfo.EventHandlerType);
#endif

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, settings);

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

        #region --- Instance: Profiling Generic Basetype Concretions ---

        // Methods in this region are called after every type has been profiled.
        // If a member of a generic type was flagged to be monitored, we have to again iterate over every type in the
        // solution, to see if we we have concrete subtypes of that generic type that we now can use as a definition, to
        // monitor the values of the generic base type.

        private static void CreateInstanceFieldProfileForGenericBaseType(FieldInfo fieldInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteFieldInfo = concreteSubtype.GetFieldIncludeBaseTypes(fieldInfo.Name, INSTANCE_FLAGS);

                // create a generic type definition.
                var concreteGenericType =
#if ENALBE_IL2CPP && ALLOW_ALLOC
                    typeof(FieldProfile<object,object>);
#else
                    typeof(FieldProfile<,>).MakeGenericType(concreteSubtype, concreteFieldInfo.FieldType);
#endif

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concreteFieldInfo, attribute,
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
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concretePropertyInfo = concreteSubtype.GetPropertyIncludeBaseTypes(propertyInfo.Name, INSTANCE_FLAGS);

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
#if ENALBE_IL2CPP && ALLOW_ALLOC
                    typeof(PropertyProfile<object,object>);
#else
                    typeof(PropertyProfile<,>).MakeGenericType(concreteSubtype, concretePropertyInfo.PropertyType);
#endif

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, settings);

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
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteEventInfo = concreteSubtype.GetEventIncludeBaseTypes(eventInfo.Name, INSTANCE_FLAGS);

                // create a generic type definition.
                var concreteGenericType =
#if ENALBE_IL2CPP && ALLOW_ALLOC
                    typeof(EventProfile<object,object>);
#else
                    typeof(EventProfile<,>).MakeGenericType(concreteSubtype, concreteEventInfo!.EventHandlerType);
#endif

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) InstanceFactory.CreateInstance(concreteGenericType, concreteEventInfo, attribute, args);

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

        #region --- Static: Inspection ---

        private static void InspectStaticFields(FieldInfo[] staticFields)
        {
            for (var i = 0; i < staticFields.Length; i++)
            {
                try
                {
                    if (staticFields[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateStaticFieldProfile(staticFields[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, settings.LogBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        private static void InspectStaticProperties(PropertyInfo[] staticProperties)
        {
            for (var i = 0; i < staticProperties.Length; i++)
            {
                try
                {
                    if (staticProperties[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateStaticPropertyProfile(staticProperties[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, settings.LogBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        private static void InspectStaticEvents(EventInfo[] staticEvents)
        {
            for (var i = 0; i < staticEvents.Length; i++)
            {
                try
                {
                    if (staticEvents[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateStaticEventProfile(staticEvents[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    ExceptionLogging.LogException(badImageFormatException, settings.LogBadImageFormatException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        #endregion

        //---------

        #region --- Static: Profiling ---

        private static void CreateStaticFieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (fieldInfo!.DeclaringType!.IsGenericType)
                {
                    genericFieldBaseTypes.Add((fieldInfo, attribute, true));
                    return;
                }

                // create a generic type definition.
                var genericType = typeof(FieldProfile<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, settings);

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

        private static void CreateStaticPropertyProfile(PropertyInfo propertyInfo, MonitorAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (propertyInfo!.DeclaringType!.IsGenericType)
                {
                    genericPropertyBaseTypes.Add((propertyInfo, attribute, true));
                    return;
                }

                var genericType = typeof(PropertyProfile<,>).MakeGenericType(propertyInfo.DeclaringType,
                    propertyInfo.GetMethod.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, settings);

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

        private static void CreateStaticEventProfile(EventInfo eventInfo, MonitorAttribute attribute)
        {
            try
            {
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (eventInfo!.DeclaringType!.IsGenericType)
                {
                    genericEventBaseTypes.Add((eventInfo, attribute, true));
                    return;
                }

                var genericType =
                    typeof(EventProfile<,>).MakeGenericType(eventInfo.DeclaringType, eventInfo.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, settings);

                var profile = (MonitorProfile) InstanceFactory.CreateInstance(genericType, eventInfo, attribute, args);
                StaticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        #region --- Static: Profiling Generic Basetype Concretions ---

        // Methods in this region are called after every type has been profiled.
        // If a member of a generic type was flagged to be monitored, we have to again iterate over every type in the
        // solution, to see if we we have concrete subtypes of that generic type that we now can use as a definition, to
        // monitor the values of the generic base type.

        private static void CreateStaticFieldProfileForGenericBaseType(FieldInfo fieldInfo,
            MonitorAttribute attribute, Type concreteSubtype)
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
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, settings);

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
            MonitorAttribute attribute, Type concreteSubtype)
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
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, settings);

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
            MonitorAttribute attribute, Type concreteSubtype)
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
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, settings);

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

        #region --- Post Profining ---

        private static void PostProfileGenericTypeFieldInfo(Type type)
        {
            foreach (var (fieldInfo, attribute, isStatic) in genericFieldBaseTypes)
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
            foreach (var (propertyInfo, attribute, isStatic) in genericPropertyBaseTypes)
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
            foreach (var (eventInfo, attribute, isStatic) in genericEventBaseTypes)
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

        #region --- Type Formatter ---

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
                    ExceptionLogging.LogException(exception, settings.LogBadImageFormatException);
                }
                catch (ThreadAbortException threadAbortException)
                {
                    ExceptionLogging.LogException(threadAbortException, settings.LogThreadAbortException);
                }
                catch (Exception exception)
                {
                    ExceptionLogging.LogException(exception, settings.LogUnknownExceptions);
                }
            }
        }

        #endregion
    }
}