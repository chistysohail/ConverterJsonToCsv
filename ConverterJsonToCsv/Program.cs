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
        List<string> headers = new List<string> { "Serial Number" };
        List<string> values = new List<string> { serialNumber.ToString() };
        bool firstItem = true;
        foreach (var property in jObject.Properties())
        {
            if (property.Value is JObject subObject)
            {
                ProcessJObject(subObject, csv, new List<string>(parentKeys) { property.Name }, serialNumber);
            }
            else if (property.Value is JArray array && property.Name == "Skills")
            {
                // Handle Skills by joining them into a single column
                values.Add(string.Join(", ", array.ToObject<List<string>>()));
                if (firstItem) headers.Add("Skills");
            }
            else
            {
                if (firstItem) headers.Add(property.Name);
                values.Add(property.Value.ToString());
            }
            firstItem = false;
        }

        if (headers.Count > 1) // Ensure we have more than just the serial number
        {
            if (csv.Length == 0)
            {
                csv.AppendLine(string.Join(",", headers));
            }
            csv.AppendLine(string.Join(",", values));
        }
    }

    static void ProcessJArray(JArray jArray, StringBuilder csv, List<string> parentKeys, string arrayName, int serialNumber)
    {
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
