using System;
using System.IO;
using System.Collections.Generic;

namespace EmvIdGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var emvIds = GenerateEmvIds(1000); // Generates 1000 EMV IDs
            File.WriteAllLines("emv_ids.txt", emvIds);
            Console.WriteLine("EMV IDs generated successfully!");
        }

        static List<string> GenerateEmvIds(int count)
        {
            var ids = new List<string>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                // Generate a random 16-digit EMV ID
                string id = "";
                for (int j = 0; j < 16; j++)
                {
                    id += random.Next(0, 10).ToString();
                }
                ids.Add(id);
            }

            return ids;
        }
    }
}