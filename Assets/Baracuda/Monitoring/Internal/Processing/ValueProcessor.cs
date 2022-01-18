using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Profiles;
using Baracuda.Monitoring.Internal.Reflection;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utils;
using Baracuda.Monitoring.Management;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Processing
{
    /// <summary>
    /// This class creates custom ValueProcessor delegates for Monitoring units based on their values type.
    /// </summary>
    internal static class ValueProcessor
    {
        #region --- [FORMATTING] ---

        private const string DEFAULT_INDENT = "<pos=14>";
        private const int DEFAULT_INDENT_NUM = 14;

        private static readonly MonitoringSettings _settings = Dispatcher.InvokeAsync(MonitoringSettings.Instance).Result;
        
        private const string NULL = "<color=red>NULL</color>";

        private static readonly string _xColor = _settings.xColor.ToRichTextPrefix();
        private static readonly string _yColor = _settings.yColor.ToRichTextPrefix();
        private static readonly string _zColor = _settings.zColor.ToRichTextPrefix();
        private static readonly string _wColor = _settings.wColor.ToRichTextPrefix();
        
        #endregion
                
        //--------------------------------------------------------------------------------------------------------------

        #region --- [REFLECTION FIELDS] ---

        private const BindingFlags FLAGS 
            = BindingFlags.Default | 
              BindingFlags.Static | 
              BindingFlags.Public |
              BindingFlags.NonPublic | 
              BindingFlags.DeclaredOnly | 
              BindingFlags.Instance;

        private const BindingFlags STATIC_FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALUE PROCESSOR: TYPE SPECIFIC FALLBACK] ---

        /// <summary>
        /// Creates a default type specific processor to format the <see cref="TValue"/> depending on its exact type.
        /// </summary>
        /// <param name="profile">The target <see cref="MonitorProfile"/></param>
        /// <typeparam name="TValue">The type of the value that should be parsed/formatted</typeparam>
        /// <returns></returns>
        internal static Func<TValue, string> CreateTypeSpecificProcessor<TValue>(MonitorProfile profile)
        {
            // Transform
            if (profile.UnitValueType == typeof(Transform))
            {
                return (Func<TValue, string>)(Delegate) CreateTransformProcessor(profile);
            }
            
            // Boolean
            if (profile.UnitValueType == typeof(bool))
            {
                return (Func<TValue, string>)(Delegate) CreateBooleanProcessor(profile);
            }
            
            // Dictionary<TKey, TValue>
            if (profile.UnitValueType.IsDictionary())
            {
                var keyType   = profile.UnitValueType.GetGenericArguments()[0];
                var valueType = profile.UnitValueType.GetGenericArguments()[1];
                var genericMethod = _createDictionaryProcessorMethod.MakeGenericMethod(keyType, valueType);
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }

            // IEnumerable<bool>
            if (profile.UnitValueType.HasInterface<IEnumerable<bool>>())
            {
                return (Func<TValue, string>) (Delegate) CreateIEnumerableBooleanProcessor(profile);
            }
            
            // IEnumerable<T>
            if (profile.UnitValueType.IsGenericIEnumerable(true))
            {
                var type = profile.UnitValueType.GetElementType() ?? profile.UnitValueType.GetGenericArguments()[0];
                var genericMethod = _createGenericIEnumerableMethod.MakeGenericMethod(type);
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }

            // Array<T>
            if (profile.UnitValueType.IsArray)
            {
                var type = profile.UnitValueType.GetElementType();
                
                var genericMethod = type!.IsValueType 
                        ? _createValueTypeArrayMethod.MakeGenericMethod(type)
                        : _createReferenceTypeArrayMethod.MakeGenericMethod(type);
                
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }
            
            // IEnumerable
            if (profile.UnitValueType.IsIEnumerable(true))
            {
                return (Func<TValue, string>) (Delegate) CreateIEnumerableProcessor(profile);
            }

            if (profile.UnitValueType == typeof(Quaternion))
            {
                return (Func<TValue, string>) (Delegate) CreateQuaternionProcessor(profile);
            }

            // Vector3
            if (profile.UnitValueType == typeof(Vector3))
            {
                return (Func<TValue, string>) (Delegate) CreateVector3Processor(profile);
            }
            
            // Vector2
            if (profile.UnitValueType == typeof(Vector2))
            {
                return (Func<TValue, string>) (Delegate) CreateVector2Processor(profile);
            }

            // Color
            if (profile.UnitValueType == typeof(Color))
            {
                return (Func<TValue, string>) (Delegate) CreateColorProcessor(profile);
            }
            
            // Color32
            if (profile.UnitValueType == typeof(Color32))
            {
                return (Func<TValue, string>) (Delegate) CreateColor32Processor(profile);
            }

            // Format
            if (profile.UnitValueType.HasInterface<IFormattable>() && profile.Format != null)
            {
                return CreateFormatProcessor<TValue>(profile);
            }
            
            // UnityEngine.Object
            if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return (Func<TValue, string>) (Delegate) CreateUnityEngineObjectProcessor(profile);
            }

            // Value Type
            if (profile.UnitValueType.IsValueType)
            {
                return CreateValueProcessor<TValue>(profile);
            }
            
            // Reference Type
            else
            {
                return CreateObjectProcessor<TValue>(profile);
            }
        }

        private static Func<TValue, string> CreateObjectProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(profile.Label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value?.ToString() ?? NULL);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<TValue, string> CreateValueProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(profile.Label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }

        private static Func<TValue, string> CreateFormatProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(profile.Label);
                stringBuilder.Append(": ");
                stringBuilder.Append((value as IFormattable)?.ToString(profile.Format, null) ?? NULL);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<Color, string> CreateColorProcessor(MonitorProfile profile)
        {
            var format = profile.Format;
            var name = profile.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString(format).Colorize(value));
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString().Colorize(value));
                    return stringBuilder.ToString();
                };
            }
        }
        
        private static Func<Color32, string> CreateColor32Processor(MonitorProfile profile)
        {
            var format = profile.Format;
            var name = profile.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString(format).Colorize(value));
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value.ToString().Colorize(value));
                    return stringBuilder.ToString();
                };
            }
        }
        
        private static Func<UnityEngine.Object, string> CreateUnityEngineObjectProcessor(MonitorProfile profile)
        {
            var name = profile.Label;
            var stringBuilder = new StringBuilder();
            
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(name);
                stringBuilder.Append(": ");
                stringBuilder.Append((value != null ? value.ToString() : NULL));
                return stringBuilder.ToString();
            };
        }
        
        private static Func<Quaternion, string> CreateQuaternionProcessor(MonitorProfile profile)
        {
            var format = profile.Format;
            var name = profile.Label;
            var stringBuilder = new StringBuilder();


            return format != null
                ? (Func<Quaternion, string>) ((value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(format));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(_wColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.w.ToString(format));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                })
                : (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(_wColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.w.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
        }
        
        private static Func<Vector3, string> CreateVector3Processor(MonitorProfile profile)
        {
            var format = profile.Format;
            var name = profile.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(format));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(_zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
            }
        }
        
        private static Func<Vector2, string> CreateVector2Processor(MonitorProfile profile)
        {
            var format = profile.Format;
            var label = profile.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color>");

                    return stringBuilder.ToString();
                };
            }
            else
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(_xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(_yColor);
                    stringBuilder.Append(value.y.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");

                    return stringBuilder.ToString();
                };
            }
        }
        
        private static Func<IEnumerable, string> CreateIEnumerableProcessor(MonitorProfile profile)
        {
            var name = profile.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = profile.Indent ?? DEFAULT_INDENT;
            
            if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return profile.ShowIndexer
                    ? (Func<IEnumerable, string>) ((value) =>
                    {
                        if ((UnityEngine.Object) value == null)
                        {
                            return nullString;
                        }

                        var index = 0;
                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (object element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    })
                    : (value) =>
                    {
                        if ((UnityEngine.Object) value == null)
                        {
                            return nullString;
                        }

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    };
            }
            else
            {
                return profile.ShowIndexer
                    ? (Func<IEnumerable, string>) ((value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        var index = 0;

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    })
                    : (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append(element);
                        }

                        return stringBuilder.ToString();
                    };
            }
        }
        
        private static readonly MethodInfo _createDictionaryProcessorMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(CreateDictionaryProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<Dictionary<TKey, TValue>, string> CreateDictionaryProcessor<TKey, TValue>(MonitorProfile profile)
        {
            var name = profile.Label;
            var stringBuilder = new StringBuilder();
            var indent = profile.Indent ?? DEFAULT_INDENT;
            
            if (typeof(TKey).IsValueType)
            {
                if (typeof(TValue).IsValueType)
                {
                    return profile.ShowIndexer
                        ? (Func<Dictionary<TKey, TValue>, string>) ((value) =>
                        {
                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        })
                        : (value) =>
                        {
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
                else
                {
                    return profile.ShowIndexer
                        ? (Func<Dictionary<TKey, TValue>, string>) ((value) =>
                        {
                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        })
                        : (value) =>
                        {
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
            }
            else
            {
                if (typeof(TValue).IsValueType)
                {
                    return profile.ShowIndexer
                        ? (Func<Dictionary<TKey, TValue>, string>) ((value) =>
                        {
                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        })
                        : (value) =>
                        {
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
                else
                {
                    return profile.ShowIndexer
                        ? (Func<Dictionary<TKey, TValue>, string>) ((value) =>
                        {
                            var index = 0;
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append('[');
                                stringBuilder.Append(index++);
                                stringBuilder.Append(']');
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        })
                        : (value) =>
                        {
                            stringBuilder.Clear();
                            stringBuilder.Append(name);

                            foreach (KeyValuePair<TKey, TValue> element in value)
                            {
                                stringBuilder.Append(Environment.NewLine);
                                stringBuilder.Append(indent);
                                stringBuilder.Append(' ');
                                stringBuilder.Append('[');
                                stringBuilder.Append(element.Key?.ToString());
                                stringBuilder.Append(',');
                                stringBuilder.Append(' ');
                                stringBuilder.Append(element.Value?.ToString());
                                stringBuilder.Append(']');
                            }

                            return stringBuilder.ToString();
                        };
                }
            }
        }
        
        private static readonly MethodInfo _createReferenceTypeArrayMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(CreateReferenceTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<T[], string> CreateReferenceTypeArrayProcessor<T>(MonitorProfile profile)
        {
            var name = profile.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = profile.Indent ?? DEFAULT_INDENT;
            
            if(typeof(T).IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return profile.ShowIndexer
                    ? (Func<T[], string>) ((value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        var index = 0;

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element != null ? element.ToString() : NULL);
                        }

                        return stringBuilder.ToString();
                    })
                    : (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }

                        stringBuilder.Clear();
                        stringBuilder.Append(name);

                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append(element != null ? element.ToString() : NULL);
                        }

                        return stringBuilder.ToString();
                    };
            }
            else
            {
                if (profile.ShowIndexer)
                {
                    return (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }
                        var index = 0;
                        
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        
                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append('[');
                            stringBuilder.Append(index++);
                            stringBuilder.Append("]: ");
                            stringBuilder.Append(element?.ToString() ?? NULL);
                        }
                        return stringBuilder.ToString();
                    };    
                }
                else
                {
                    return (value) =>
                    {
                        if (value == null)
                        {
                            return nullString;
                        }
                        
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        
                        foreach (var element in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(indent);
                            stringBuilder.Append(element?.ToString() ?? NULL);
                        }
                        return stringBuilder.ToString();
                    };
                }
                
            }
        }
        
        private static readonly MethodInfo _createValueTypeArrayMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(CreateValueTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<T[], string> CreateValueTypeArrayProcessor<T>(MonitorProfile profile) where T : unmanaged
        {
            var name = profile.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();

            return profile.ShowIndexer
                ? (Func<T[], string>) ((value) =>
                {
                    if (value == null)
                    {
                        return nullString;
                    }

                    var index = 0;

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    foreach (var element in value)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(profile.Indent ?? DEFAULT_INDENT);
                        stringBuilder.Append('[');
                        stringBuilder.Append(index++);
                        stringBuilder.Append("]: ");
                        stringBuilder.Append(element.ToString());
                    }

                    return stringBuilder.ToString();
                })
                : (value) =>
                {
                    if (value == null)
                    {
                        return nullString;
                    }

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    foreach (var element in value)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(profile.Indent ?? DEFAULT_INDENT);
                        stringBuilder.Append(element.ToString());
                    }

                    return stringBuilder.ToString();
                };
        }
        
        private static readonly MethodInfo _createGenericIEnumerableMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(CreateGenericIEnumerableProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<IEnumerable<T>, string> CreateGenericIEnumerableProcessor<T>(MonitorProfile profile)
        {
            var name = profile.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = profile.Indent ?? DEFAULT_INDENT;
            
            // Unity objects might not be properly initialized in builds leading to a false result when performing a null check.
#if UNITY_EDITOR
             if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
             {
                 return profile.ShowIndexer
                     ? (Func<IEnumerable<T>, string>) ((value) =>
                     {
                         if ((UnityEngine.Object) value == null)
                         {
                             return nullString;
                         }

                         var index = 0;
                         stringBuilder.Clear();
                         stringBuilder.Append(name);

                         foreach (var element in value)
                         {
                             stringBuilder.Append(Environment.NewLine);
                             stringBuilder.Append(indent);
                             stringBuilder.Append('[');
                             stringBuilder.Append(index++);
                             stringBuilder.Append("]: ");
                             stringBuilder.Append(element);
                         }

                         return stringBuilder.ToString();
                     })
                     : (value) =>
                     {
                         // ReSharper disable once SuspiciousTypeConversion.Global
                         if ((UnityEngine.Object) value == null)
                         {
                             return nullString;
                         }

                         stringBuilder.Clear();
                         stringBuilder.Append(name);

                         foreach (var element in value)
                         {
                             stringBuilder.Append(Environment.NewLine);
                             stringBuilder.Append(indent);
                             stringBuilder.Append(element);
                         }

                         return stringBuilder.ToString();
                     };
             }
            else
#endif //UNITY_EDITOR
             {
                 return profile.ShowIndexer
                     ? (Func<IEnumerable<T>, string>) ((value) =>
                     {
                         if (value == null)
                         {
                             return nullString;
                         }

                         var index = 0;

                         stringBuilder.Clear();
                         stringBuilder.Append(name);

                         foreach (var element in value)
                         {
                             stringBuilder.Append(Environment.NewLine);
                             stringBuilder.Append(indent);
                             stringBuilder.Append('[');
                             stringBuilder.Append(index++);
                             stringBuilder.Append("]: ");
                             stringBuilder.Append(element);
                         }

                         return stringBuilder.ToString();
                     })
                     : (value) =>
                     {
                         if (value == null)
                         {
                             return nullString;
                         }

                         stringBuilder.Clear();
                         stringBuilder.Append(name);

                         foreach (var element in value)
                         {
                             stringBuilder.Append(Environment.NewLine);
                             stringBuilder.Append(indent);
                             stringBuilder.Append(element);
                         }

                         return stringBuilder.ToString();
                     };
             }
        }
        
        private static Func<IEnumerable<bool>, string> CreateIEnumerableBooleanProcessor(MonitorProfile profile)
        {
            var name = profile.Label;
            var nullString = $"{name}: {NULL} (IEnumerable<bool>)";
            var tureString  =  $"<color=green>TRUE</color>";
            var falseString =  $"<color=red>FALSE</color>";
            var stringBuilder = new StringBuilder();
            var indent = profile.Indent ?? DEFAULT_INDENT;

            return profile.ShowIndexer
                ? (Func<IEnumerable<bool>, string>) ((value) =>
                {
                    if (value == null) return nullString;
                    var index = 0;

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    foreach (var element in value)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(indent);
                        stringBuilder.Append('[');
                        stringBuilder.Append(index++);
                        stringBuilder.Append("]: ");
                        stringBuilder.Append(element ? tureString : falseString);
                    }

                    return stringBuilder.ToString();
                })
                : (value) =>
                {
                    if (value == null) return nullString;

                    stringBuilder.Clear();
                    stringBuilder.Append(name);

                    foreach (var element in value)
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(indent);
                        stringBuilder.Append(element ? tureString : falseString);
                    }

                    return stringBuilder.ToString();
                };
        }
        
        private static Func<Transform, string> CreateTransformProcessor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var name = profile.Label;
            var nullString = $"{name}: {NULL}";
            var indentValue = profile.IndentValue >= 0 ? profile.IndentValue : DEFAULT_INDENT_NUM;

            return (value) =>
            {
                if (value == null) return nullString;

                stringBuilder.Clear();
                stringBuilder.Append(name);
                
                var layer = 1;
                foreach (Transform element in value)
                {
                    stringBuilder.Append("\n- <pos=");
                    stringBuilder.Append((layer * indentValue).ToString());
                    stringBuilder.Append('>');
                    stringBuilder.Append(element.ToString());
                    
                    foreach (Transform child in element)
                    {
                        Traverse(child, layer, ref stringBuilder);
                    }
                }
                
                return stringBuilder.ToString();

                void Traverse(Transform parent, int i, ref StringBuilder builder)
                {
                    builder.Append("\n- <pos=");
                    builder.Append((++i * indentValue).ToString());
                    builder.Append('>');
                    builder.Append(parent);
                    foreach (Transform child in parent)
                    {
                        Traverse(child, i, ref builder);
                    }
                }
            };
        }
        
        private static Func<bool, string> CreateBooleanProcessor(MonitorProfile profile)
        {
            var tureString  =  $"{profile.Label}: {"TRUE".Colorize(_settings.trueColor)}";
            var falseString =  $"{profile.Label}: {"FALSE".Colorize(_settings.falseColor)}";
            return (value) => value ? tureString : falseString;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VALUE PROCESSOR: CUSTOM] ---

        /// <summary>
        /// This method will scan the declaring <see cref="Type"/> of the passed
        /// <see cref="ValueProfile{TTarget,TValue}"/> for a valid processor method with the passed name.<br/>
        /// Certain types offer special functionality and require additional handling. Those types are:<br/>
        /// Types assignable from <see cref="IList{T}"/> (inc. <see cref="Array"/>)<br/>
        /// Types assignable from <see cref="IDictionary{TKey, TValue}"/><br/>
        /// Types assignable from <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="processor">name of the method declared as a value processor</param>
        /// <param name="valueProfile">the profile of the <see cref="ValueUnit{TTarget,TValue}"/></param>
        /// <typeparam name="TTarget">the <see cref="Type"/> of the profiles Target instance</typeparam>
        /// <typeparam name="TValue">the <see cref="Type"/> of the profiles value instance</typeparam>
        /// <returns></returns>
        internal static Func<TValue, string> FindCustomProcessor<TTarget, TValue>(
            string processor,
            ValueProfile<TTarget, TValue> valueProfile) where TTarget : class
        {
            try
            {
                // validate that the processor name is not null.
                if (string.IsNullOrWhiteSpace(processor)) return null;
                
                // setup
                var declaringType = valueProfile.UnitDeclaringType;
                var valueType = valueProfile.UnitValueType;
                
                // get the processor method.
                var processorMethod = declaringType.GetMethod(processor, STATIC_FLAGS);
                
                // validate that the processor is not null.
                if (processorMethod == null)
                {
                    ExceptionLogging.LogException(new ProcessorNotFoundException(processor, declaringType));
                    return null;
                }
                
                // create a delegate with for the processors method.
                var processorFunc = processorMethod.CreateMatchingDelegate(FLAGS);
            
                //----------------------------
                
                // cache the parameter information of the processor method.
                var parameterInfos = processorFunc.Method.GetParameters();
                
                if (!parameterInfos.Any())
                {
                    ExceptionLogging.LogException(new InvalidProcessorSignatureException(processor, declaringType));
                    return null;
                }
                
                //----------------------------
                
                // Check if the types are a perfect match
                if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsAssignableFrom(valueType))
                {
                    var defaultDelegate = (Func<TValue, string>) processorFunc;
                    return value => defaultDelegate(value);
                }
                
                //----------------------------

                #region --- [ILIST<T>] ---

                // IList<T> processor
                if (valueType.IsGenericIList())
                {
                    if (parameterInfos[0].ParameterType.IsAssignableFrom(valueType.IsArray? valueType.GetElementType() : valueType.GetGenericArguments()[0]))
                    {
                        // IList<T> processor passing each element of the collection
                        if (parameterInfos.Length == 1)
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = _genericIListWithoutIndexCreateMethod
                                .MakeGenericMethod(valueType, valueType.IsArray? valueType.GetElementType() : valueType.GetGenericArguments()[0]);
                            
                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.Label})
                                ?.ConvertUnsafe<object, Func<TValue, string>>();
                            
                            // return the delegate if it was created successfully
                            if (func != null) return func;
                        }
                        
                        // IList<T> processor passing each element of the collection with the associated index.
                        if (parameterInfos.Length == 2 && parameterInfos[1].ParameterType == typeof(int))
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = _genericIListWithIndexCreateMethod
                                .MakeGenericMethod(valueType, valueType.IsArray? valueType.GetElementType() : valueType.GetGenericArguments()[0]);
                            
                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.Label})
                                ?.ConvertUnsafe<object, Func<TValue, string>>();
                            
                            // return the delegate if it was created successfully
                            if (func != null) return func;
                        }   
                    }
                }

                #endregion
                
                //----------------------------

                #region --- [IDICTIONARY<TKEY,TVALUE>] ---

                // IDictionary<TKey, TValue> processor
                if (valueType.IsGenericIDictionary())
                {
                    // Signature validation 1.
                    // check that the length of the signature is 2, so that it could potentially contain two
                    // valid arguments. (TKey and TValue)
                    if (parameterInfos.Length == 2)
                    {
                        // Signature validation 2.
                        // check that the signature of the processor method is compatible with the generic type definition
                        // of the dictionaries KeyValuePair<TKey, TValue> 
                        var genericArgs = valueType.GetGenericArguments();
                        if (parameterInfos[0].ParameterType == genericArgs[0]
                            && parameterInfos[1].ParameterType == genericArgs[1])
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = _genericIDictionaryCreateMethod
                                .MakeGenericMethod(valueType, genericArgs[0], genericArgs[1]);
                            
                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.Label})
                                ?.ConvertUnsafe<object, Func<TValue, string>>();
                            
                            // return the delegate if it was created successfully
                            if (func != null) return func;
                        }
                    }
                }

                #endregion
                
                //----------------------------

                #region --- [IENUMERABLE<T>] ---

                // IEnumerable<T> processor
                if (valueType.IsGenericIEnumerable(true))
                {
                    // check that the signature of the processor method is compatible with the generic type definition
                    // of the IEnumerable<T>s generic type definition.
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == valueType.GetGenericArguments()[0])
                    {
                        // create a generic method to create the generic processor
                        var delegateCreationMethod = _genericIEnumerableProcessorMethod
                            .MakeGenericMethod(valueType, valueType.GetGenericArguments()[0]);
                        
                        // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                        var func = delegateCreationMethod
                            .Invoke(null, new object[] {processorFunc.Method, valueProfile.Label})
                            ?.ConvertUnsafe<object, Func<TValue, string>>();
                        
                        // return the delegate if it was created successfully
                        if (func != null) return func;
                    }
                }

                #endregion
                
                //----------------------------
                
                return null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALUE PROCESSOR: ILIST WITH INDEX ARGUMENT] ---

        private static readonly MethodInfo _genericIListWithIndexCreateMethod =
            typeof(ValueProcessor).GetMethod(nameof(CreateIListFuncWithIndexArgument), STATIC_FLAGS);
        
        /// <summary>
        /// Creates a delegate that accepts an input of type <see cref="IList{TElement}"/> and returns a string, using
        /// a custom value processor with a signature <see cref="string"/>(<see cref="TElement"/> element <see cref="Int32"/> index)<br/>
        /// </summary>
        /// <param name="processor">the <see cref="MethodInfo"/> of the previously validated processor</param>
        /// <param name="name">the name of the <see cref="ValueUnit{TTarget,TValue}"/></param>
        /// <typeparam name="TInput">the exact argument/input type of the processors method. This type must be assignable from <see cref="IList{TElement}"/></typeparam>
        /// <typeparam name="TElement">the element type of the IList</typeparam>
        /// <returns></returns>
        private static Func<TInput, string> CreateIListFuncWithIndexArgument<TInput, TElement>(MethodInfo processor, string name) where TInput : IList<TElement>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var listDelegate = (Func<TElement, int, string>)Delegate.CreateDelegate(typeof(Func<TElement, int, string>), processor);
                
                // create a matching null string.
                var nullString = $"{name}: {NULL}";
                
                // create a stringBuilder object to be used by the lambda.
                var stringBuilder = new StringBuilder();

                #region --- [PROCESSOR CODE] ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if (value != null)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        for (int i = 0; i < value.Count; i++)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DEFAULT_INDENT);
                            stringBuilder.Append(listDelegate(value[i], i));
                        }
                        return stringBuilder.ToString();
                    }

                    return nullString;
                };

                #endregion 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALUE PROCESSOR: ILIST WITHOUT INDEX ARGUMENT] ---

        private static readonly MethodInfo _genericIListWithoutIndexCreateMethod =
            typeof(ValueProcessor).GetMethod(nameof(CreateIListFuncWithoutIndexArgument), STATIC_FLAGS);
        
        private static Func<TInput, string> CreateIListFuncWithoutIndexArgument<TInput, TElement>(MethodInfo processor, string name) where TInput : IList<TElement>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var listDelegate = (Func<TElement, string>)Delegate.CreateDelegate(typeof(Func<TElement, string>), processor);
                
                // create a matching null string.
                var nullString = $"{name}: {NULL}";

                // create a stringBuilder object to be used by the lambda.
                var stringBuilder = new StringBuilder();

                #region --- [PROCESSOR CODE] ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if (value != null)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        
                        for (int i = 0; i < value.Count; i++)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DEFAULT_INDENT);
                            stringBuilder.Append(listDelegate(value[i]));
                        }
                        return stringBuilder.ToString();
                    }

                    return nullString;
                };

                #endregion 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VALUE PROCESSOR: DICTIONARY] ---

        
        private static readonly MethodInfo _genericIDictionaryCreateMethod =
            typeof(ValueProcessor).GetMethod(nameof(CreateIDictionaryFunc), STATIC_FLAGS);

        private static Func<TInput, string> CreateIDictionaryFunc<TInput, TKey, TValue>(MethodInfo processor, string name) where TInput : IDictionary<TKey, TValue>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var genericFunc = (Func<TKey,TValue,string>)Delegate.CreateDelegate(typeof(Func<TKey,TValue,string>), processor);

                // create a matching null string.
                var nullString = $"{name}: {NULL}";
                
                // create a stringBuilder object to be used by the lambda.
                var stringBuilder = new StringBuilder();
                
                #region --- [PROCESSOR CODE] ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if (value != null)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append(name);
                        
                        foreach (KeyValuePair<TKey, TValue> valuePair in value)
                        {
                            stringBuilder.Append(Environment.NewLine);
                            stringBuilder.Append(DEFAULT_INDENT);
                            stringBuilder.Append(genericFunc(valuePair.Key, valuePair.Value));
                        }
                        return stringBuilder.ToString();
                    }

                    return nullString;
                };

                #endregion
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALUE PROCESSOR: IENUMERABLE] ---

        private static readonly MethodInfo _genericIEnumerableProcessorMethod =
            typeof(ValueProcessor).GetMethod(nameof(CreateIEnumerableFunc), STATIC_FLAGS);
        
        private static Func<TInput, string> CreateIEnumerableFunc<TInput, TElement>(MethodInfo processor, string name) where TInput : IEnumerable<TElement>
        {
            try
            {
                // create a matching delegate with the processors method info.
                var enumerableDelegate = (Func<TElement,string>)Delegate.CreateDelegate(typeof(Func<TElement,string>), processor);
                
                // create a matching null string.
                var nullString = $"{name}: {NULL}";
                
                // create a stringBuilder object to be used by the lambda.
                var sb = new StringBuilder();

                #region --- [PROCESSOR CODE] ---

                return (value) =>
                {
                    // check that the passed value is not null.
                    if ((object)value != null)
                    {
                        sb.Clear();
                        sb.Append(name);
                        foreach (TElement element in value)
                        {
                            sb.Append(Environment.NewLine);
                            sb.Append(DEFAULT_INDENT);
                            sb.Append(enumerableDelegate(element));
                        }

                        return sb.ToString();
                    }

                    return nullString;
                };

                #endregion 
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            return null;
        }

        #endregion
    }
}