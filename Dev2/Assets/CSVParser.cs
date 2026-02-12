using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class CSVParser
{
    public static List<Dictionary<string, string>> Parse(string csvData)
    {
        var result = new List<Dictionary<string, string>>();
        
        string[] lines = csvData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length < 2)
            return result;
        
        string[] headers = ParseCSVLine(lines[0]);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = ParseCSVLine(lines[i]);
            
            if (values.Length == headers.Length)
            {
                var row = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length; j++)
                {
                    row[headers[j].Trim()] = values[j].Trim();
                }
                result.Add(row);
            }
        }
        
        return result;
    }
    
    private static string[] ParseCSVLine(string line)
    {
        var pattern = new Regex("(?:,|^)(\"(?:[^\"]+|\"\")*\"|[^,]*)");
        var matches = pattern.Matches(line);
        var result = new string[matches.Count];
        
        for (int i = 0; i < matches.Count; i++)
        {
            string value = matches[i].Value;
            if (value.StartsWith(","))
                value = value.Substring(1);
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
            result[i] = value;
        }
        
        return result;
    }
}