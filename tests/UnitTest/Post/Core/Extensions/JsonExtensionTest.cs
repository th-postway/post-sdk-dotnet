using Postway.Core.Extensions;

namespace Postway.UnitTest.Post.Core.Extensions;

public class JsonExtensionTest
{
    private class TestModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void ToJson_ShouldSerializeObject()
    {
        var model = new TestModel { Name = "John", Age = 30 };
        var json = model.ToJson();
        Assert.Contains("John", json);
        Assert.Contains("30", json);
    }

    [Fact]
    public void FromJson_ShouldDeserializeToObject()
    {
        var json = "{\"Name\":\"Jane\",\"Age\":25}";
        var model = json.FromJson<TestModel>();
        Assert.NotNull(model);
        Assert.Equal("Jane", model!.Name);
        Assert.Equal(25, model.Age);
    }

    [Fact]
    public void FromJson_ShouldReturnNullForInvalidJson()
    {
        var invalidJson = "not a json string";
        var model = invalidJson.FromJson<TestModel>();
        Assert.Null(model);
    }
}
