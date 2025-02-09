using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace EmvIdGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var count = args.Length > 0 ? int.Parse(args[0]) : 1000000;
            var blacklistPercentage = args.Length > 1 ? double.Parse(args[1]) : 0.1; // 10% blacklisted by default

            Console.WriteLine($"Generating {count} EMV IDs ({blacklistPercentage * 100}% blacklisted)...");
            
            var (emvIds, blacklist) = GenerateEmvIds(count, blacklistPercentage);
            
            // Write all EMV IDs
            File.WriteAllLines("emv_ids.txt", emvIds);
            
            // Write blacklisted IDs
            File.WriteAllLines("blacklist.txt", blacklist);
            
            Console.WriteLine($"Generated {emvIds.Count} EMV IDs with {blacklist.Count} blacklisted.");
        }

        static (List<string> allIds, HashSet<string> blacklist) GenerateEmvIds(int count, double blacklistPercentage)
        {
            var allIds = new List<string>(count);
            var blacklist = new HashSet<string>();
            var random = new Random();
            
            // Generate IDs with different patterns
            for (int i = 0; i < count; i++)
            {
                string id;
                if (i % 3 == 0)
                {
                    // Standard random 16-digit
                    id = GenerateRandomEmvId(random);
                }
                else if (i % 3 == 1)
                {
                    // Sequential with checksum
                    id = GenerateSequentialEmvId(i);
                }
                else
                {
                    // Pattern-based
                    id = GeneratePatternEmvId(i, random);
                }
                
                allIds.Add(id);
                
                // Add to blacklist based on percentage
                if (random.NextDouble() < blacklistPercentage)
                {
                    blacklist.Add(id);
                }
            }
            
            return (allIds, blacklist);
        }

        static string GenerateRandomEmvId(Random random)
        {
            var id = new StringBuilder(16);
            for (int i = 0; i < 16; i++)
            {
                id.Append(random.Next(0, 10));
            }
            return id.ToString();
        }

        static string GenerateSequentialEmvId(int index)
        {
            return index.ToString("D16");
        }

        static string GeneratePatternEmvId(int index, Random random)
        {
            // Create patterns like 4532XXXXXXXX1234
            return $"4532{random.Next(0, 100000000):D8}1234";
        }
    }
}