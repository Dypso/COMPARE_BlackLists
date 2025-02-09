FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["BlacklistApi/BlacklistApi.csproj", "BlacklistApi/"]
RUN dotnet restore "BlacklistApi/BlacklistApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/BlacklistApi"
RUN dotnet build "BlacklistApi.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "BlacklistApi.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlacklistApi.dll"]