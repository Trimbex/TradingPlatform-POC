using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TradingPlatform.Api.Controllers;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Domain.Enums;
using Xunit;

namespace TradingPlatform.IntegrationTests;

/// <summary>
/// Integration tests against real PostgreSQL and Kafka (Testcontainers). Exercises full infrastructure.
/// </summary>
public class OrdersApiTests : IClassFixture<TradingWebApplicationFactoryWithRealInfra>
{
    private readonly HttpClient _client;

    public OrdersApiTests(TradingWebApplicationFactoryWithRealInfra factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ExecuteOrder_ValidOrder_Returns204AndUpdatesOrderAndPortfolio()
    {
        var userId = $"user-exec-{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/portfolio/deposit", new DepositRequest(userId, 10000m));
        var placeResponse = await _client.PostAsJsonAsync("/api/orders", new PlaceOrderRequest(userId, "AAPL", 10, 150.50m));
        placeResponse.EnsureSuccessStatusCode();
        var orderId = await placeResponse.Content.ReadFromJsonAsync<Guid>();

        var executeResponse = await _client.PostAsync($"/api/orders/{orderId}/execute", null);

        executeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var orderResponse = await _client.GetAsync($"/api/orders/{orderId}");
        orderResponse.EnsureSuccessStatusCode();
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Executed);

        var portfolioResponse = await _client.GetAsync($"/api/portfolio?userId=" + userId);
        portfolioResponse.EnsureSuccessStatusCode();
        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioDto>();
        portfolio.Should().NotBeNull();
        portfolio!.CashBalance.Should().Be(8495m); // 10000 - 10*150.50
        portfolio.Holdings.Should().ContainSingle(h => h.Symbol == "AAPL" && h.Quantity == 10);
    }

    [Fact]
    public async Task ExecuteOrder_NonExistentOrder_Returns404()
    {
        var orderId = Guid.NewGuid();
        var response = await _client.PostAsync($"/api/orders/{orderId}/execute", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExecuteOrder_NoPortfolio_Returns404()
    {
        var userId = $"user-noport-{Guid.NewGuid():N}";
        var placeResponse = await _client.PostAsJsonAsync("/api/orders", new PlaceOrderRequest(userId, "AAPL", 10, 150.50m));
        placeResponse.EnsureSuccessStatusCode();
        var orderId = await placeResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.PostAsync($"/api/orders/{orderId}/execute", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExecuteOrder_InsufficientFunds_Returns500()
    {
        var userId = $"user-insuff-{Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/portfolio/deposit", new DepositRequest(userId, 100m));
        var placeResponse = await _client.PostAsJsonAsync("/api/orders", new PlaceOrderRequest(userId, "AAPL", 10, 150.50m));
        placeResponse.EnsureSuccessStatusCode();
        var orderId = await placeResponse.Content.ReadFromJsonAsync<Guid>();

        var response = await _client.PostAsync($"/api/orders/{orderId}/execute", null);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
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
