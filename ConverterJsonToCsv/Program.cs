using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;

class Program
{
    static void Main()
    {
        // Ask for the directory path
        Console.Write("Enter the directory path: ");
        string directoryPath = Console.ReadLine();

        // Get all JSON files in the directory
        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");

        foreach (var jsonFilePath in jsonFiles)
        {
            // Read JSON data from file
            string jsonData = File.ReadAllText(jsonFilePath);

            // Deserialize JSON to a dynamic object
            var items = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonData);

            // Generate CSV data
            var csv = new StringBuilder();
            if (items.Count > 0)
            {
                // Add headers
                AddCsvRow(csv, items[0].Keys);

                // Add data
                foreach (var item in items)
                {
                    AddCsvRow(csv, item.Values);
                }
            }

            // Create a timestamp for the filename
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string csvFileName = $"Exported_{Path.GetFileNameWithoutExtension(jsonFilePath)}_{timestamp}.csv";
            string csvFilePath = Path.Combine(directoryPath, csvFileName);

            // Save CSV to file
            File.WriteAllText(csvFilePath, csv.ToString());

            // Inform the user
            Console.WriteLine($"CSV file has been saved: {csvFilePath}");
        }
    }

    static void AddCsvRow(StringBuilder csvBuilder, IEnumerable<object> fields)
    {
        string row = string.Join(",", fields);
        csvBuilder.AppendLine(row);
    }
}
