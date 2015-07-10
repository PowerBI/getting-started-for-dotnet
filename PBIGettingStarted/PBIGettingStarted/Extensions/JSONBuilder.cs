// Copyright Microsoft 2015

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using PBIGettingStarted;

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

        public static string ToJson(this IDataReader reader)
        {
            StringBuilder jsonBuilder = new StringBuilder();

            jsonBuilder.Append(string.Format("{0}\"rows\":", "{"));
            jsonBuilder.Append(JsonFromDataReader(reader));

            jsonBuilder.Append(string.Format("{0}", "}"));

            return jsonBuilder.ToString();
        }

        public static String JsonFromDataReader(IDataReader reader)
        {
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            int length = columnNames.Count;

            String res = "[";

            while (reader.Read())
            {
                res += "{";

                for (int i = 0; i < length; i++)
                {
                    res += "\"" + columnNames[i] + "\":\"" + reader[columnNames[i]].ToString() + "\"";                   
                    if (i < length - 1)
                        res += ",";
                }

                res += "},";
            }

            res = res.Remove(res.Length - 1, 1);

            res += "]";

            return res;
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
                string sPropertyTypeName = p.PropertyType.Name;
                if (sPropertyTypeName.StartsWith("Nullable") && p.PropertyType.GenericTypeArguments != null && p.PropertyType.GenericTypeArguments.Length == 1)
                    sPropertyTypeName = p.PropertyType.GenericTypeArguments[0].Name;
                switch (sPropertyTypeName)
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

        public static string ToJsonSchema(this SqlConnection sqlConnection, string name, string viewName)
        {
            StringBuilder jsonSchemaBuilder = new StringBuilder();

            jsonSchemaBuilder.Append(string.Format("{0}\"name\": \"{1}\",\"tables\": [", "{", name));
            jsonSchemaBuilder.Append(String.Format("{0}\"name\": \"{1}\", ", "{", viewName));
            jsonSchemaBuilder.Append("\"columns\": [");

            string json = String.Concat(from r in sqlConnection.GetSchema("Columns").AsEnumerable()
                           where r.Field<string>("TABLE_NAME") == viewName
                          orderby r.Field<int>("ORDINAL_POSITION")
                                  select 
                                  string.Format("{0} \"name\":\"{1}\", \"dataType\": \"{2}\"{3}, ", "{", r.Field<string>("COLUMN_NAME"), ConvertSqlType(r.Field<string>("DATA_TYPE")), "}")
                           );

            jsonSchemaBuilder.Append(json);
            jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 2, 2);
            jsonSchemaBuilder.Append("]}]}");

            return jsonSchemaBuilder.ToString();
        }

        public static IEnumerable<Dataset> GetDataset(this Dataset[] datasets, string name)
        {
            var q = from d in datasets where d.Name == name select d;

            return q;
        }

        private static string ConvertSqlType(string sqlType)
        {
            string jsonType = string.Empty;

            switch (sqlType)
            {
                case "int":
                case "smallint":
                case "bigint":
                    jsonType = "Int64";
                    break;
                case "decimal":
                case "money":
                    jsonType = "Double";
                    break;
                case "bit":
                    jsonType = "bool";
                    break;
                case "datetime":
                    jsonType = "DateTime";
                    break;
                case "nvarchar":
                    jsonType = "string";
                    break;
                default:
                    jsonType = null;
                    break;
            }

            return jsonType;

        }

        public static string ToJsonSchema(this DataTable dataTable)
        {
            StringBuilder jsonSchemaBuilder = new StringBuilder();
            string typeName = string.Empty;

            jsonSchemaBuilder.Append(string.Format("{0}\"name\": \"{1}\",\"tables\": [", "{", dataTable.DataSet.DataSetName));
            jsonSchemaBuilder.Append(String.Format("{0}\"name\": \"{1}\", ", "{", dataTable.TableName));
            jsonSchemaBuilder.Append("\"columns\": [");

            foreach (DataColumn dc in dataTable.Columns)
            {
                jsonSchemaBuilder.Append(string.Format("{0} \"name\": \"{1}\", \"dataType\": \"{2}\"{3},", "{", dc.ColumnName, dc.DataType.Name, "}"));
            }

            jsonSchemaBuilder.Remove(jsonSchemaBuilder.ToString().Length - 1, 1);
            jsonSchemaBuilder.Append("]}]}");

            return jsonSchemaBuilder.ToString();
        }

        public static string ToJson(this DataTable dataTable)
        {
            StringBuilder jsonBuilder = new StringBuilder();

            jsonBuilder.Append(string.Format("{0}\"rows\":", "{"));
            jsonBuilder.Append(JsonFromDataTable(dataTable));

            jsonBuilder.Append(string.Format("{0}", "}"));

            return jsonBuilder.ToString();
        }

        private static String JsonFromDataTable(DataTable dataTable)
        {
            var columnNames = dataTable.Columns;

            int length = columnNames.Count;

            String res = "[";

            foreach (DataRow row in dataTable.Rows)
            {
                res += "{";

                for (int i = 0; i < length; i++)
                {
                    res += "\"" + columnNames[i] + "\":\"" + row[columnNames[i]].ToString() + "\"";
                    if (i < length - 1)
                        res += ",";
                }

                res += "},";
            }

            res = res.Remove(res.Length - 1, 1);

            res += "]";

            return res;
        }
    }

    public class JsonColumn
    {
        public string Name { get; set; }
        public string DataType { get; set; }
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
