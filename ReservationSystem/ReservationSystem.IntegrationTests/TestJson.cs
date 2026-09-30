using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReservationSystem.IntegrationTests;

public static class TestJson
{
    // Matches the API settings: enums are serialized as strings.
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
