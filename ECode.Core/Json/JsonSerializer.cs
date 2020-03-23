using System;
using System.IO;
using ECode.Core;
using Newtonsoft.Json.Serialization;
using NewtonsoftJson = Newtonsoft.Json;

namespace ECode.Json
{
    public class JsonSerializer
    {
        NewtonsoftJson.JsonSerializer   internalSerializer          = null;
        IContractResolver               defaultContractResolver     = null;


        public string DateFormatString
        {
            get { return internalSerializer.DateFormatString; }

            set { internalSerializer.DateFormatString = value; }
        }

        public NewtonsoftJson.JsonConverterCollection Converters
        {
            get { return internalSerializer.Converters; }
        }


        public bool IgnoreLoopReference
        {
            get { return internalSerializer.ReferenceLoopHandling == NewtonsoftJson.ReferenceLoopHandling.Ignore; }

            set
            {
                internalSerializer.ReferenceLoopHandling
                    = value ? NewtonsoftJson.ReferenceLoopHandling.Ignore : NewtonsoftJson.ReferenceLoopHandling.Error;
            }
        }

        public bool UseCamelCasePropertyNames
        {
            get
            {
                if (internalSerializer.ContractResolver == null)
                { return false; }

                return internalSerializer.ContractResolver.GetType() == typeof(CamelCasePropertyNamesContractResolver);
            }
            set
            {
                if (true == value)
                {
                    internalSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }
                else
                {
                    internalSerializer.ContractResolver = defaultContractResolver;
                }
            }
        }


        public JsonSerializer()
        {
            internalSerializer = new Newtonsoft.Json.JsonSerializer();
            defaultContractResolver = internalSerializer.ContractResolver;
        }


        public string Serialize(object value)
        {
            using (var writer = new StringWriter())
            using (var jsonWriter = new NewtonsoftJson.JsonTextWriter(writer))
            {
                internalSerializer.Serialize(jsonWriter, value);
                return writer.ToString();
            }
        }

        public string Serialize<T>(T value)
        {
            return Serialize(value, typeof(T));
        }

        public string Serialize(object value, Type objectType)
        {
            using (var writer = new StringWriter())
            using (var jsonWriter = new NewtonsoftJson.JsonTextWriter(writer))
            {
                internalSerializer.Serialize(jsonWriter, value, objectType);
                return writer.ToString();
            }
        }


        public object Deserialize(string json)
        {
            using (var reader = new System.IO.StringReader(json))
            using (var jsonReader = new NewtonsoftJson.JsonTextReader(reader))
            {
                return internalSerializer.Deserialize(jsonReader);
            }
        }

        public T Deserialize<T>(string json)
        {
            using (var reader = new System.IO.StringReader(json))
            using (var jsonReader = new NewtonsoftJson.JsonTextReader(reader))
            {
                return internalSerializer.Deserialize<T>(jsonReader);
            }
        }

        public object Deserialize(string json, Type objectType)
        {
            using (var reader = new System.IO.StringReader(json))
            using (var jsonReader = new NewtonsoftJson.JsonTextReader(reader))
            {
                return internalSerializer.Deserialize(jsonReader, objectType);
            }
        }

        public object Deserialize(TextReader reader)
        {
            using (var jsonReader = new NewtonsoftJson.JsonTextReader(reader))
            {
                return internalSerializer.Deserialize(jsonReader);
            }
        }

        public T Deserialize<T>(TextReader reader)
        {
            using (var jsonReader = new NewtonsoftJson.JsonTextReader(reader))
            {
                return internalSerializer.Deserialize<T>(jsonReader);
            }
        }

        public object Deserialize(TextReader reader, Type objectType)
        {
            using (var jsonReader = new NewtonsoftJson.JsonTextReader(reader))
            {
                return internalSerializer.Deserialize(jsonReader, objectType);
            }
        }
    }


    public sealed class UnixTimeConverter : NewtonsoftJson.JsonConverter
    {
        static readonly Type    TYPE_INT64                 = typeof(Int64);
        static readonly Type    TYPE_DATETIME              = typeof(DateTime);
        static readonly Type    TYPE_NULLABLE_DATETIME     = typeof(DateTime?);


        public override bool CanRead
        { get { return true; } }

        public override bool CanWrite
        { get { return true; } }


        public override bool CanConvert(Type objectType)
        {
            return objectType == TYPE_INT64 || objectType == TYPE_DATETIME || objectType == TYPE_NULLABLE_DATETIME;
        }

        public override object ReadJson(NewtonsoftJson.JsonReader reader, Type objectType, object existingValue, NewtonsoftJson.JsonSerializer serializer)
        {
            if (TYPE_INT64 == reader.ValueType)
            {
                if (objectType == TYPE_DATETIME || objectType == TYPE_NULLABLE_DATETIME)
                {
                    return DateTimeExtensions.TIMESTAMP_BASE.AddSeconds((long)reader.Value);
                }
                else
                {
                    return reader.Value;
                }
            }
            else if (TYPE_DATETIME == reader.ValueType || TYPE_NULLABLE_DATETIME == reader.ValueType)
            {
                return reader.Value;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(NewtonsoftJson.JsonWriter writer, object value, NewtonsoftJson.JsonSerializer serializer)
        {
            if (value is Int64)
            {
                writer.WriteValue((long)value);
            }
            else if (value is DateTime)
            {
                writer.WriteValue(((DateTime)value).ToUnixTimeStamp());
            }
            else if (value is DateTime?)
            {
                if (((DateTime?)value).HasValue)
                {
                    writer.WriteValue(((DateTime?)value).Value.ToUnixTimeStamp());
                }
                else
                {
                    writer.WriteNull();
                }
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
