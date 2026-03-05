using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TradingPlatform.Application.DTOs;
using Xunit;

namespace TradingPlatform.IntegrationTests;

public class TransactionsApiTests : IClassFixture<TradingWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionsApiTests(TradingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactions_AfterDeposit_ReturnsDepositTransaction()
    {
        var userId = $"user-tx-{Guid.NewGuid():N}";
        var amount = 500m;
        var depositRequest = new { userId, amount };

        await _client.PostAsJsonAsync("/api/portfolio/deposit", depositRequest);

        var response = await _client.GetAsync($"/api/transactions?userId={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        transactions.Should().NotBeNull();
        transactions!.Should().NotBeEmpty();
        transactions.Should().Contain(t => t.UserId == userId && t.Amount == amount && t.Type == "Deposit");
    }

    [Fact]
    public async Task GetTransactions_NoTransactions_ReturnsEmptyList()
    {
        var userId = $"user-empty-{Guid.NewGuid():N}";

        var response = await _client.GetAsync($"/api/transactions?userId={userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        transactions.Should().NotBeNull();
        transactions!.Should().BeEmpty();
    }
}

