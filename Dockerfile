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

# -------- Stage 2: Query Backend --------
FROM python:3.9-slim AS query-backend
WORKDIR /query-backend
COPY query-backend/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt
COPY query-backend/rag_app ./rag_app
COPY query-backend/.env ./
EXPOSE 9001
CMD ["uvicorn", "rag_app.api_server:app", "--host", "0.0.0.0", "--port", "9001"]

# -------- Stage 3: Final Multi-Service --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out .
COPY --from=query-backend /query-backend /query-backend
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5001
EXPOSE 9001
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]