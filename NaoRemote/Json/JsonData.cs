//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace NaoRemote.Json
{
    /// <summary>
    /// Generic type to hold JSON data (objects, arrays, and so on). This is the default type returned by JsonMapper.ToObject().
    /// </summary>
    public class JsonData : IJsonWrapper, IEquatable<JsonData>
    {
        #region Fields
        private IList<JsonData> _instArray;
        private bool _instBoolean;
        private double _instDouble;
        private int _instInt;
        private long _instLong;
        private IDictionary<string, JsonData> _instObject;
        private string _instString;
        private string _json;
        private JsonType _type;
        // Used to implement the IOrderedDictionary interface
        private IList<KeyValuePair<string, JsonData>> _objectList;
        #endregion

        #region Properties
        public int Count
        {
            get { return EnsureCollection().Count; }
        }

        public bool IsArray
        {
            get { return _type == JsonType.Array; }
        }

        public bool IsBoolean
        {
            get { return _type == JsonType.Boolean; }
        }

        public bool IsDouble
        {
            get { return _type == JsonType.Double; }
        }

        public bool IsInt
        {
            get { return _type == JsonType.Int; }
        }

        public bool IsLong
        {
            get { return _type == JsonType.Long; }
        }

        public bool IsObject
        {
            get { return _type == JsonType.Object; }
        }

        public bool IsString
        {
            get { return _type == JsonType.String; }
        }
        #endregion

        #region ICollection Properties
        int ICollection.Count
        {
            get
            {
                return Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return EnsureCollection().IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return EnsureCollection().SyncRoot;
            }
        }
        #endregion

        #region IDictionary Properties
        bool IDictionary.IsFixedSize
        {
            get
            {
                return EnsureDictionary().IsFixedSize;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return EnsureDictionary().IsReadOnly;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                EnsureDictionary();
                IList<string> keys = new List<string>();

                foreach (var entry in
                         _objectList)
                {
                    keys.Add(entry.Key);
                }

                return (ICollection)keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                EnsureDictionary();
                IList<JsonData> values = new List<JsonData>();

                foreach (var entry in
                         _objectList)
                {
                    values.Add(entry.Value);
                }

                return (ICollection)values;
            }
        }
        #endregion

        #region IJsonWrapper Properties
        bool IJsonWrapper.IsArray
        {
            get { return IsArray; }
        }

        bool IJsonWrapper.IsBoolean
        {
            get { return IsBoolean; }
        }

        bool IJsonWrapper.IsDouble
        {
            get { return IsDouble; }
        }

        bool IJsonWrapper.IsInt
        {
            get { return IsInt; }
        }

        bool IJsonWrapper.IsLong
        {
            get { return IsLong; }
        }

        bool IJsonWrapper.IsObject
        {
            get { return IsObject; }
        }

        bool IJsonWrapper.IsString
        {
            get { return IsString; }
        }
        #endregion

        #region IList Properties
        bool IList.IsFixedSize
        {
            get
            {
                return EnsureList().IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return EnsureList().IsReadOnly;
            }
        }
        #endregion

        #region IDictionary Indexer
        object IDictionary.this[object key]
        {
            get
            {
                return EnsureDictionary()[key];
            }

            set
            {
                if (!(key is string))
                    throw new ArgumentException("The key has to be a string");

                var data = ToJsonData(value);

                this[(string)key] = data;
            }
        }
        #endregion

        #region IOrderedDictionary Indexer
        object IOrderedDictionary.this[int idx]
        {
            get
            {
                EnsureDictionary();
                return _objectList[idx].Value;
            }

            set
            {
                EnsureDictionary();
                var data = ToJsonData(value);
                var oldEntry = _objectList[idx];
                _instObject[oldEntry.Key] = data;
                var entry = new KeyValuePair<string, JsonData>(oldEntry.Key, data);
                _objectList[idx] = entry;
            }
        }
        #endregion

        #region IList Indexer
        object IList.this[int index]
        {
            get
            {
                return EnsureList()[index];
            }

            set
            {
                EnsureList();
                var data = ToJsonData(value);

                this[index] = data;
            }
        }
        #endregion

        #region Public Indexers
        public JsonData this[string propName]
        {
            get
            {
                EnsureDictionary();
                return _instObject[propName];
            }

            set
            {
                EnsureDictionary();

                var entry =
                    new KeyValuePair<string, JsonData>(propName, value);

                if (_instObject.ContainsKey(propName))
                {
                    for (var i = 0; i < _objectList.Count; i++)
                    {
                        if (_objectList[i].Key == propName)
                        {
                            _objectList[i] = entry;
                            break;
                        }
                    }
                }
                else
                    _objectList.Add(entry);

                _instObject[propName] = value;

                _json = null;
            }
        }

        public JsonData this[int index]
        {
            get
            {
                EnsureCollection();

                if (_type == JsonType.Array)
                    return _instArray[index];

                return _objectList[index].Value;
            }

            set
            {
                EnsureCollection();

                if (_type == JsonType.Array)
                    _instArray[index] = value;
                else
                {
                    var entry = _objectList[index];
                    var newEntry = new KeyValuePair<string, JsonData>(entry.Key, value);

                    _objectList[index] = newEntry;
                    _instObject[entry.Key] = value;
                }

                _json = null;
            }
        }
        #endregion

        #region Constructors
        public JsonData()
        {
        }

        public JsonData(bool boolean)
        {
            _type = JsonType.Boolean;
            _instBoolean = boolean;
        }

        public JsonData(double number)
        {
            _type = JsonType.Double;
            _instDouble = number;
        }

        public JsonData(int number)
        {
            _type = JsonType.Int;
            _instInt = number;
        }

        public JsonData(long number)
        {
            _type = JsonType.Long;
            _instLong = number;
        }

        public JsonData(object obj)
        {
            if (obj is Boolean)
            {
                _type = JsonType.Boolean;
                _instBoolean = (bool)obj;
                return;
            }

            if (obj is Double)
            {
                _type = JsonType.Double;
                _instDouble = (double)obj;
                return;
            }

            if (obj is Int32)
            {
                _type = JsonType.Int;
                _instInt = (int)obj;
                return;
            }

            if (obj is Int64)
            {
                _type = JsonType.Long;
                _instLong = (long)obj;
                return;
            }

            if (obj is string)
            {
                _type = JsonType.String;
                _instString = (string)obj;
                return;
            }

            throw new ArgumentException("Unable to wrap the given object with JsonData");
        }

        public JsonData(string str)
        {
            _type = JsonType.String;
            _instString = str;
        }
        #endregion

        #region Implicit Conversions
        public static implicit operator JsonData(Boolean data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Double data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Int32 data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(Int64 data)
        {
            return new JsonData(data);
        }

        public static implicit operator JsonData(string data)
        {
            return new JsonData(data);
        }
        #endregion

        #region Explicit Conversions
        public static explicit operator Boolean(JsonData data)
        {
            if (data._type != JsonType.Boolean)
                throw new InvalidCastException("Instance of JsonData doesn't hold a double");

            return data._instBoolean;
        }

        public static explicit operator Double(JsonData data)
        {
            if (data._type != JsonType.Double)
                throw new InvalidCastException("Instance of JsonData doesn't hold a double");

            return data._instDouble;
        }

        public static explicit operator Int32(JsonData data)
        {
            if (data._type != JsonType.Int)
                throw new InvalidCastException("Instance of JsonData doesn't hold an int");

            return data._instInt;
        }

        public static explicit operator Int64(JsonData data)
        {
            if (data._type != JsonType.Long)
                throw new InvalidCastException("Instance of JsonData doesn't hold an int");

            return data._instLong;
        }

        public static explicit operator string(JsonData data)
        {
            if (data._type != JsonType.String)
                throw new InvalidCastException("Instance of JsonData doesn't hold a string");

            return data._instString;
        }
        #endregion

        #region ICollection Methods
        void ICollection.CopyTo(Array array, int index)
        {
            EnsureCollection().CopyTo(array, index);
        }
        #endregion

        #region IDictionary Methods
        void IDictionary.Add(object key, object value)
        {
            var data = ToJsonData(value);

            EnsureDictionary().Add(key, data);

            var entry =
                new KeyValuePair<string, JsonData>((string)key, data);
            _objectList.Add(entry);

            _json = null;
        }

        void IDictionary.Clear()
        {
            EnsureDictionary().Clear();
            _objectList.Clear();
            _json = null;
        }

        bool IDictionary.Contains(object key)
        {
            return EnsureDictionary().Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary)this).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            EnsureDictionary().Remove(key);

            for (var i = 0; i < _objectList.Count; i++)
            {
                if (_objectList[i].Key == (string)key)
                {
                    _objectList.RemoveAt(i);
                    break;
                }
            }

            _json = null;
        }
        #endregion

        #region IEnumerable Methods
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnsureCollection().GetEnumerator();
        }
        #endregion

        #region IJsonWrapper Methods
        bool IJsonWrapper.GetBoolean()
        {
            if (_type != JsonType.Boolean)
                throw new InvalidOperationException("JsonData instance doesn't hold a boolean");

            return _instBoolean;
        }

        double IJsonWrapper.GetDouble()
        {
            if (_type != JsonType.Double)
                throw new InvalidOperationException("JsonData instance doesn't hold a double");

            return _instDouble;
        }

        int IJsonWrapper.GetInt()
        {
            if (_type != JsonType.Int)
                throw new InvalidOperationException("JsonData instance doesn't hold an int");

            return _instInt;
        }

        long IJsonWrapper.GetLong()
        {
            if (_type != JsonType.Long)
                throw new InvalidOperationException("JsonData instance doesn't hold a long");

            return _instLong;
        }

        string IJsonWrapper.GetString()
        {
            if (_type != JsonType.String)
                throw new InvalidOperationException("JsonData instance doesn't hold a string");

            return _instString;
        }

        void IJsonWrapper.SetBoolean(bool val)
        {
            _type = JsonType.Boolean;
            _instBoolean = val;
            _json = null;
        }

        void IJsonWrapper.SetDouble(double val)
        {
            _type = JsonType.Double;
            _instDouble = val;
            _json = null;
        }

        void IJsonWrapper.SetInt(int val)
        {
            _type = JsonType.Int;
            _instInt = val;
            _json = null;
        }

        void IJsonWrapper.SetLong(long val)
        {
            _type = JsonType.Long;
            _instLong = val;
            _json = null;
        }

        void IJsonWrapper.SetString(string val)
        {
            _type = JsonType.String;
            _instString = val;
            _json = null;
        }

        string IJsonWrapper.ToJson()
        {
            return ToJson();
        }

        void IJsonWrapper.ToJson(JsonWriter writer)
        {
            ToJson(writer);
        }
        #endregion

        #region IList Methods
        int IList.Add(object value)
        {
            return Add(value);
        }

        void IList.Clear()
        {
            EnsureList().Clear();
            _json = null;
        }

        bool IList.Contains(object value)
        {
            return EnsureList().Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return EnsureList().IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            EnsureList().Insert(index, value);
            _json = null;
        }

        void IList.Remove(object value)
        {
            EnsureList().Remove(value);
            _json = null;
        }

        void IList.RemoveAt(int index)
        {
            EnsureList().RemoveAt(index);
            _json = null;
        }
        #endregion

        #region IOrderedDictionary Methods
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            EnsureDictionary();

            return new OrderedDictionaryEnumerator(_objectList.GetEnumerator());
        }

        void IOrderedDictionary.Insert(int idx, object key, object value)
        {
            var property = (string)key;
            var data = ToJsonData(value);

            this[property] = data;

            var entry = new KeyValuePair<string, JsonData>(property, data);

            _objectList.Insert(idx, entry);
        }

        void IOrderedDictionary.RemoveAt(int idx)
        {
            EnsureDictionary();

            _instObject.Remove(_objectList[idx].Key);
            _objectList.RemoveAt(idx);
        }
        #endregion

        #region Private Methods
        private ICollection EnsureCollection()
        {
            if (_type == JsonType.Array)
                return (ICollection)_instArray;

            if (_type == JsonType.Object)
                return (ICollection)_instObject;

            throw new InvalidOperationException("The JsonData instance has to be initialized first");
        }

        private IDictionary EnsureDictionary()
        {
            if (_type == JsonType.Object)
                return (IDictionary)_instObject;

            if (_type != JsonType.None)
                throw new InvalidOperationException("Instance of JsonData is not a dictionary");

            _type = JsonType.Object;
            _instObject = new Dictionary<string, JsonData>();
            _objectList = new List<KeyValuePair<string, JsonData>>();

            return (IDictionary)_instObject;
        }

        private IList EnsureList()
        {
            if (_type == JsonType.Array)
                return (IList)_instArray;

            if (_type != JsonType.None)
                throw new InvalidOperationException("Instance of JsonData is not a list");

            _type = JsonType.Array;
            _instArray = new List<JsonData>();

            return (IList)_instArray;
        }

        private JsonData ToJsonData(object obj)
        {
            if (obj == null)
                return null;

            if (obj is JsonData)
                return (JsonData)obj;

            return new JsonData(obj);
        }

        private static void WriteJson(IJsonWrapper obj, JsonWriter writer)
        {
            if (obj == null)
            {
                writer.Write(null);
                return;
            }

            if (obj.IsString)
            {
                writer.Write(obj.GetString());
                return;
            }

            if (obj.IsBoolean)
            {
                writer.Write(obj.GetBoolean());
                return;
            }

            if (obj.IsDouble)
            {
                writer.Write(obj.GetDouble());
                return;
            }

            if (obj.IsInt)
            {
                writer.Write(obj.GetInt());
                return;
            }

            if (obj.IsLong)
            {
                writer.Write(obj.GetLong());
                return;
            }

            if (obj.IsArray)
            {
                writer.WriteArrayStart();
                foreach (var elem in (IList)obj)
                    WriteJson((JsonData)elem, writer);
                writer.WriteArrayEnd();

                return;
            }

            if (obj.IsObject)
            {
                writer.WriteObjectStart();

                foreach (DictionaryEntry entry in ((IDictionary)obj))
                {
                    writer.WritePropertyName((string)entry.Key);
                    WriteJson((JsonData)entry.Value, writer);
                }
                writer.WriteObjectEnd();
            }
        }
        #endregion

        public int Add(object value)
        {
            var data = ToJsonData(value);
            _json = null;

            return EnsureList().Add(data);
        }

        public void Clear()
        {
            if (IsObject)
            {
                ((IDictionary)this).Clear();
                return;
            }

            if (IsArray)
            {
                ((IList)this).Clear();
            }
        }

        public bool Equals(JsonData x)
        {
            if (x == null)
                return false;

            if (x._type != _type)
                return false;

            switch (_type)
            {
                case JsonType.None:
                    return true;

                case JsonType.Object:
                    return _instObject.Equals(x._instObject);

                case JsonType.Array:
                    return _instArray.Equals(x._instArray);

                case JsonType.String:
                    return _instString.Equals(x._instString);

                case JsonType.Int:
                    return _instInt.Equals(x._instInt);

                case JsonType.Long:
                    return _instLong.Equals(x._instLong);

                case JsonType.Double:
                    return _instDouble.Equals(x._instDouble);

                case JsonType.Boolean:
                    return _instBoolean.Equals(x._instBoolean);
            }

            return false;
        }

        public JsonType GetJsonType()
        {
            return _type;
        }

        public void SetJsonType(JsonType type)
        {
            if (_type == type)
                return;

            switch (type)
            {
                case JsonType.None:
                    break;

                case JsonType.Object:
                    _instObject = new Dictionary<string, JsonData>();
                    _objectList = new List<KeyValuePair<string, JsonData>>();
                    break;

                case JsonType.Array:
                    _instArray = new List<JsonData>();
                    break;

                case JsonType.String:
                    _instString = default(string);
                    break;

                case JsonType.Int:
                    _instInt = default(Int32);
                    break;

                case JsonType.Long:
                    _instLong = default(Int64);
                    break;

                case JsonType.Double:
                    _instDouble = default(Double);
                    break;

                case JsonType.Boolean:
                    _instBoolean = default(Boolean);
                    break;
            }

            _type = type;
        }

        public string ToJson()
        {
            if (_json != null)
                return _json;

            var sw = new StringWriter();
            var writer = new JsonWriter(sw);
            writer.Validate = false;

            WriteJson(this, writer);
            _json = sw.ToString();

            return _json;
        }

        public void ToJson(JsonWriter writer)
        {
            var oldValidate = writer.Validate;
            writer.Validate = false;
            WriteJson(this, writer);
            writer.Validate = oldValidate;
        }

        public override string ToString()
        {
            switch (_type)
            {
                case JsonType.Array:
                    return "JsonData array";

                case JsonType.Boolean:
                    return _instBoolean.ToString();

                case JsonType.Double:
                    return _instDouble.ToString(CultureInfo.InvariantCulture);

                case JsonType.Int:
                    return _instInt.ToString(CultureInfo.InvariantCulture);

                case JsonType.Long:
                    return _instLong.ToString(CultureInfo.InvariantCulture);

                case JsonType.Object:
                    return "JsonData object";

                case JsonType.String:
                    return _instString;
            }

            return "Uninitialized JsonData";
        }
    }
}
