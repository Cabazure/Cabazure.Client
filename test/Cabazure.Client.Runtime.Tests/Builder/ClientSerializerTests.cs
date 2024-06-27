using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.Options;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class ClientSerializerTests
{
    public record TestObject(string Name, int Age);

    [Theory, AutoNSubstituteData]
    public void Serialize_Gets_Named_SerializerOptions(
        [Frozen] IOptionsMonitor<JsonSerializerOptions> optionsMonitor,
        ClientSerializer sut,
        string clientName,
        TestObject value)
    {
        sut.Serialize(clientName, value);
        optionsMonitor.Received(1).Get(clientName);
    }

    [Theory, AutoNSubstituteData]
    public void Serialize_Returns_Serialized_Value(
        [Frozen] IOptionsMonitor<JsonSerializerOptions> optionsMonitor,
        ClientSerializer sut,
        string clientName,
        TestObject value)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        optionsMonitor
            .Get(default)
            .ReturnsForAnyArgs(options);

        var result = sut.Serialize(clientName, value);

        result
            .Should()
            .Be(JsonSerializer.Serialize(value, options));
    }

    [Theory, AutoNSubstituteData]
    public void Deserialize_Gets_Named_SerializerOptions(
        [Frozen] IOptionsMonitor<JsonSerializerOptions> optionsMonitor,
        ClientSerializer sut,
        string clientName,
        TestObject value)
    {
        var json = JsonSerializer.Serialize(value);

        sut.Deserialize<TestObject>(clientName, json);

        optionsMonitor.Received(1).Get(clientName);
    }

    [Theory, AutoNSubstituteData]
    public void Deserialize_Returns_Deserialized_Value(
        [Frozen] IOptionsMonitor<JsonSerializerOptions> optionsMonitor,
        ClientSerializer sut,
        string clientName,
        TestObject value)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(value, options);
        optionsMonitor
            .Get(default)
            .ReturnsForAnyArgs(options);

        var result = sut.Deserialize<TestObject>(clientName, json);

        result
            .Should()
            .BeEquivalentTo(value);
    }
}
