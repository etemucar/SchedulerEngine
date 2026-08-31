# 1. Derleme (Build) Aşaması - .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FinYo.Api/FinYo.Api.csproj", "FinYo.Api/"]
COPY ["FinYo.Service/FinYo.Service.csproj", "FinYo.Service/"]
COPY ["FinYo.Core/FinYo.Core.csproj", "FinYo.Core/"]
COPY ["FinYo.Infrastructure/FinYo.Infrastructure.csproj", "FinYo.Infrastructure/"]
COPY ["RuleEngine/RuleEngine.csproj", "RuleEngine/"]

RUN dotnet restore "FinYo.Api/FinYo.Api.csproj"

COPY . .
WORKDIR "/src/FinYo.Api"
RUN dotnet build "FinYo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinYo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Çalışma Zamanı (Runtime) Aşaması - .NET 10.0 ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FinYo.Api.dll"]