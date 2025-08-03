using Postway.Libraries;

namespace Postway.IntegrationTest.Postway.Libraries;

public class HttpClientServiceTest
{
    private class TestModel
    {
        public string? Message { get; set; }
    }

    [Fact]
    public void SetHeader_ShouldAddHeadersToHttpClient()
    {
        var httpClient = new HttpClient();
        var service = new HttpClientService();
        var headers = new Dictionary<string, string> { { "X-Test", "Value" } };
        service.SetHeader(httpClient, headers);
        Assert.True(httpClient.DefaultRequestHeaders.Contains("X-Test"));
    }

    //[Fact]
    //public async Task GetAsync_ShouldReturnDeserializedObject()
    //{
    //    var handler = new Mock<HttpMessageHandler>();
    //    var responseObj = new TestModel { Message = "Hello" };
    //    var responseJson = JsonSerializer.Serialize(responseObj);
    //    handler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new HttpResponseMessage
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
    //        });
    //    var httpClient = new HttpClient(handler.Object);
    //    var service = new HttpClientService();
    //    // Use reflection to inject httpClient for test
    //    var result = await service.GetAsync<TestModel>("http://test");
    //    Assert.NotNull(result);
    //}

    //[Fact]
    //public async Task PostAsync_ShouldReturnDeserializedObject()
    //{
    //    var handler = new Mock<HttpMessageHandler>();
    //    var responseObj = new TestModel { Message = "Posted" };
    //    var responseJson = JsonSerializer.Serialize(responseObj);
    //    handler.Setup(h => h.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Post), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new HttpResponseMessage
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
    //        });
    //    var httpClient = new HttpClient(handler.Object);
    //    var service = new HttpClientService();
    //    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    //    var result = await service.PostAsync<TestModel>("http://test", content);
    //    Assert.NotNull(result);
    //}

    //[Fact]
    //public async Task PutAsync_ShouldReturnDeserializedObject()
    //{
    //    var handler = new Mock<HttpMessageHandler>();
    //    var responseObj = new TestModel { Message = "Put" };
    //    var responseJson = JsonSerializer.Serialize(responseObj);
    //    handler.Setup(h => h.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Put), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new HttpResponseMessage
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
    //        });
    //    var httpClient = new HttpClient(handler.Object);
    //    var service = new HttpClientService();
    //    var content = new StringContent("{}", Encoding.UTF8, "application/json");
    //    var result = await service.PutAsync<TestModel>("http://test", content);
    //    Assert.NotNull(result);
    //}

    //[Fact]
    //public async Task DeleteAsync_ShouldReturnDeserializedObject()
    //{
    //    var handler = new Mock<HttpMessageHandler>();
    //    var responseObj = new TestModel { Message = "Deleted" };
    //    var responseJson = JsonSerializer.Serialize(responseObj);
    //    handler.Setup(h => h.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Delete), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new HttpResponseMessage
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
    //        });
    //    var httpClient = new HttpClient(handler.Object);
    //    var service = new HttpClientService();
    //    var result = await service.DeleteAsync<TestModel>("http://test");
    //    Assert.NotNull(result);
    //}
}
