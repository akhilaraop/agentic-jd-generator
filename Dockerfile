# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["AgenticJDGenerator.csproj", "./"]
RUN dotnet restore

# Copy the rest of the project files
COPY . .

# Build the project
RUN dotnet build -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8.0 ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set working directory
WORKDIR /app

# Copy the published files from the publish stage
COPY --from=publish /app/publish .

# Create a directory for the database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port 5000
EXPOSE 5000

# Set the entry point
ENTRYPOINT ["dotnet", "AgenticJDGenerator.dll"] 