using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TradingPlatform.Api.Controllers;
using Xunit;

namespace TradingPlatform.IntegrationTests;

public class OrdersApiTests : IClassFixture<TradingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersApiTests(TradingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PlaceOrder_ValidRequest_Returns200WithOrderId()
    {
        var request = new PlaceOrderRequest("user-1", "AAPL", 10, 150.50m);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orderId = await response.Content.ReadFromJsonAsync<Guid>();
        orderId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetOrder_ExistingOrder_ReturnsOrder()
    {
        var placeRequest = new PlaceOrderRequest("user-2", "MSFT", 5, 400m);
        var placeResponse = await _client.PostAsJsonAsync("/api/orders", placeRequest);
        placeResponse.EnsureSuccessStatusCode();
        var orderId = await placeResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("user-2");
        content.Should().Contain("MSFT");
        content.Should().Contain("5");
        content.Should().Contain("400");
    }

    [Fact]
    public async Task PlaceOrder_InvalidRequest_Returns400WithValidationErrors()
    {
        var request = new PlaceOrderRequest("", "AAPL", -1, 0);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var root = json.RootElement;
        root.GetProperty("message").GetString().Should().Be("One or more validation errors occurred.");
        var errors = root.GetProperty("errors");
        errors.GetArrayLength().Should().BeGreaterThan(0);
    }
}
