using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace MaxPainInfrastructure.Code
{
    public class DBHelper
    {
        #region DB Context
        public static SqlParameter CreateParm(string name, SqlDbType type, object value)
        {
            return new SqlParameter(name, type) { Value = value };
        }

        public static async Task<string> FetchJson(DatabaseFacade db, string sql, List<SqlParameter>? parms, int timeout = 30)
        {
            DataTable dt = new DataTable();
            using (var command = db.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                if (parms != null)
                {
                    command.Parameters.AddRange(parms.ToArray());
                }

                await db.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    dt.Load(reader);
                }
            }

            return SerializeDataTableJson(dt);
        }

        public static async Task<List<T>> FetchModel<T>(DatabaseFacade db, string sql, List<SqlParameter>? parms, int timeout = 30)
        {
            List<T> list = new List<T>();
            using (var command = db.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                if (parms != null)
                {
                    command.Parameters.AddRange(parms.ToArray());
                }

                await db.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    list = DataReaderMapToList<T>(reader);
                }
            }

            return list;
        }

        public static async Task<string> FetchContent(DatabaseFacade db, string sql, List<SqlParameter>? parms)
        {
            object result = await FetchScalar(db, sql, parms, "Content");
            return result?.ToString() ?? string.Empty;
        }

        public static async Task<object> FetchScalar(DatabaseFacade db, string sql, List<SqlParameter>? parms, string fieldName)
        {
            object? result = null;
            using (var command = db.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                if (parms != null)
                {
                    command.Parameters.AddRange(parms.ToArray());
                }

                await db.OpenConnectionAsync();
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (await reader.ReadAsync())
                    {
                        result = reader[fieldName];
                    }
                }
            }
            return result;
        }

        public static async Task<bool> Execute(DatabaseFacade db, string sql, List<SqlParameter>? parms, int timeout = 30)
        {
            using (var command = db.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.CommandTimeout = timeout;
                if (parms != null)
                {
                    command.Parameters.AddRange(parms.ToArray());
                }

                await db.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
            }
            return true;
        }

        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columnNames = dr.GetSchemaTable().Rows.Cast<DataRow>().Select(row => row["ColumnName"].ToString()).ToList();

            while (dr.Read())
            {
                T obj = Activator.CreateInstance<T>();
                foreach (var prop in properties)
                {
                    if (columnNames.Contains(prop.Name) && !object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name]);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        #endregion

        #region SQL Server
        public static async Task<DataTable> ExecuteQuery(string connStr, string sql, List<SqlParameter> parms)
        {
            return await ExecuteQuery(connStr, sql, parms, 30);
        }

        public static async Task<DataTable> ExecuteQuery(string connStr, string sql, List<SqlParameter> parms, int timeout)
        {
            DataTable dt = new DataTable();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandTimeout = timeout;

                    if (parms != null)
                    {
                        command.Parameters.AddRange(parms.ToArray());
                    }
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public static async Task<bool> ExecuteNonQuery(string connStr, string sql, List<SqlParameter> parms)
        {
            return await ExecuteNonQuery(connStr, sql, parms, 30);
        }

        public static async Task<bool> ExecuteNonQuery(string connStr, string sql, List<SqlParameter> parms, int timeout)
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandTimeout = timeout;

                    if (parms != null)
                    {
                        command.Parameters.AddRange(parms.ToArray());
                    }
                    await command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }
        #endregion

        #region Serialization
        private string SerializeNewtonsoft(object? obj)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.None,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None, settings);
        }

        public static string Serialize(object? obj)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            return JsonSerializer.Serialize(obj, options);
        }

        public static T Deserialize<T>(string json)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }

        public static XmlDocument SerializeDataTable(DataTable dt, string rootName, string nodeName)
        {
            return SerializeDataTable(dt, rootName, nodeName, true);
        }

        public static XmlDocument SerializeDataTable(DataTable dt, string rootName, string nodeName, bool useAttributes)
        {
            XmlDocument xmlDom = new XmlDocument();
            xmlDom.LoadXml($"<{rootName}/>");

            XmlElement? xmlRoot = xmlDom.DocumentElement;
            if (xmlRoot != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    XmlElement xmlRow = xmlDom.CreateElement(nodeName);
                    xmlRoot.AppendChild(xmlRow);
                    foreach (DataColumn col in dt.Columns)
                    {
                        string key = col.ColumnName;
                        string value = row[col] != DBNull.Value ? row[col].ToString() : string.Empty;

                        if (useAttributes)
                        {
                            xmlRow.SetAttribute(key, value);
                        }
                        else
                        {
                            XmlElement xmlField = xmlDom.CreateElement(key);
                            xmlRow.AppendChild(xmlField);
                            xmlField.InnerText = value;
                        }
                    }
                }
            }
            return xmlDom;
        }

        public static string SerializeDataTableJson(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return Serialize(rows);
        }
        #endregion

        #region HTML
        public static string DataTableToHTMLBootstrap(DataTable dt)
        {
            var html = new System.Text.StringBuilder();
            string alignRight = "style=\"text-align:right\"";

            string width = "1";
            if (dt.Columns.Count == 2) width = "6";
            if (dt.Columns.Count == 3) width = "4";
            if (dt.Columns.Count == 4) width = "3";
            if (dt.Columns.Count == 5 || dt.Columns.Count == 6) width = "2";

            html.AppendLine("<div class=\"container\">");
            html.AppendLine("<div class=\"row MyRowHead\">");
            foreach (DataColumn c in dt.Columns)
            {
                html.AppendFormat("<div class=\"col-xs-{0} MyCellHead\">{1}</div>\r\n", width, c.ColumnName);
            }
            html.AppendLine("</div>");

            foreach (DataRow r in dt.Rows)
            {
                html.AppendLine("<div class=\"row MyRowBody\">");
                foreach (DataColumn c in dt.Columns)
                {
                    string align = c.DataType == typeof(decimal) || c.DataType == typeof(double) || c.DataType == typeof(short) ||
                                   c.DataType == typeof(int) || c.DataType == typeof(long) || c.DataType == typeof(float) ? alignRight : string.Empty;

                    string value = r[c.ColumnName] != DBNull.Value ? r[c.ColumnName].ToString() : "&nbsp;";
                    html.AppendFormat("<div class=\"col-xs-{0} MyCellBody\" {1}>{2}</div>\r\n", width, align, value);
                }
                html.AppendLine("</div>");
            }
            html.AppendLine("</div>");
            return html.ToString();
        }
        #endregion

        #region CSV
        public static string DataTableToCSV(DataTable dt)
        {
            var result = new System.Text.StringBuilder();

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0)
                    result.Append(",");
                result.AppendFormat("\"{0}\"", dt.Columns[i].ColumnName);
            }
            result.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0)
                        result.Append(",");
                    result.AppendFormat("\"{0}\"", row[i] != DBNull.Value ? row[i].ToString() : string.Empty);
                }
                result.AppendLine();
            }
            return result.ToString();
        }
        #endregion
    }
}
