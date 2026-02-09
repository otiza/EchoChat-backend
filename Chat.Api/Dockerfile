# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first (better layer caching)
COPY EchoChatService.sln ./
COPY Chat.Api/Chat.Api.csproj Chat.Api/
COPY Chat.Application/Chat.Application.csproj Chat.Application/
COPY Chat.Domain/Chat.Domain.csproj Chat.Domain/
COPY Chat.Infrastructure/Chat.Infrastructure.csproj Chat.Infrastructure/

RUN dotnet restore Chat.Api/Chat.Api.csproj

# Copy the rest and publish
COPY . ./
RUN dotnet publish Chat.Api/Chat.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

# Listen on HTTP inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Chat.Api.dll"]