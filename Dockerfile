# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY JiniTechApiSolution.sln ./
COPY JiniTechApi/JiniTechApi.csproj ./JiniTechApi/

# Restore dependencies
RUN dotnet restore JiniTechApi/JiniTechApi.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish JiniTechApi/JiniTechApi.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose port and run
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JiniTechApi.dll"]
