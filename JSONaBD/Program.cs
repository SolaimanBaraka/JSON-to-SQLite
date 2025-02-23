using System;
using System.IO;
using System.Data.SQLite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace JsonToSQLite
{
    class Program
    {
        static void Main()
        {
            Console.Write("Nombre del archivo JSON: ");
            string nomArxiu = Console.ReadLine();
            string jsonPath = $@"C:\Users\Solaiman Baraka\Desktop\JSON\{nomArxiu}.json";
            string dbPath = @"C:\Users\Solaiman Baraka\Desktop\Datos\database.db";
            
            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<JArray>(json);

            using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();

            foreach (JObject obj in data)
            {
                foreach (var property in obj.Properties())
                {
                    if (property.Value is JArray array)
                        CrearInsertarTabla(conn, property.Name, array);
                    else if (property.Value is JObject subObj)
                        CrearInsertarTabla(conn, property.Name, new JArray(subObj));
                }
            }

            Console.WriteLine("Datos importados correctamente");
        }

        static void CrearInsertarTabla(SQLiteConnection conn, string tableName, JArray array)
        {
            if (!array.Any()) return;

            var first = (JObject)array[0];
            var columns = string.Join(", ", first.Properties().Select(p => $"{p.Name} {GetSqlType(p.Value)}"));
            new SQLiteCommand($"CREATE TABLE IF NOT EXISTS {tableName} (id INTEGER PRIMARY KEY AUTOINCREMENT, {columns})", conn).ExecuteNonQuery();

            foreach (JObject item in array)
            {
                var keys = string.Join(", ", item.Properties().Select(p => p.Name));
                var values = string.Join(", ", item.Properties().Select(p => $"@{p.Name}"));
                var cmd = new SQLiteCommand($"INSERT INTO {tableName} ({keys}) VALUES ({values})", conn);

                foreach (var prop in item.Properties())
                    cmd.Parameters.AddWithValue($"@{prop.Name}", prop.Value.ToObject<object>());

                cmd.ExecuteNonQuery();
            }
        }

        static string GetSqlType(JToken token) =>
            token.Type switch
            {
                JTokenType.Integer => "INTEGER",
                JTokenType.Float => "REAL",
                JTokenType.String => "TEXT",
                JTokenType.Boolean => "INTEGER",
                _ => "TEXT"
            };
    }
}
