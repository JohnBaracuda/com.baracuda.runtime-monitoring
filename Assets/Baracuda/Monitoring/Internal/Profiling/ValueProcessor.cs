// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Internal.Exceptions;
using Baracuda.Monitoring.Internal.Units;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using Baracuda.Threading;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Profiling
{
    /// <summary>
    /// Class creates custom ValueProcessor delegates for Monitoring units based on their values type.
    /// </summary>
    internal static class ValueProcessor
    {
        #region --- Formatting ---

        //TODO: Fix indent rich text for GUI
        private const string DEFAULT_INDENT = "  ";//"<pos=14>";
        private const int DEFAULT_INDENT_NUM = 14;
        private const string NULL = "<color=red>NULL</color>";

        private static readonly MonitoringSettings settings = Dispatcher.InvokeAsync(MonitoringSettings.GetInstance).Result;
        
        private static readonly string xColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.XColor)}>";
        private static readonly string yColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.YColor)}>";
        private static readonly string zColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.ZColor)}>";
        private static readonly string wColor = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.WColor)}>";
        
        #endregion
                
        //--------------------------------------------------------------------------------------------------------------

        #region --- Reflection Fields ---

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
        
        private const BindingFlags INSTANCE_FLAGS
            = BindingFlags.Instance | 
              BindingFlags.NonPublic |
              BindingFlags.Public |
              BindingFlags.DeclaredOnly;

        #endregion

        #region --- Misc ---
        
        private static string CreateIndentString(MonitorProfile profile)
        {
            return profile.TryGetMetaAttribute<FormatAttribute>(out var attribute)
                ? attribute.ElementIndent >= 0 ? $"<pos={attribute.ElementIndent.ToString()}>" : DEFAULT_INDENT
                : DEFAULT_INDENT;
        }

        private static int CreateIndentValue(MonitorProfile profile)
        {
            return profile.TryGetMetaAttribute<FormatAttribute>(out var attribute)
                ? attribute.ElementIndent >= 0 ? attribute.ElementIndent : DEFAULT_INDENT_NUM
                : DEFAULT_INDENT_NUM;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Value Processor: Type Specific Fallback ---

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
                return (Func<TValue, string>)(Delegate) TransformProcessor(profile);
            }
            
            // Boolean
            if (profile.UnitValueType == typeof(bool))
            {
                return (Func<TValue, string>)(Delegate) CreateBooleanProcessor(profile);
            }
            
#if !ENABLE_IL2CPP
            // Dictionary<TKey, TValue>
            if (profile.UnitValueType.IsGenericIDictionary())
            {
                var keyType   = profile.UnitValueType.GetGenericArguments()[0];
                var valueType = profile.UnitValueType.GetGenericArguments()[1];
                var genericMethod = createDictionaryProcessorMethod.MakeGenericMethod(keyType, valueType);
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }
#endif

            // IEnumerable<bool>
            if (profile.UnitValueType.HasInterface<IEnumerable<bool>>())
            {
                return (Func<TValue, string>) (Delegate) IEnumerableBooleanProcessor(profile);
            }
            
#if !ENABLE_IL2CPP
            // IEnumerable<T>
            if (profile.UnitValueType.IsGenericIEnumerable(true))
            {
                var type = profile.UnitValueType.GetElementType() ?? profile.UnitValueType.GetGenericArguments()[0];
                var genericMethod = createGenericIEnumerableMethod.MakeGenericMethod(type);
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }

            // Array<T>
            if (profile.UnitValueType.IsArray)
            {
                var type = profile.UnitValueType.GetElementType();

                Debug.Assert(type != null, nameof(type) + " != null");
                
                var genericMethod = type.IsValueType ? createValueTypeArrayMethod.MakeGenericMethod(type) : createReferenceTypeArrayMethod.MakeGenericMethod(type);
                
                return (Func<TValue, string>) genericMethod.Invoke(null, new object[]{profile});
            }
