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
                ProcessJArray(array, csv, new List<string>(parentKeys) { property.Name }, property.Name);
            }
            else
            {
                headers.Add(string.Join("_", parentKeys) + "_" + property.Name);
                values.Add(property.Value.ToString());
            }
        }

        if (!hasSubObject && headers.Count > 0)
        {
            csv.AppendLine(string.Join(",", headers));  // Print headers
            csv.AppendLine(string.Join(",", values));  // Print values
            csv.AppendLine();  // Add a blank line to separate different "tables"
        }
    }

    static void ProcessJArray(JArray jArray, StringBuilder csv, List<string> parentKeys, string arrayName)
    {
        int index = 0;
        var headers = new List<string> { "Serial Number" };
        headers.AddRange(GetHeaders(jArray.First));
        csv.AppendLine(string.Join(",", headers));

        foreach (JToken item in jArray)
        {
            var values = new List<string> { (index + 1).ToString() }; // Add serial number starting at 1
            if (item.Type == JTokenType.Object)
            {
                JObject obj = (JObject)item;
                foreach (var prop in obj.Properties())
                {
                    if (prop.Value.Type != JTokenType.Array && prop.Value.Type != JTokenType.Object)
                    {
                        values.Add(prop.Value.ToString());
                    }
                }
                csv.AppendLine(string.Join(",", values));
            }
            index++;
        }
        csv.AppendLine();  // Add a blank line to separate different "tables"
    }

    static List<string> GetHeaders(JToken firstItem)
    {
        var headers = new List<string>();
        if (firstItem.Type == JTokenType.Object)
        {
            JObject obj = (JObject)firstItem;
            foreach (var prop in obj.Properties())
            {
                if (prop.Value.Type != JTokenType.Array && prop.Value.Type != JTokenType.Object)
                {
                    headers.Add(prop.Name);
                }
            }
        }
        return headers;
    }
}
