using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace TradingPlatform.IntegrationTests;

public class PortfolioApiTests : IClassFixture<TradingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PortfolioApiTests(TradingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPortfolio_AfterDeposit_ReturnsCorrectData()
    {
        var userId = $"user-{Guid.NewGuid():N}";
        var depositRequest = new { userId, amount = 1000m };

        await _client.PostAsJsonAsync("/api/portfolio/deposit", depositRequest);

        var response = await _client.GetAsync($"/api/portfolio?userId={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(userId);
        content.Should().Contain("1000");
    }

    [Fact]
    public async Task GetPortfolio_NonExistentUser_Returns404()
    {
        var response = await _client.GetAsync("/api/portfolio?userId=nonexistent-user-12345");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
