using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

class Program
{
    static void Main()
    {
        Console.Write("Enter the directory path: ");
        string directoryPath = Console.ReadLine();

        string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");
        foreach (var jsonFilePath in jsonFiles)
        {
            string jsonData = File.ReadAllText(jsonFilePath);
            JToken jsonToken = JToken.Parse(jsonData);

            string baseFileName = Path.GetFileNameWithoutExtension(jsonFilePath);
            string csvFilePath = Path.Combine(directoryPath, $"{baseFileName}_combined.csv");
            var csv = new StringBuilder();

            // Start the process based on the type of the root token
            if (jsonToken.Type == JTokenType.Object)
            {
                ProcessJObject((JObject)jsonToken, csv, new List<string>());
            }
            else if (jsonToken.Type == JTokenType.Array)
            {
                ProcessJArray((JArray)jsonToken, csv, new List<string>(), "Root");
            }

            File.WriteAllText(csvFilePath, csv.ToString());
            Console.WriteLine($"Combined CSV file has been saved: {csvFilePath}");
        }
    }

    static void ProcessJObject(JObject jObject, StringBuilder csv, List<string> parentKeys)
    {
        var headers = new List<string> { "Serial Number" };
        var values = new List<string>();
        int serialNumber = 1;
        bool hasSubItems = false;

        foreach (var property in jObject.Properties())
        {
            if (property.Value is JObject subObject)
            {
                ProcessJObject(subObject, csv, new List<string>(parentKeys) { property.Name });
            }
            else if (property.Value is JArray array)
            {
                if (property.Name == "Skills")
                {
                    hasSubItems = true;
                    continue; // Skip adding to headers, handle separately
                }
                ProcessJArray(array, csv, new List<string>(parentKeys) { property.Name }, property.Name);
            }
            else
            {
                headers.Add(property.Name);
                values.Add(property.Value.ToString());
            }
        }

        if (values.Count > 0)
        {
            // Print headers
            csv.AppendLine(string.Join(",", headers));
            // Print values
            csv.AppendLine($"{serialNumber++},{string.Join(",", values)}");
            if (hasSubItems && jObject["Skills"] != null)
            {
                // Special handling for Skills array
                csv.AppendLine("      Skills 1," + string.Join(",", jObject["Skills"]));
            }
            csv.AppendLine();  // Add a blank line to separate different entries
        }
    }

    static void ProcessJArray(JArray jArray, StringBuilder csv, List<string> parentKeys, string arrayName)
    {
        int index = 0;
        foreach (JToken item in jArray)
        {
            if (item.Type == JTokenType.Object)
            {
                JObject obj = (JObject)item;
                ProcessJObject(obj, csv, new List<string>(parentKeys) { $"{index++}" });
            }
        }
    }
}
