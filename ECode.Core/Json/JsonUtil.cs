using System;
using System.IO;

namespace ECode.Json
{
    public static class JsonUtil
    {
        static JsonSerializer   serializer   = new JsonSerializer();


        public static string Serialize(object value)
        {
            return serializer.Serialize(value);
        }

        public static string Serialize<T>(T value)
        {
            return serializer.Serialize(value, typeof(T));
        }

        public static string Serialize(object value, Type objectType)
        {
            return serializer.Serialize(value, objectType);
        }


        public static object Deserialize(string json)
        {
            return serializer.Deserialize(json);
        }

        public static T Deserialize<T>(string json)
        {
            return serializer.Deserialize<T>(json);
        }

        public static object Deserialize(string json, Type objectType)
        {
            return serializer.Deserialize(json, objectType);
        }

        public static object Deserialize(TextReader reader)
        {
            return serializer.Deserialize(reader);
        }

        public static T Deserialize<T>(TextReader reader)
        {
            return serializer.Deserialize<T>(reader);
        }

        public static object Deserialize(TextReader reader, Type objectType)
        {
            return serializer.Deserialize(reader, objectType);
        }
    }
}
