# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY JiniTechApiSolution.sln ./
COPY JiniTechApi/JiniTechApi.csproj JiniTechApi/

# Restore dependencies
RUN dotnet restore JiniTechApi/JiniTechApi.csproj

# Copy the rest of the source code
COPY . .

# Build and publish the app
RUN dotnet publish JiniTechApi/JiniTechApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JiniTechApi.dll"]
