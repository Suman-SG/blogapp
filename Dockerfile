# Use .NET 9 SDK for build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# Copy all files from the root repo
COPY . ./

# Go to the folder containing the .csproj
WORKDIR /src/blogapp

# Restore and publish the project
RUN dotnet restore blogapp.csproj
RUN dotnet publish -c Release -o /app/publish blogapp.csproj

# Use ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS final
WORKDIR /app

# Copy the published app from build stage
COPY --from=build /app/publish .

# Start the app
ENTRYPOINT ["dotnet", "blogapp.dll"]
