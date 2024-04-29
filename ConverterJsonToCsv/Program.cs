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
                ProcessJArray((JArray)jsonToken, csv, new List<string>());
            }

            File.WriteAllText(csvFilePath, csv.ToString());
            Console.WriteLine($"Combined CSV file has been saved: {csvFilePath}");
        }
    }

    static void ProcessJObject(JObject jObject, StringBuilder csv, List<string> parentKeys)
    {
        var headers = new List<string>();
        var values = new List<string>();
        bool hasSubObject = false;

        foreach (var property in jObject.Properties())
        {
            if (property.Value is JObject subObject)
            {
                hasSubObject = true;
                List<string> newParentKeys = new List<string>(parentKeys) { property.Name };
                ProcessJObject(subObject, csv, newParentKeys);
            }
            else if (property.Value is JArray array)
            {
                hasSubObject = true;
                ProcessJArray(array, csv, new List<string>(parentKeys) { property.Name });
            }
            else
            {
                headers.Add(string.Join("_", parentKeys) + "_" + property.Name);
                values.Add(property.Value.ToString());
            }
        }

        if (!hasSubObject && headers.Count > 0)
        {
            csv.AppendLine(string.Join(",", headers));
            csv.AppendLine(string.Join(",", values));
            csv.AppendLine();  // Add a blank line to separate different "tables"
        }
    }

    static void ProcessJArray(JArray jArray, StringBuilder csv, List<string> parentKeys)
    {
        int index = 0;
        foreach (JToken item in jArray)
        {
            if (item.Type == JTokenType.Object)
            {
                ProcessJObject((JObject)item, csv, new List<string>(parentKeys) { $"{index}" });
            }
            index++;
        }
    }
}
