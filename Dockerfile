# Use the .NET SDK for building the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY JiniTechApiSolution.sln ./
COPY JiniTechApi/JiniTechApi.csproj JiniTechApi/

# Restore dependencies
RUN dotnet restore JiniTechApi/JiniTechApi.csproj

# Copy everything and build the release
COPY . .
RUN dotnet publish JiniTechApi/JiniTechApi.csproj -c Release -o /app/out

# Use ASP.NET runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "JiniTechApi.dll"]
