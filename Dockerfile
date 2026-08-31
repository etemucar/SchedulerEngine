# 1. Derleme (Build) Aşaması - .NET 10.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sadece proje dosyalarını kopyala
COPY ["SchedulerEngine.Api/SchedulerEngine.Api.csproj", "SchedulerEngine.Api/"]
COPY ["SchedulerEngine.Service/SchedulerEngine.Service.csproj", "SchedulerEngine.Service/"]
COPY ["SchedulerEngine.Core/SchedulerEngine.Core.csproj", "SchedulerEngine.Core/"]
COPY ["SchedulerEngine.Infrastructure/SchedulerEngine.Infrastructure.csproj", "SchedulerEngine.Infrastructure/"]

# Eğer Scheduler/ klasöründe ayrı bir .csproj varsa burayı aktif edin:
# COPY ["Scheduler/Scheduler.csproj", "Scheduler/"]

# Bağımlılıkları yükle
RUN dotnet restore "SchedulerEngine.Api/SchedulerEngine.Api.csproj"

# Tüm kaynak kodları kopyala ve yayınla (publish)
COPY . .
WORKDIR "/src/SchedulerEngine.Api"
RUN dotnet publish "SchedulerEngine.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Çalışma Zamanı (Runtime) Aşaması - .NET 10.0 ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SchedulerEngine.Api.dll"]