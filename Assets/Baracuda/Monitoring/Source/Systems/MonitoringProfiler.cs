// Copyright (c) 2022 Jonathan Lang

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Profiles;
using Baracuda.Monitoring.Source.Types;
using Baracuda.Threading;
using Baracuda.Utilities.Extensions;
using Baracuda.Utilities.Reflection;
using Debug = UnityEngine.Debug;

namespace Baracuda.Monitoring.Source.Systems
{
    /// <summary>
    /// Class responsible for creating <see cref="MonitorProfile"/>s for member found in custom assemblies that were
    /// flagged to be monitored by the use of a <see cref="MonitorAttribute"/>. 
    /// </summary>
    internal class MonitoringProfiler : IMonitoringProfiler
    {
        #region --- Fields ---

        /*
         * Internal   
         */
        
        private readonly List<MonitorProfile> _staticProfiles = new List<MonitorProfile>();
        private readonly Dictionary<Type, List<MonitorProfile>> _instanceProfiles = new Dictionary<Type, List<MonitorProfile>>();

        /*
         * Private   
         */
        
        private readonly List<(FieldInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            _genericFieldBaseTypes = new List<(FieldInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();

        private readonly List<(PropertyInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            _genericPropertyBaseTypes = new List<(PropertyInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();

        private readonly List<(EventInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            _genericEventBaseTypes = new List<(EventInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();
        
        private readonly List<(MethodInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>
            _genericMethodBaseTypes = new List<(MethodInfo fieldInfo, MonitorAttribute attribute, bool isStatic)>();

        
        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private IMonitoringSettings _settings;
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Profiling Task ---

        public void BeginProfiling(CancellationToken ct)
        {
            _settings = MonitoringSystems.Resolve<IMonitoringSettings>();
            
            if (_settings.AsyncProfiling)
            {
                Task.Run(() => BeginProfilingAsync(ct), ct);
            }
            else
            {
                BeginProfilingAsync(ct).Wait(ct);
            }
        }
        
        private async Task BeginProfilingAsync(CancellationToken ct)
        {
            try
            {
                var types = CreateAssemblyProfile(ct);
                CreateMonitoringProfile(types, ct);
                await MonitoringSystems.Resolve<IMonitoringManagerInternal>().CompleteProfilingAsync(_staticProfiles, _instanceProfiles, ct);
            }
            catch (OperationCanceledException oce)
            {
                MonitoringSystems.Resolve<IMonitoringLogger>().LogOperationCancelledException(oce);
            }
            catch (ThreadAbortException tae)
            {
                MonitoringSystems.Resolve<IMonitoringLogger>().LogThreadAbortedException(tae);
            }
            catch (Exception exception)
            {
                MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
            }
            finally
            {
                _genericFieldBaseTypes.Clear();
                _genericPropertyBaseTypes.Clear();
                _genericEventBaseTypes.Clear();
                _genericMethodBaseTypes.Clear();
            }
        }

        /*
         * Assembly & Profiling   
         */

        private Type[] CreateAssemblyProfile(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var typeCache = new List<Type>(short.MaxValue);
            var assemblies =
                AssemblyProfiler.GetFilteredAssemblies(
                    _settings.BannedAssemblyNames,
                    _settings.BannedAssemblyPrefixes);

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

            return typeCache.ToArray();
        }
        
        private void CreateMonitoringProfile(Type[] types, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            // search for global value processors
            for (var i = 0; i < types.Length; i++)
            {
                var methods = types[i].GetMethods(STATIC_FLAGS);
                for (var j = 0; j < methods.Length; j++)
                {
                    if (methods[j].HasAttribute<GlobalValueProcessor>())
                    {
                        MonitoringSystems.Resolve<IValueProcessorFactory>().AddGlobalValueProcessor(methods[j]);
                    }
                }
            }
            
            // inspect static member
            for (var i = 0; i < types.Length; i++)
            {
                var staticFields = types[i].GetFields(STATIC_FLAGS);
                var staticProperties = types[i].GetProperties(STATIC_FLAGS);
                var staticEvents = types[i].GetEvents(STATIC_FLAGS);
                var staticMethods = types[i].GetMethods(STATIC_FLAGS);
            
                InspectStaticFields(staticFields);
                InspectStaticProperties(staticProperties);
                InspectStaticEvents(staticEvents);
                InspectStaticMethods(staticMethods);
            }
            
            ct.ThrowIfCancellationRequested();
            
            // inspect instance member
            for (var i = 0; i < types.Length; i++)
            {
                var instanceFields = types[i].GetFields(INSTANCE_FLAGS);
                var instanceProperties = types[i].GetProperties(INSTANCE_FLAGS);
                var instanceEvents = types[i].GetEvents(INSTANCE_FLAGS);
                var instanceMethods = types[i].GetMethods(INSTANCE_FLAGS);
                
                InspectInstanceFields(instanceFields);
                InspectInstanceProperties(instanceProperties);
                InspectInstanceEvents(instanceEvents);
                InspectInstanceMethods(instanceMethods);
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

            if (_genericMethodBaseTypes.Any())
            {
                postProfileAction += PostProfileGenericTypeMethodInfo;
            }
            
            if (postProfileAction != null)
            {
                for (var i = 0; i < types.Length; i++)
                {
                    postProfileAction(types[i]);
                }
            }
            
            ct.ThrowIfCancellationRequested();
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Instance: Inspection ---

        private void InspectInstanceFields(FieldInfo[] fieldInfos)
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
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }

        private void InspectInstanceProperties(PropertyInfo[] propertyInfos)
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
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }

        private void InspectInstanceEvents(EventInfo[] eventInfos)
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
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }
        
        private void InspectInstanceMethods(MethodInfo[] methodInfos)
        {
            for (var i = 0; i < methodInfos.Length; i++)
            {
                try
                {
                    if (methodInfos[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateInstanceMethodProfile(methodInfos[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }

        #endregion

        //--------- 

        #region --- Instance: Profiling ---

        private void CreateInstanceFieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(fieldInfo.DeclaringType != null, "fieldInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (fieldInfo.DeclaringType.IsGenericType)
                {
                    _genericFieldBaseTypes.Add((fieldInfo, attribute, false));
                    return;
                }
                
                // create a generic type definition.
                var genericType = typeof(FieldProfile<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(genericType, fieldInfo, attribute, args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(fieldInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(fieldInfo.DeclaringType, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
        
        private void CreateInstancePropertyProfile(PropertyInfo propertyInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(propertyInfo.DeclaringType != null, "propertyInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (propertyInfo.DeclaringType.IsGenericType)
                {
                    _genericPropertyBaseTypes.Add((propertyInfo, attribute, false));
                    return;
                }
                
                // create a generic type definition.
                var genericType = typeof(PropertyProfile<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);


                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the property using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(genericType, propertyInfo, attribute, args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(propertyInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(propertyInfo.DeclaringType, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void CreateInstanceEventProfile(EventInfo eventInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(eventInfo.DeclaringType != null, "eventInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (eventInfo.DeclaringType.IsGenericType)
                {
                    _genericEventBaseTypes.Add((eventInfo, attribute, false));
                    return;
                }

                // create a generic type definition.
                var genericType = typeof(EventProfile<,>).MakeGenericType(eventInfo.DeclaringType, eventInfo.EventHandlerType);


                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the event. First parameter is the generic type definition.
                var profile = (MonitorProfile) CreateInstance(genericType, eventInfo, attribute, args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(eventInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(eventInfo.DeclaringType, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
        
        private void CreateInstanceMethodProfile(MethodInfo methodInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(methodInfo.DeclaringType != null, "methodInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (methodInfo.DeclaringType.IsGenericType)
                {
                    _genericMethodBaseTypes.Add((methodInfo, attribute, false));
                    return;
                }

                if (!methodInfo.HasReturnValueOrOutParameter())
                {
                    Debug.LogWarning($"Monitored Method {methodInfo.DeclaringType?.Name}.{methodInfo.Name} needs a return value or out parameter!");
                    return;
                }
                
                // create a generic type definition.
                var genericType = typeof(MethodProfile<,>).MakeGenericType(methodInfo.DeclaringType, methodInfo.ReturnType.NotVoid(typeof(VoidValue)));


                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the property using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(genericType, methodInfo, attribute, args);

                // cache the profile
                // ReSharper disable once AssignNullToNotNullAttribute
                if (_instanceProfiles.TryGetValue(methodInfo.DeclaringType, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(methodInfo.DeclaringType, new List<MonitorProfile> {profile});
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

        private void CreateInstanceFieldProfileForGenericBaseType(FieldInfo fieldInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteFieldInfo = concreteSubtype.GetFieldIncludeBaseTypes(fieldInfo.Name, INSTANCE_FLAGS);

                // create a generic type definition.
                var concreteGenericType = typeof(FieldProfile<,>).MakeGenericType(concreteSubtype, concreteFieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(concreteGenericType, concreteFieldInfo, attribute,
                        args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }


        private void CreateInstancePropertyProfileForGenericBaseType(PropertyInfo propertyInfo,
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
                    typeof(PropertyProfile<,>).MakeGenericType(concreteSubtype, concretePropertyInfo.PropertyType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the property using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) CreateInstance(concreteGenericType, concretePropertyInfo,
                        attribute, args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void CreateInstanceEventProfileForGenericBaseType(EventInfo eventInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteEventInfo = concreteSubtype.GetEventIncludeBaseTypes(eventInfo.Name, INSTANCE_FLAGS);

                // create a generic type definition.
                var concreteGenericType = typeof(EventProfile<,>).MakeGenericType(concreteSubtype, concreteEventInfo.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) CreateInstance(concreteGenericType, concreteEventInfo, attribute, args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
        
        private void CreateInstanceMethodProfileForGenericBaseType(MethodInfo methodInfo, MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteMethodInfo = concreteSubtype.GetMethodIncludeBaseTypes(methodInfo.Name, INSTANCE_FLAGS);

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
                    typeof(MethodProfile<,>).MakeGenericType(concreteSubtype, concreteMethodInfo.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(INSTANCE_FLAGS, _settings);

                // create a profile for the property using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(concreteGenericType, concreteMethodInfo, attribute, args);

                // cache the profile
                if (_instanceProfiles.TryGetValue(concreteSubtype, out var profiles))
                {
                    profiles.Add(profile);
                }
                else
                {
                    // create a new entry using the declaring type as key.
                    _instanceProfiles.Add(concreteSubtype, new List<MonitorProfile> {profile});
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        #region --- Profiling Helper ---
        
        private object CreateInstance<T1, T2, T3>(Type type, T1 arg1, T2 arg2, T3 arg3)
        {
            var ctorArray = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            return ctorArray[0].Invoke(new object[] {arg1, arg2, arg3});
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Static: Inspection ---

        private void InspectStaticFields(FieldInfo[] staticFields)
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
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }

        private void InspectStaticProperties(PropertyInfo[] staticProperties)
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
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }

        private void InspectStaticEvents(EventInfo[] staticEvents)
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
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }
        
        private void InspectStaticMethods(MethodInfo[] staticMethods)
        {
            for (var i = 0; i < staticMethods.Length; i++)
            {
                try
                {
                    if (staticMethods[i].TryGetCustomAttribute<MonitorAttribute>(out var attribute, true))
                    {
                        CreateStaticMethodProfile(staticMethods[i], attribute);
                    }
                }
                catch (BadImageFormatException badImageFormatException)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogBadImageFormatException(badImageFormatException);
                }
                catch (Exception exception)
                {
                    MonitoringSystems.Resolve<IMonitoringLogger>().LogException(exception);
                }
            }
        }

        #endregion

        //---------

        #region --- Static: Profiling ---

        private void CreateStaticFieldProfile(FieldInfo fieldInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(fieldInfo.DeclaringType != null, "fieldInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (fieldInfo.DeclaringType.IsGenericType)
                {
                    _genericFieldBaseTypes.Add((fieldInfo, attribute, true));
                    return;
                }

                // create a generic type definition.
                var genericType = typeof(FieldProfile<,>).MakeGenericType(fieldInfo.DeclaringType, fieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(genericType, fieldInfo, attribute, args);

                // cache the profile and create an instance of with the static profile.
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void CreateStaticPropertyProfile(PropertyInfo propertyInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(propertyInfo.DeclaringType != null, "propertyInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (propertyInfo.DeclaringType.IsGenericType)
                {
                    _genericPropertyBaseTypes.Add((propertyInfo, attribute, true));
                    return;
                }

                var genericType = typeof(PropertyProfile<,>).MakeGenericType(propertyInfo.DeclaringType,
                    propertyInfo.GetMethod.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                var profile = (MonitorProfile) CreateInstance(genericType, propertyInfo, attribute, args);
                
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void CreateStaticEventProfile(EventInfo eventInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(eventInfo.DeclaringType != null, "eventInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (eventInfo.DeclaringType.IsGenericType)
                {
                    _genericEventBaseTypes.Add((eventInfo, attribute, true));
                    return;
                }

                var genericType =
                    typeof(EventProfile<,>).MakeGenericType(eventInfo.DeclaringType, eventInfo.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                var profile = (MonitorProfile) CreateInstance(genericType, eventInfo, attribute, args);
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------
        
        private void CreateStaticMethodProfile(MethodInfo methodInfo, MonitorAttribute attribute)
        {
            try
            {
                Debug.Assert(methodInfo.DeclaringType != null, "methodInfo.DeclaringType != null");
                
                // we cannot construct an object based on a generic type definition without having a concrete
                // subtype as a template which is the reason why we are storing this profile in a special list and 
                // instantiate it for each subtype we find.
                if (methodInfo.DeclaringType.IsGenericType)
                {
                    _genericMethodBaseTypes.Add((methodInfo, attribute, true));
                    return;
                }
                
                if (!methodInfo.HasReturnValueOrOutParameter())
                {
                    Debug.LogWarning($"Monitored Method {methodInfo.DeclaringType?.Name}.{methodInfo.Name} needs a return value or out parameter!");
                    return;
                }

                var genericType =
                    typeof(MethodProfile<,>).MakeGenericType(methodInfo.DeclaringType, methodInfo.ReturnType.NotVoid(typeof(VoidValue)));

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                var profile = (MonitorProfile) CreateInstance(genericType, methodInfo, attribute, args);
                _staticProfiles.Add(profile);
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

        private void CreateStaticFieldProfileForGenericBaseType(FieldInfo fieldInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                
                Debug.Assert(concreteBaseType != null, nameof(concreteBaseType) + " != null");
                
                var concreteFieldInfo = concreteBaseType.GetField(fieldInfo.Name, STATIC_FLAGS);
                
                Debug.Assert(concreteFieldInfo != null, nameof(concreteFieldInfo) + " != null");

                // create a generic type definition.
                var concreteGenericType =
                    typeof(FieldProfile<,>).MakeGenericType(concreteSubtype, concreteFieldInfo.FieldType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile =
                    (MonitorProfile) CreateInstance(concreteGenericType, concreteFieldInfo, attribute,
                        args);

                // cache the profile and create an instance of with the static profile.
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void CreateStaticPropertyProfileForGenericBaseType(PropertyInfo propertyInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                
                Debug.Assert(concreteBaseType != null, nameof(concreteBaseType) + " != null");
                
                var concretePropertyInfo = concreteBaseType.GetProperty(propertyInfo.Name, STATIC_FLAGS);
                
                Debug.Assert(concretePropertyInfo != null, nameof(concretePropertyInfo) + " != null");

                // create a generic type definition.
                var concreteGenericType = typeof(PropertyProfile<,>).MakeGenericType(concreteSubtype, concretePropertyInfo.GetMethod.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(concreteGenericType, concretePropertyInfo, attribute, args);

                // cache the profile and create an instance of with the static profile.
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        private void CreateStaticEventProfileForGenericBaseType(EventInfo eventInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                
                Debug.Assert(concreteBaseType != null, nameof(concreteBaseType) + " != null");
                
                var concreteEventInfo = concreteBaseType.GetEvent(eventInfo.Name, STATIC_FLAGS);
                
                Debug.Assert(concreteEventInfo != null, nameof(concreteEventInfo) + " != null");
                
                // create a generic type definition.
                var concreteGenericType = typeof(EventProfile<,>).MakeGenericType(concreteSubtype, concreteEventInfo.EventHandlerType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(concreteGenericType, concreteEventInfo, attribute, args);

                // cache the profile and create an instance of with the static profile.
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        //--------------------------------------------------------------------------------------------------------------
        
        private void CreateStaticMethodProfileForGenericBaseType(MethodInfo methodInfo,
            MonitorAttribute attribute, Type concreteSubtype)
        {
            try
            {
                // everything must be concrete when creating the generic method/ctor bellow so we have to use
                // magic by creating a concrete type, based on a concrete subtype of a generic base type.
                var concreteBaseType = concreteSubtype.BaseType;
                
                Debug.Assert(concreteBaseType != null, nameof(concreteBaseType) + " != null");
                
                var concreteMethodInfo = concreteBaseType.GetMethod(methodInfo.Name, STATIC_FLAGS);
                
                Debug.Assert(concreteMethodInfo != null, nameof(concreteMethodInfo) + " != null");

                // create a generic type definition.
                var concreteGenericType = typeof(MethodProfile<,>).MakeGenericType(concreteSubtype, concreteMethodInfo.ReturnType);

                // additional MonitorProfile arguments
                var args = new MonitorProfileCtorArgs(STATIC_FLAGS, _settings);

                // create a profile for the field using the the generic type and the attribute.
                var profile = (MonitorProfile) CreateInstance(concreteGenericType, concreteMethodInfo, attribute, args);

                // cache the profile and create an instance of with the static profile.
                _staticProfiles.Add(profile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- Post Profining ---

        private void PostProfileGenericTypeFieldInfo(Type type)
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

        private void PostProfileGenericTypePropertyInfo(Type type)
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
        
        private void PostProfileGenericTypeEventInfo(Type type)
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
        
        private void PostProfileGenericTypeMethodInfo(Type type)
        {
            foreach (var (methodInfo, attribute, isStatic) in _genericMethodBaseTypes)
            {
                if (!type.IsGenericType && type.IsSubclassOfRawGeneric(methodInfo.DeclaringType, false))
                {
                    if (isStatic)
                    {
                        CreateStaticMethodProfileForGenericBaseType(methodInfo, attribute, type);
                    }
                    else
                    {
                        CreateInstanceMethodProfileForGenericBaseType(methodInfo, attribute, type);
                    }
                }
            } 
        }

        #endregion
        
    }
}