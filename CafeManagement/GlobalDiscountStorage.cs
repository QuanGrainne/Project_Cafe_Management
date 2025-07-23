using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CafeManagement
{
    public static class GlobalDiscountStorage
    {
        private static string path = "discount.txt";

        public static void Save(decimal discount)
        {
            File.WriteAllText(path, discount.ToString());
        }

        public static decimal Load()
        {
            if (File.Exists(path) && decimal.TryParse(File.ReadAllText(path), out decimal discount))
                return discount;

            return 0;
        }
    }
}