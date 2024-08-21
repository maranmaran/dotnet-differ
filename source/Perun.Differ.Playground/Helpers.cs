using System.Text.Json;
using Differ.DotNet;

public static class DifferenceExtensions
{
    public static void Output(this IEnumerable<Difference> diffs)
    {
        Console.WriteLine(JsonSerializer.Serialize(diffs, new JsonSerializerOptions()
        {
            WriteIndented = true,
        }));
    }
}