#endif
            
            // IEnumerable
            if (profile.UnitValueType.IsIEnumerable(true))
            {
                return (Func<TValue, string>) (Delegate) IEnumerableProcessor(profile);
            }

            // Quaternion
            if (profile.UnitValueType == typeof(Quaternion))
            {
                return (Func<TValue, string>) (Delegate) QuaternionProcessor(profile);
            }

            // Vector3
            if (profile.UnitValueType == typeof(Vector3))
            {
                return (Func<TValue, string>) (Delegate) Vector3Processor(profile);
            }
            
            // Vector2
            if (profile.UnitValueType == typeof(Vector2))
            {
                return (Func<TValue, string>) (Delegate) Vector2Processor(profile);
            }

            // Color
            if (profile.UnitValueType == typeof(Color))
            {
                return (Func<TValue, string>) (Delegate) ColorProcessor(profile);
            }
            
            // Color32
            if (profile.UnitValueType == typeof(Color32))
            {
                return (Func<TValue, string>) (Delegate) Color32Processor(profile);
            }

            // Format
            if (profile.UnitValueType.HasInterface<IFormattable>() && profile.FormatData.Format != null)
            {
                return FormattedProcessor<TValue>(profile);
            }
            
            // UnityEngine.Object
            if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return (Func<TValue, string>) (Delegate) UnityEngineObjectProcessor(profile);
            }
            
            // Int32
            if (profile.UnitValueType.IsInt32())
            {
                return (Func<TValue, string>) (Delegate) Int32Processor(profile);
            }
            
            // Int64
            if (profile.UnitValueType.IsInt64())
            {
                return (Func<TValue, string>) (Delegate) Int64Processor(profile);
            }
            
            // Float
            if (profile.UnitValueType.IsSingle())
            {
                return (Func<TValue, string>) (Delegate) SingleProcessor(profile);
            }
            
            // Double
            if (profile.UnitValueType.IsDouble())
            {
                return (Func<TValue, string>) (Delegate) DoubleProcessor(profile);
            }

            // Value Type
            if (profile.UnitValueType.IsValueType)
            {
                return ValueTypeProcessor<TValue>(profile);
            }
            
            // Reference Type
            else
            {
                return ObjectProcessor<TValue>(profile);
            }
        }

        private static Func<TValue, string> ObjectProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value?.ToString() ?? NULL);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<TValue, string> ValueTypeProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<int, string> Int32Processor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<long, string> Int64Processor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<float, string> SingleProcessor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }
        
        
        private static Func<double, string> DoubleProcessor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append(value);
                return stringBuilder.ToString();
            };
        }
        

        private static Func<TValue, string> FormattedProcessor<TValue>(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var label = profile.FormatData.Label;
            return (value) =>
            {
                stringBuilder.Clear();
                stringBuilder.Append(label);
                stringBuilder.Append(": ");
                stringBuilder.Append((value as IFormattable)?.ToString(profile.FormatData.Format, null) ?? NULL);
                return stringBuilder.ToString();
            };
        }
        
        private static Func<Color, string> ColorProcessor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
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
        
        private static Func<Color32, string> Color32Processor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
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
        
        private static Func<UnityEngine.Object, string> UnityEngineObjectProcessor(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
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
        
        private static Func<Quaternion, string> QuaternionProcessor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
            var stringBuilder = new StringBuilder();


            return format != null
                ? (Func<Quaternion, string>) ((value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(format));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(wColor);
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
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");
                    stringBuilder.Append("W:");
                    stringBuilder.Append(wColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.w.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
        }
        
        private static Func<Vector3, string> Vector3Processor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var name = profile.FormatData.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(name);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(format));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
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
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.y.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> ");
                    stringBuilder.Append("Z:");
                    stringBuilder.Append(zColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.z.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");
                    return stringBuilder.ToString();
                };
            }
        }
        
        private static Func<Vector2, string> Vector2Processor(MonitorProfile profile)
        {
            var format = profile.FormatData.Format;
            var label = profile.FormatData.Label;
            var stringBuilder = new StringBuilder();

            if (format != null)
            {
                return (value) =>
                {
                    stringBuilder.Clear();
                    stringBuilder.Append(label);
                    stringBuilder.Append(": X:");
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(format));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(yColor);
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
                    stringBuilder.Append(xColor);
                    stringBuilder.Append('[');
                    stringBuilder.Append(value.x.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color> Y:");
                    stringBuilder.Append(yColor);
                    stringBuilder.Append(value.y.ToString(CultureInfo.InvariantCulture));
                    stringBuilder.Append("]</color>");

                    return stringBuilder.ToString();
                };
            }
        }

        
        private static Func<IEnumerable, string> IEnumerableProcessor(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = CreateIndentString(profile);
            
            if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return profile.FormatData.ShowIndexer
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
                return profile.FormatData.ShowIndexer
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
        
        private static readonly MethodInfo createDictionaryProcessorMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(DictionaryProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<IDictionary<TKey, TValue>, string> DictionaryProcessor<TKey, TValue>(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var stringBuilder = new StringBuilder();
            var indent = CreateIndentString(profile);
            
            if (typeof(TKey).IsValueType)
            {
                if (typeof(TValue).IsValueType)
                {
                    return profile.FormatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
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
                    return profile.FormatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
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
                    return profile.FormatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
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
                    return profile.FormatData.ShowIndexer
                        ? (Func<IDictionary<TKey, TValue>, string>) ((value) =>
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
        
        private static readonly MethodInfo createReferenceTypeArrayMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(ReferenceTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<T[], string> ReferenceTypeArrayProcessor<T>(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = CreateIndentString(profile);
            
            if(typeof(T).IsSubclassOrAssignable(typeof(UnityEngine.Object)))
            {
                return profile.FormatData.ShowIndexer
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
                if (profile.FormatData.ShowIndexer)
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
        
        private static readonly MethodInfo createValueTypeArrayMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(ValueTypeArrayProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<T[], string> ValueTypeArrayProcessor<T>(MonitorProfile profile) where T : unmanaged
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = CreateIndentString(profile);

            return profile.FormatData.ShowIndexer
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
                        stringBuilder.Append(indent);
                        stringBuilder.Append(element.ToString());
                    }

                    return stringBuilder.ToString();
                };
        }
        
        private static readonly MethodInfo createGenericIEnumerableMethod = typeof(ValueProcessor)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(methodInfo => methodInfo.Name == nameof(GenericIEnumerableProcessor) && methodInfo.IsGenericMethodDefinition);
        
        private static Func<IEnumerable<T>, string> GenericIEnumerableProcessor<T>(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var stringBuilder = new StringBuilder();
            var indent = CreateIndentString(profile);
            
            // Unity objects might not be properly initialized in builds leading to a false result when performing a null check.
#if UNITY_EDITOR
             if (profile.UnitValueType.IsSubclassOrAssignable(typeof(UnityEngine.Object)))
             {
                 return profile.FormatData.ShowIndexer
                     ? (Func<IEnumerable<T>, string>) ((value) =>
                     {
                         // ReSharper disable once SuspiciousTypeConversion.Global
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
                 return profile.FormatData.ShowIndexer
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
        
        private static Func<IEnumerable<bool>, string> IEnumerableBooleanProcessor(MonitorProfile profile)
        {
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL} (IEnumerable<bool>)";
            var tureString  =  $"<color=green>TRUE</color>";
            var falseString =  $"<color=red>FALSE</color>";
            var stringBuilder = new StringBuilder();
            var indent = CreateIndentString(profile);

            return profile.FormatData.ShowIndexer
                ? (Func<IEnumerable<bool>, string>) ((value) =>
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
                        stringBuilder.Append(element ? tureString : falseString);
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
                        stringBuilder.Append(element ? tureString : falseString);
                    }

                    return stringBuilder.ToString();
                };
        }
        
        private static Func<Transform, string> TransformProcessor(MonitorProfile profile)
        {
            var stringBuilder = new StringBuilder();
            var name = profile.FormatData.Label;
            var nullString = $"{name}: {NULL}";
            var indentValue = CreateIndentValue(profile);

            return (value) =>
            {
                if (value == null)
                {
                    return nullString;
                }

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
            var tureString  =  $"{profile.FormatData.Label}: {"TRUE".Colorize(settings.TrueColor)}";
            var falseString =  $"{profile.FormatData.Label}: {"FALSE".Colorize(settings.FalseColor)}";
            return (value) => value ? tureString : falseString;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Value Processor: Custom ---

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
        internal static Func<TValue, string> FindCustomStaticProcessor<TTarget, TValue>(
            string processor,
            ValueProfile<TTarget, TValue> valueProfile) where TTarget : class
        {
            try
            {
                // validate that the processor name is not null.
                if (string.IsNullOrWhiteSpace(processor))
                {
                    return null;
                }

                // setup
                var declaringType = valueProfile.UnitTargetType;
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

                #region --- Ilist<T> ---

                // IList<T> processor
                if (valueType.IsGenericIList())
                {
                    //Debug.Log("IList");
                    if (parameterInfos[0].ParameterType.IsAssignableFrom(valueType.IsArray? valueType.GetElementType() : valueType.GetGenericArguments()[0]))
                    {
                        // IList<T> processor passing each element of the collection
                        if (parameterInfos.Length == 1)
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = genericIListWithoutIndexCreateMethod
                                .MakeGenericMethod(valueType, valueType.IsArray? valueType.GetElementType() : valueType.GetGenericArguments()[0]);
                            
                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                                ?.ConvertFast<object, Func<TValue, string>>();
                            
                            // return the delegate if it was created successfully
                            if (func != null)
                            {
                                return func;
                            }
                        }
                        
                        // IList<T> processor passing each element of the collection with the associated index.
                        if (parameterInfos.Length == 2 && parameterInfos[1].ParameterType == typeof(int))
                        {
                            // create a generic method to create the generic processor
                            var delegateCreationMethod = genericIListWithIndexCreateMethod
                                .MakeGenericMethod(valueType, valueType.IsArray? valueType.GetElementType() : valueType.GetGenericArguments()[0]);
                            
                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                                ?.ConvertFast<object, Func<TValue, string>>();
                            
                            // return the delegate if it was created successfully
                            if (func != null)
                            {
                                return func;
                            }
                        }   
                    }
                }

                #endregion
                
                //----------------------------

                #region --- IDictionary<TKey,TValue> ---

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
                            var delegateCreationMethod = genericIDictionaryCreateMethod
                                .MakeGenericMethod(valueType, genericArgs[0], genericArgs[1]);
                            
                            // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                            var func = delegateCreationMethod
                                .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                                ?.ConvertFast<object, Func<TValue, string>>();
                            
                            // return the delegate if it was created successfully
                            if (func != null)
                            {
                                return func;
                            }
                        }
                    }
                }

                #endregion
                
                //----------------------------

                #region --- IEnumerable<T> ---

                // IEnumerable<T> processor
                if (valueType.IsGenericIEnumerable(true))
                {
                    // check that the signature of the processor method is compatible with the generic type definition
                    // of the IEnumerable<T>s generic type definition.
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == valueType.GetGenericArguments()[0])
                    {
                        // create a generic method to create the generic processor
                        var delegateCreationMethod = genericIEnumerableProcessorMethod
                            .MakeGenericMethod(valueType, valueType.GetGenericArguments()[0]);
                        
                        // create a delegate of type: <Func<TValue, string>> by invoking the delegateCreationMethod
                        var func = delegateCreationMethod
                            .Invoke(null, new object[] {processorFunc.Method, valueProfile.FormatData.Label})
                            ?.ConvertFast<object, Func<TValue, string>>();
                        
                        // return the delegate if it was created successfully
                        if (func != null)
                        {
                            return func;
                        }
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
        internal static Func<TTarget, TValue, string> FindCustomInstanceProcessor<TTarget, TValue>(
            string processor,
            ValueProfile<TTarget, TValue> valueProfile) where TTarget : class
        {
            try
            {
                // validate that the processor name is not null.
                if (string.IsNullOrWhiteSpace(processor))
                {
                    return null;
                }

                var declaringType = valueProfile.UnitTargetType;
                var valueType = valueProfile.UnitValueType;
                
                var processorMethod = declaringType.GetMethod(processor, INSTANCE_FLAGS);
                
                if (processorMethod == null)
                {
                    ExceptionLogging.LogException(new ProcessorNotFoundException(processor, declaringType));
                    return null;
                }
                
                // create a delegate with for the processors method.
                var processorFunc = (Func<TTarget, TValue, string>)processorMethod.CreateDelegate(typeof(Func<TTarget, TValue, string>));
                
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
                    return (target, value) => processorFunc(target, value);
                }
                
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

        #region --- Value Processor: Ilist With Index Argument ---

        private static readonly MethodInfo genericIListWithIndexCreateMethod =
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

                #region --- Processor Code ---

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

        #region --- Value Processor: Ilist Without Index Argument ---

        private static readonly MethodInfo genericIListWithoutIndexCreateMethod =
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

                #region --- Processor Code ---

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
        
        #region --- Value Processor: Dictionary ---

        
        private static readonly MethodInfo genericIDictionaryCreateMethod =
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
                
                #region --- Processor Code ---

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

        #region --- Value Processor: Ienumerable ---

        private static readonly MethodInfo genericIEnumerableProcessorMethod =
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

                #region --- Processor Code ---

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