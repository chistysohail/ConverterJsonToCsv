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

            if (jsonToken.Type == JTokenType.Object)
            {
                ProcessJObject((JObject)jsonToken, csv, new List<string>(), 1);
            }
            else if (jsonToken.Type == JTokenType.Array)
            {
                ProcessJArray((JArray)jsonToken, csv, new List<string>(), "Root", 1);
            }

            File.WriteAllText(csvFilePath, csv.ToString());
            Console.WriteLine($"Combined CSV file has been saved: {csvFilePath}");
        }
    }

    static void ProcessJObject(JObject jObject, StringBuilder csv, List<string> parentKeys, int serialNumber)
    {
        foreach (var property in jObject.Properties())
        {
            if (property.Value is JObject subObject)
            {
                // Pass serialNumber without incrementing for nested objects
                ProcessJObject(subObject, csv, new List<string>(parentKeys) { property.Name }, serialNumber);
            }
            else if (property.Value is JArray array)
            {
                // Increment serialNumber for each element in the array if it's a list of objects
                ProcessJArray(array, csv, new List<string>(parentKeys) { property.Name }, property.Name, serialNumber);
            }
            else
            {
                var headers = new List<string> { "Serial Number" };
                var values = new List<string> { serialNumber.ToString() };
                headers.Add(string.Join("_", parentKeys) + "_" + property.Name);
                values.Add(property.Value.ToString());

                // Print headers only once
                if (csv.Length == 0)
                {
                    csv.AppendLine(string.Join(",", headers));
                }
                csv.AppendLine(string.Join(",", values));
            }
        }
    }

    static void ProcessJArray(JArray jArray, StringBuilder csv, List<string> parentKeys, string arrayName, int serialNumber)
    {
        int index = 1; // Start indexing from 1 for serial numbers
        foreach (JToken item in jArray)
        {
            if (item.Type == JTokenType.Object)
            {
                JObject obj = (JObject)item;
                ProcessJObject(obj, csv, new List<string>(parentKeys) { arrayName }, serialNumber++);
            }
        }
    }
}
