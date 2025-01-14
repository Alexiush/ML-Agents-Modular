using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MappingGenerator
{
    private static HashSet<string> _mappings = new();

    public static void GenerateMappingFile(string path, IEnumerable<(string, string)> entries)
    {
        if (_mappings.Contains(path))
        {
            return;
        }

        var mapping = new StringBuilder();
        mapping.AppendLine("literals_mapping = {");

        entries
            .ToList()
            .ForEach(e => mapping.AppendLine($"\t'{e.Item1}':{e.Item2},"));

        mapping.AppendLine("}");

        var script = mapping.ToString();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream dataStream = new FileStream(path, FileMode.Create))
            {
                dataStream.Write(Encoding.UTF8.GetBytes(script));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        _mappings.Add(path);
    }
}
