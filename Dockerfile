# -------- Stage 1: Build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY AgenticJDGenerator.sln .
COPY AgenticJDGenerator.csproj .

# Restore dependencies
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build and publish the release version
RUN dotnet publish -c Release -o /out

# -------- Stage 2: Run --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /out .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development

# Expose port 5001
EXPOSE 5001

# Run the application
ENTRYPOINT ["dotnet", "AgenticJDGenerator.dll"]
    