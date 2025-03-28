//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace NaoRemote.Json
{
    internal delegate void ExporterFunc(object obj, JsonWriter writer);
    public delegate void ExporterFunc<T>(T obj, JsonWriter writer);
    internal delegate object ImporterFunc(object input);
    public delegate TValue ImporterFunc<TJson, TValue>(TJson input);
    public delegate IJsonWrapper WrapperFactory();

    public class JsonMapper
    {
        #region Fields
        private static readonly int MaxNestingDepth;

        private static readonly IFormatProvider DateTimeFormat;

        private static readonly IDictionary<Type, ExporterFunc> BaseExportersTable;
        private static readonly IDictionary<Type, ExporterFunc> CustomExportersTable;

        private static readonly IDictionary<Type, IDictionary<Type, ImporterFunc>> BaseImportersTable;
        private static readonly IDictionary<Type, IDictionary<Type, ImporterFunc>> CustomImportersTable;

        private static readonly IDictionary<Type, ArrayMetadata> ArrayMetadata;
        private static readonly object ArrayMetadataLock = new object();

        private static readonly IDictionary<Type, IDictionary<Type, MethodInfo>> ConvOps;
        private static readonly object ConvOpsLock = new object();

        private static readonly IDictionary<Type, ObjectMetadata> ObjectMetadata;
        private static readonly object ObjectMetadataLock = new object();

        private static readonly IDictionary<Type, IList<PropertyMetadata>> TypeProperties;
        private static readonly object TypePropertiesLock = new object();

        private static readonly JsonWriter StaticWriter;
        private static readonly object StaticWriterLock = new object();
        #endregion

        #region Constructors
        static JsonMapper()
        {
            MaxNestingDepth = 100;

            ArrayMetadata = new Dictionary<Type, ArrayMetadata>();
            ConvOps = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
            ObjectMetadata = new Dictionary<Type, ObjectMetadata>();
            TypeProperties = new Dictionary<Type,
                            IList<PropertyMetadata>>();

            StaticWriter = new JsonWriter();

            DateTimeFormat = DateTimeFormatInfo.InvariantInfo;

            BaseExportersTable = new Dictionary<Type, ExporterFunc>();
            CustomExportersTable = new Dictionary<Type, ExporterFunc>();

            BaseImportersTable = new Dictionary<Type,
                                 IDictionary<Type, ImporterFunc>>();
            CustomImportersTable = new Dictionary<Type,
                                   IDictionary<Type, ImporterFunc>>();

            RegisterBaseExporters();
            RegisterBaseImporters();
        }
        #endregion

        #region Private Methods
        private static void AddArrayMetadata(Type type)
        {
            if (ArrayMetadata.ContainsKey(type))
                return;

            var data = new ArrayMetadata {IsArray = type.IsArray};

            if (type.GetInterface("System.Collections.IList") != null)
                data.IsList = true;

            foreach (var pInfo in type.GetProperties())
            {
                if (pInfo.Name != "Item")
                    continue;

                var parameters = pInfo.GetIndexParameters();

                if (parameters.Length != 1)
                    continue;

                if (parameters[0].ParameterType == typeof(int))
                    data.ElementType = pInfo.PropertyType;
            }

            lock (ArrayMetadataLock)
            {
                try
                {
                    ArrayMetadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddObjectMetadata(Type type)
        {
            if (ObjectMetadata.ContainsKey(type))
                return;

            var data = new ObjectMetadata();

            if (type.GetInterface("System.Collections.IDictionary") != null)
                data.IsDictionary = true;

            data.Properties = new Dictionary<string, PropertyMetadata>();

            foreach (var pInfo in type.GetProperties())
            {
                if (pInfo.Name == "Item")
                {
                    var parameters = pInfo.GetIndexParameters();

                    if (parameters.Length != 1)
                        continue;

                    if (parameters[0].ParameterType == typeof(string))
                        data.ElementType = pInfo.PropertyType;

                    continue;
                }

                var pData = new PropertyMetadata {Info = pInfo, Type = pInfo.PropertyType};
                data.Properties.Add(pInfo.Name, pData);
            }

            foreach (var fInfo in type.GetFields())
            {
                var pData = new PropertyMetadata {Info = fInfo, IsField = true, Type = fInfo.FieldType};
                data.Properties.Add(fInfo.Name, pData);
            }

            lock (ObjectMetadataLock)
            {
                try
                {
                    ObjectMetadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddTypeProperties(Type type)
        {
            if (TypeProperties.ContainsKey(type))
                return;

            IList<PropertyMetadata> props = new List<PropertyMetadata>();

            foreach (var p_info in type.GetProperties())
            {
                if (p_info.Name == "Item")
                    continue;

                var p_data = new PropertyMetadata();
                p_data.Info = p_info;
                p_data.IsField = false;
                props.Add(p_data);
            }

            foreach (var f_info in type.GetFields())
            {
                var p_data = new PropertyMetadata();
                p_data.Info = f_info;
                p_data.IsField = true;

                props.Add(p_data);
            }

            lock (TypePropertiesLock)
            {
                try
                {
                    TypeProperties.Add(type, props);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static MethodInfo GetConvOp(Type t1, Type t2)
        {
            lock (ConvOpsLock)
            {
                if (!ConvOps.ContainsKey(t1))
                    ConvOps.Add(t1, new Dictionary<Type, MethodInfo>());
            }

            if (ConvOps[t1].ContainsKey(t2))
                return ConvOps[t1][t2];

            var op = t1.GetMethod(
                "op_Implicit", new Type[] { t2 });

            lock (ConvOpsLock)
            {
                try
                {
                    ConvOps[t1].Add(t2, op);
                }
                catch (ArgumentException)
                {
                    return ConvOps[t1][t2];
                }
            }

            return op;
        }

        private static object ReadValue(Type inst_type, JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd)
                return null;

            if (reader.Token == JsonToken.Null)
            {

                if (!inst_type.IsClass)
                    throw new JsonException(string.Format(
                            "Can't assign null to an instance of type {0}",
                            inst_type));

                return null;
            }

            if (reader.Token == JsonToken.Double ||
                reader.Token == JsonToken.Int ||
                reader.Token == JsonToken.Long ||
                reader.Token == JsonToken.String ||
                reader.Token == JsonToken.Boolean)
            {

                var json_type = reader.Value.GetType();

                if (inst_type.IsAssignableFrom(json_type))
                    return reader.Value;

                // If there's a custom importer that fits, use it
                if (CustomImportersTable.ContainsKey(json_type) &&
                    CustomImportersTable[json_type].ContainsKey(
                        inst_type))
                {

                    var importer =
                        CustomImportersTable[json_type][inst_type];

                    return importer(reader.Value);
                }

                // Maybe there's a base importer that works
                if (BaseImportersTable.ContainsKey(json_type) &&
                    BaseImportersTable[json_type].ContainsKey(
                        inst_type))
                {

                    var importer =
                        BaseImportersTable[json_type][inst_type];

                    return importer(reader.Value);
                }

                // Maybe it's an enum
                if (inst_type.IsEnum)
                    return Enum.ToObject(inst_type, reader.Value);

                // Try using an implicit conversion operator
                var conv_op = GetConvOp(inst_type, json_type);

                if (conv_op != null)
                    return conv_op.Invoke(null,
                                           new object[] { reader.Value });

                // No luck
                throw new JsonException(string.Format(
                        "Can't assign value '{0}' (type {1}) to type {2}",
                        reader.Value, json_type, inst_type));
            }

            object instance = null;

            if (reader.Token == JsonToken.ArrayStart)
            {

                AddArrayMetadata(inst_type);
                var t_data = ArrayMetadata[inst_type];

                if (!t_data.IsArray && !t_data.IsList)
                    throw new JsonException(string.Format(
                            "Type {0} can't act as an array",
                            inst_type));

                IList list;
                Type elem_type;

                if (!t_data.IsArray)
                {
                    list = (IList)Activator.CreateInstance(inst_type);
                    elem_type = t_data.ElementType;
                }
                else
                {
                    list = new ArrayList();
                    elem_type = inst_type.GetElementType();
                }

                while (true)
                {
                    var item = ReadValue(elem_type, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    list.Add(item);
                }

                if (t_data.IsArray)
                {
                    var n = list.Count;
                    instance = Array.CreateInstance(elem_type, n);

                    for (var i = 0; i < n; i++)
                        ((Array)instance).SetValue(list[i], i);
                }
                else
                    instance = list;

            }
            else if (reader.Token == JsonToken.ObjectStart)
            {

                AddObjectMetadata(inst_type);
                var t_data = ObjectMetadata[inst_type];

                instance = Activator.CreateInstance(inst_type);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    var property = (string)reader.Value;

                    if (t_data.Properties.ContainsKey(property))
                    {
                        var prop_data =
                            t_data.Properties[property];

                        if (prop_data.IsField)
                        {
                            ((FieldInfo)prop_data.Info).SetValue(
                                instance, ReadValue(prop_data.Type, reader));
                        }
                        else
                        {
                            var p_info =
                                (PropertyInfo)prop_data.Info;

                            if (p_info.CanWrite)
                                p_info.SetValue(
                                    instance,
                                    ReadValue(prop_data.Type, reader),
                                    null);
                            else
                                ReadValue(prop_data.Type, reader);
                        }

                    }
                    else
                    {
                        if (!t_data.IsDictionary)
                        {

                            if (!reader.SkipNonMembers)
                            {
                                throw new JsonException(string.Format(
                                        "The type {0} doesn't have the " +
                                        "property '{1}'",
                                        inst_type, property));
                            }
                            else
                            {
                                ReadSkip(reader);
                                continue;
                            }
                        }

                      ((IDictionary)instance).Add(
                          property, ReadValue(
                              t_data.ElementType, reader));
                    }

                }

            }

            return instance;
        }

        private static IJsonWrapper ReadValue(WrapperFactory factory,
                                               JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd ||
                reader.Token == JsonToken.Null)
                return null;

            var instance = factory();

            if (reader.Token == JsonToken.String)
            {
                instance.SetString((string)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Double)
            {
                instance.SetDouble((double)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Int)
            {
                instance.SetInt((int)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Long)
            {
                instance.SetLong((long)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Boolean)
            {
                instance.SetBoolean((bool)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.ArrayStart)
            {
                instance.SetJsonType(JsonType.Array);

                while (true)
                {
                    var item = ReadValue(factory, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                        break;

                    ((IList)instance).Add(item);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                instance.SetJsonType(JsonType.Object);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                        break;

                    var property = (string)reader.Value;

                    ((IDictionary)instance)[property] = ReadValue(
                        factory, reader);
                }

            }

            return instance;
        }

        private static void ReadSkip(JsonReader reader)
        {
            ToWrapper(
                delegate { return new JsonMockWrapper(); }, reader);
        }

        private static void RegisterBaseExporters()
        {
            BaseExportersTable[typeof(byte)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((byte)obj));
                };

            BaseExportersTable[typeof(char)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToString((char)obj));
                };

            BaseExportersTable[typeof(DateTime)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToString((DateTime)obj,
                                                    DateTimeFormat));
                };

            BaseExportersTable[typeof(decimal)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write((decimal)obj);
                };

            BaseExportersTable[typeof(sbyte)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((sbyte)obj));
                };

            BaseExportersTable[typeof(short)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((short)obj));
                };

            BaseExportersTable[typeof(ushort)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToInt32((ushort)obj));
                };

            BaseExportersTable[typeof(uint)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write(Convert.ToUInt64((uint)obj));
                };

            BaseExportersTable[typeof(ulong)] =
                delegate (object obj, JsonWriter writer)
                {
                    writer.Write((ulong)obj);
                };
        }

        private static void RegisterBaseImporters()
        {
            ImporterFunc importer;

            importer = delegate (object input)
            {
                return Convert.ToByte((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(byte), importer);

            importer = delegate (object input)
            {
                return Convert.ToUInt64((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(ulong), importer);

            importer = delegate (object input)
            {
                return Convert.ToSByte((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(sbyte), importer);

            importer = delegate (object input)
            {
                return Convert.ToInt16((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(short), importer);

            importer = delegate (object input)
            {
                return Convert.ToUInt16((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(ushort), importer);

            importer = delegate (object input)
            {
                return Convert.ToUInt32((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(uint), importer);

            importer = delegate (object input)
            {
                return Convert.ToSingle((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(float), importer);

            importer = delegate (object input)
            {
                return Convert.ToDouble((int)input);
            };
            RegisterImporter(BaseImportersTable, typeof(int),
                              typeof(double), importer);

            importer = delegate (object input)
            {
                return Convert.ToDecimal((double)input);
            };
            RegisterImporter(BaseImportersTable, typeof(double),
                              typeof(decimal), importer);


            importer = delegate (object input)
            {
                return Convert.ToUInt32((long)input);
            };
            RegisterImporter(BaseImportersTable, typeof(long),
                              typeof(uint), importer);

            importer = delegate (object input)
            {
                return Convert.ToChar((string)input);
            };
            RegisterImporter(BaseImportersTable, typeof(string),
                              typeof(char), importer);

            importer = delegate (object input)
            {
                return Convert.ToDateTime((string)input, DateTimeFormat);
            };
            RegisterImporter(BaseImportersTable, typeof(string),
                              typeof(DateTime), importer);
        }

        private static void RegisterImporter(
            IDictionary<Type, IDictionary<Type, ImporterFunc>> table,
            Type json_type, Type value_type, ImporterFunc importer)
        {
            if (!table.ContainsKey(json_type))
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());

            table[json_type][value_type] = importer;
        }

        private static void WriteValue(object obj, JsonWriter writer,
                                        bool writer_is_private,
                                        int depth)
        {
            if (depth > MaxNestingDepth)
                throw new JsonException(
                    string.Format("Max allowed object depth reached while " +
                                   "trying to export from type {0}",
                                   obj.GetType()));

            if (obj == null)
            {
                writer.Write(null);
                return;
            }

            if (obj is IJsonWrapper)
            {
                if (writer_is_private)
                    writer.TextWriter.Write(((IJsonWrapper)obj).ToJson());
                else
                    ((IJsonWrapper)obj).ToJson(writer);

                return;
            }

            if (obj is string)
            {
                writer.Write((string)obj);
                return;
            }

            if (obj is Double)
            {
                writer.Write((double)obj);
                return;
            }

            if (obj is Int32)
            {
                writer.Write((int)obj);
                return;
            }

            if (obj is Boolean)
            {
                writer.Write((bool)obj);
                return;
            }

            if (obj is Int64)
            {
                writer.Write((long)obj);
                return;
            }

            if (obj is Array)
            {
                writer.WriteArrayStart();

                foreach (var elem in (Array)obj)
                    WriteValue(elem, writer, writer_is_private, depth + 1);

                writer.WriteArrayEnd();

                return;
            }

            if (obj is IList)
            {
                writer.WriteArrayStart();
                foreach (var elem in (IList)obj)
                    WriteValue(elem, writer, writer_is_private, depth + 1);
                writer.WriteArrayEnd();

                return;
            }

            if (obj is IDictionary)
            {
                writer.WriteObjectStart();
                foreach (DictionaryEntry entry in (IDictionary)obj)
                {
                    writer.WritePropertyName((string)entry.Key);
                    WriteValue(entry.Value, writer, writer_is_private,
                                depth + 1);
                }
                writer.WriteObjectEnd();

                return;
            }

            var obj_type = obj.GetType();

            // See if there's a custom exporter for the object
            if (CustomExportersTable.ContainsKey(obj_type))
            {
                var exporter = CustomExportersTable[obj_type];
                exporter(obj, writer);

                return;
            }

            // If not, maybe there's a base exporter
            if (BaseExportersTable.ContainsKey(obj_type))
            {
                var exporter = BaseExportersTable[obj_type];
                exporter(obj, writer);

                return;
            }

            // Last option, let's see if it's an enum
            if (obj is Enum)
            {
                var e_type = Enum.GetUnderlyingType(obj_type);

                if (e_type == typeof(long)
                    || e_type == typeof(uint)
                    || e_type == typeof(ulong))
                    writer.Write((ulong)obj);
                else
                    writer.Write((int)obj);

                return;
            }

            // Okay, so it looks like the input should be exported as an
            // object
            AddTypeProperties(obj_type);
            var props = TypeProperties[obj_type];

            writer.WriteObjectStart();
            foreach (var p_data in props)
            {
                if (p_data.IsField)
                {
                    writer.WritePropertyName(p_data.Info.Name);
                    WriteValue(((FieldInfo)p_data.Info).GetValue(obj),
                                writer, writer_is_private, depth + 1);
                }
                else
                {
                    var p_info = (PropertyInfo)p_data.Info;

                    if (p_info.CanRead)
                    {
                        writer.WritePropertyName(p_data.Info.Name);
                        WriteValue(p_info.GetValue(obj, null),
                                    writer, writer_is_private, depth + 1);
                    }
                }
            }
            writer.WriteObjectEnd();
        }
        #endregion

        public static string ToJson(object obj)
        {
            lock (StaticWriterLock)
            {
                StaticWriter.Reset();

                WriteValue(obj, StaticWriter, true, 0);

                return StaticWriter.ToString();
            }
        }

        public static void ToJson(object obj, JsonWriter writer)
        {
            WriteValue(obj, writer, false, 0);
        }

        public static JsonData ToObject(JsonReader reader)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, reader);
        }

        public static JsonData ToObject(TextReader reader)
        {
            var jsonReader = new JsonReader(reader);

            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, jsonReader);
        }

        public static JsonData ToObject(string json)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, json);
        }

        public static T ToObject<T>(JsonReader reader)
        {
            return (T)ReadValue(typeof(T), reader);
        }

        public static T ToObject<T>(TextReader reader)
        {
            var jsonReader = new JsonReader(reader);

            return (T)ReadValue(typeof(T), jsonReader);
        }

        public static T ToObject<T>(string json)
        {
            var reader = new JsonReader(json);

            return (T)ReadValue(typeof(T), reader);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory,
                                              JsonReader reader)
        {
            return ReadValue(factory, reader);
        }

        public static IJsonWrapper ToWrapper(WrapperFactory factory,
                                              string json)
        {
            var reader = new JsonReader(json);

            return ReadValue(factory, reader);
        }

        public static void RegisterExporter<T>(ExporterFunc<T> exporter)
        {
            ExporterFunc exporter_wrapper =
                delegate (object obj, JsonWriter writer)
                {
                    exporter((T)obj, writer);
                };

            CustomExportersTable[typeof(T)] = exporter_wrapper;
        }

        public static void RegisterImporter<TJson, TValue>(
            ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc importer_wrapper =
                delegate (object input)
                {
                    return importer((TJson)input);
                };

            RegisterImporter(CustomImportersTable, typeof(TJson),
                              typeof(TValue), importer_wrapper);
        }

        public static void UnregisterExporters()
        {
            CustomExportersTable.Clear();
        }

        public static void UnregisterImporters()
        {
            CustomImportersTable.Clear();
        }
    }
}
