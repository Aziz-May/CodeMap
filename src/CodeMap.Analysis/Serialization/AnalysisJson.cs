using System.Text.Json; //for JsonSerializer, JsonSerializerOptions, JsonNamingPolicy
using System.Text.Json.Serialization; //for JsonStringEnumConverter
using CodeMap.Analysis.Model;

namespace CodeMap.Analysis.Serialization;

public static class AnalysisJson
{
    public static string Serialize(AnalysisResult result) => JsonSerializer.Serialize(result, Options);
    
        
    
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = {new JsonStringEnumConverter()},
    };
    
}