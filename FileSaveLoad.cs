using System.Text.Json;

public class FileSaveHandler
{

    //public void SaveDictionary(string path, Dictionary<string, string> dict)
    //{
    //    string json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
    //    File.WriteAllText(path, json);
    //}

    //public Dictionary<string, string> LoadDictionary(string path)
    //{
    //    if (!File.Exists(path))
    //        return new Dictionary<string, string>();

    //    string json = File.ReadAllText(path);
    //    return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
    //        ?? throw new ArgumentNullException();
    //}

    private const char Separator = '\u241E'; // ␞
    public void SaveDictionary(string path, Dictionary<string, string> dict)
    {
        using var writer = new StreamWriter(path);
        foreach (var kvp in dict)
        {
            // Escape the separator if it appears in key/value
            string safeKey = kvp.Key.Replace(Separator.ToString(), $"\\{Separator}");
            string safeValue = kvp.Value.Replace(Separator.ToString(), $"\\{Separator}");
            writer.WriteLine($"{safeKey}{Separator}{safeValue}");
        }
    }
    public Dictionary<string, string> LoadDictionary(string path)
    {
        var dict = new Dictionary<string, string>();

        if (!File.Exists(path))
            return dict;

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            int idx = line.IndexOf(Separator);
            if (idx < 0) continue; // skip malformed lines

            string key = line.Substring(0, idx).Replace($"\\{Separator}", Separator.ToString());
            string value = line.Substring(idx + 1).Replace($"\\{Separator}", Separator.ToString());

            dict[key] = value;
        }

        return dict;
    }
}