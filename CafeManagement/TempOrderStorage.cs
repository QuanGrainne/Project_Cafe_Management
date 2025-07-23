using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CafeManagement.Models;

namespace CafeManagement
{
    public static class TempOrderStorage
    {
        private static string path = "temp_orders.json";

        public static void Save(Dictionary<int, TempOrderData> data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            File.WriteAllText(path, JsonSerializer.Serialize(data, options));
        }

        public static Dictionary<int, TempOrderData> Load()
        {
            try
            {
                if (!File.Exists(path)) return new();

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };

                return JsonSerializer.Deserialize<Dictionary<int, TempOrderData>>(File.ReadAllText(path), options)
                       ?? new();
            }
            catch
            {
                // Nếu lỗi JSON → xóa file để tránh crash app
                File.Delete(path);
                return new();
            }
        }

        public static void Clear()
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
