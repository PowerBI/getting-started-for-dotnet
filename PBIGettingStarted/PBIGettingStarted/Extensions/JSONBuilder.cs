// Copyright Microsoft 2014

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PowerBIExtensionMethods
{
    //JSONBuilder used for PowerBI
    public static class JSONBuilder 
    {
        public static string ToJson(this object obj, JavaScriptSerializer serializer)
        {
            StringBuilder jsonBuilder = new StringBuilder();

            jsonBuilder.Append(string.Format("{0}\"rows\":", "{"));
            jsonBuilder.Append(serializer.Serialize(obj));
            jsonBuilder.Append(string.Format("{0}", "}"));

           return jsonBuilder.ToString();
        }

        public static string ToJsonSchema(this object obj, string name)
        {        
            StringBuilder jsonSchemaBuilder = new StringBuilder();
            string typeName = string.Empty;

            jsonSchemaBuilder.Append(string.Format("{0}\"name\": \"{1}\",\"tables\": [", "{", name));
            jsonSchemaBuilder.Append(String.Format("{0}\"name\": \"{1}\", ", "{", obj.GetType().Name));
            jsonSchemaBuilder.Append("\"columns\": [");

            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach(PropertyInfo p in properties)
            {  
                switch (p.PropertyType.Name)
                {                   
                    case "Int32": case "Int64":
                        typeName = "Int64";
                        break;
                    case "Double":
                        typeName = "Double";
                        break;
                    case "Boolean":
                        typeName = "bool";
                        break;
                    case "DateTime":
                        typeName = "DateTime";
                        break;
                    case "String":
                        typeName = "string";
                        break;
                    default:
                        typeName = null;
                        break;
                }

                if (typeName == null)
                    throw new Exception("type not supported");

                jsonSchemaBuilder.Append(string.Format("{0} \"name\": \"{1}\", \"dataType\": \"{2}\"{3},", "{", p.Name, typeName, "}"));
            }

            jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 1, 1);
            jsonSchemaBuilder.Append("]}]}");

            return jsonSchemaBuilder.ToString();
        }

        public static T ToObject<T>(this string obj, int recursionDepth = 100)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;

            string result = obj.Split(new Char[] { '[' })[1];
            result = (result.EndsWith("}")) ? result = result.Substring(0, result.Length - 1) : result;
            result = String.Format("[{0}", result);

            return serializer.Deserialize<T>(result);
        }

        public static IEnumerable<Dictionary<string, object>> Datasets(this List<Object>datasets , string name)
        {
            IEnumerable<Dictionary<string, object>> q = from d in (from d in datasets select d as Dictionary<string, object>) where d["name"] as string == name select d;

            return q;
        }
    }

    public class JavaScriptConverter<T> : JavaScriptConverter where T : new()
    {
        private const string _dateFormat = "MM/dd/yyyy";

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new[] { typeof(T) };
            }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            T p = new T();

            var props = typeof(T).GetProperties();

            foreach (string key in dictionary.Keys)
            {
                var prop = props.Where(t => t.Name == key).FirstOrDefault();
                if (prop != null)
                {
                    if (prop.PropertyType == typeof(DateTime))
                    {
                        prop.SetValue(p, DateTime.ParseExact(dictionary[key] as string, _dateFormat, DateTimeFormatInfo.InvariantInfo), null);
                    }
                    else
                    {
                        prop.SetValue(p, dictionary[key], null);
                    }
                }
            }

            return p;
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            T p = (T)obj;
            IDictionary<string, object> serialized = new Dictionary<string, object>();

            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                if (pi.PropertyType == typeof(DateTime))
                {
                    serialized[pi.Name] = ((DateTime)pi.GetValue(p, null)).ToString(_dateFormat);
                }
                else
                {
                    serialized[pi.Name] = pi.GetValue(p, null);
                }

            }


            StringBuilder powerBIJson = new StringBuilder();


            return serialized;
        }

        public static JavaScriptSerializer GetSerializer()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new[] { new JavaScriptConverter<T>() });

            return serializer;
        }
    }
}


