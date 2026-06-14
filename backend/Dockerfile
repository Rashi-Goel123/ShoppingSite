# Use the official .NET 10 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["EcommerceApi.csproj", "./"]
RUN dotnet restore "./EcommerceApi.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet build "EcommerceApi.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "EcommerceApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 10 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 8080 (Render's default expected port)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EcommerceApi.dll"]
