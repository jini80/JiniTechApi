# Use official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

# Copy solution and project files
COPY JiniTechApiSolution.sln ./
COPY JiniTechApi/JiniTechApi.csproj ./JiniTechApi/

# Restore dependencies
RUN dotnet restore JiniTechApi/JiniTechApi.csproj

# Copy the rest of the source
COPY . .

# Publish the app
RUN dotnet publish JiniTechApi/JiniTechApi.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Set the entry point
ENV ASPNETCORE_URLS=http://+:5024
EXPOSE 5024

ENTRYPOINT ["dotnet", "JiniTechApi.dll"]
