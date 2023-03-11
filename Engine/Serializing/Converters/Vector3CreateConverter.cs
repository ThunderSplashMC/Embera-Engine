using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Serializing.Converters
{
    class Vector3CreateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Vector3))
            {
                return true;
            }
            return false;
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            float Xval = (float)obj["X"];
            float Yval = (float)obj["Y"];
            float Zval = (float)obj["Z"];

            Vector3 vector3 = new Vector3(Xval, Yval, Zval);

            return vector3;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Console.WriteLine("Hello");
            Console.WriteLine(value);

            //JToken t = JToken.FromObject(value);

            //if (t.Type != JTokenType.Object)
            //{
            //    t.WriteTo(writer);
            //}
            //else
            //{
            //    JObject o = (JObject)t;
            //    o.Add("X", JToken.FromObject(value));

            //    //o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

            //    o.WriteTo(writer);
            //}
        }
    }
}
