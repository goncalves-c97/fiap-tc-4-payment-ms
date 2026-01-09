using Core.Adapters;
using System.Text.Json;

namespace Test.Adapters;

public class ObjectToJsonStringAdapterTests
{
    [Fact]
    public void ConvertToJsonString_WhenJsonElement_ReturnsElementToString()
    {
        using var doc = JsonDocument.Parse("{\"a\":1}");
        JsonElement element = doc.RootElement;

        var json = ObjectToJsonStringAdapter.ConvertToJsonString(element);

        Assert.Equal("{\"a\":1}", json);
    }

    [Fact]
    public void ConvertToJsonString_WhenPlainObject_ReturnsSerializedJson()
    {
        var json = ObjectToJsonStringAdapter.ConvertToJsonString(new { A = 1 });

        Assert.Contains("\"A\":1", json);
    }
}
