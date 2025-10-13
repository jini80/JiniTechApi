# Use official .NET 9 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore

# Publish the app in Release mode to the /out directory
RUN dotnet publish -c Release -o out

# Use the smaller runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port Render expects
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run the API
ENTRYPOINT ["dotnet", "JiniTechApi.dll"]
