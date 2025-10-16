# Use .NET 9 SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY JiniTechApiSolution.sln ./
COPY JiniTechApi/JiniTechApi.csproj ./JiniTechApi/

# Restore dependencies
RUN dotnet restore JiniTechApi/JiniTechApi.csproj

# Copy everything else
COPY . .

# Publish the app
RUN dotnet publish JiniTechApi/JiniTechApi.csproj -c Release -o /app/publish

# Use ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "JiniTechApi.dll"]
