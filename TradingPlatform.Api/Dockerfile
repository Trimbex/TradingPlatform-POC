FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TradingPlatform.Api/TradingPlatform.Api.csproj", "TradingPlatform.Api/"]
COPY ["TradingPlatform.Application/TradingPlatform.Application.csproj", "TradingPlatform.Application/"]
COPY ["TradingPlatform.Domain/TradingPlatform.Domain.csproj", "TradingPlatform.Domain/"]
COPY ["TradingPlatform.Infrastructure/TradingPlatform.Infrastructure.csproj", "TradingPlatform.Infrastructure/"]

RUN dotnet restore "TradingPlatform.Api/TradingPlatform.Api.csproj"

COPY . .
WORKDIR "/src/TradingPlatform.Api"
RUN dotnet build "TradingPlatform.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TradingPlatform.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TradingPlatform.Api.dll"]
