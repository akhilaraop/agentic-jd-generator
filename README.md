# Agentic Job Description Generator

An AI-powered job description generator that uses a multi-agent workflow to create professional, inclusive, and compliant job descriptions.

## Features

- Multi-agent workflow for job description generation
- CQRS pattern with MediatR for clean separation of commands and queries
- Clarification of vague inputs
- Professional formatting and structure
- Compliance and bias checking
- Inclusive language review
- Swagger UI for API testing (separate docs for Commands and Queries)

## Architecture Overview

- **CQRS Pattern:**
  - Write operations (commands) and read operations (queries) are separated for maintainability and scalability.
  - `Commands/` folder: Command classes and handlers for write operations (e.g., generating a job description).
  - `Queries/` folder: Query classes and handlers for read operations (e.g., fetching saved job descriptions).
- **MediatR:**
  - Controllers use dependency-injected `IMediator` to dispatch commands and queries.
  - Handlers implement the appropriate MediatR interfaces.
- **Multi-Agent Workflow:**
  - Clarifier, Generator, Critique, Compliance, and Rewriter agents collaborate to produce high-quality job descriptions.
- **Swagger UI:**
  - Separate documentation for Commands and Queries.
  - Command endpoints are grouped under "Commands API"; query endpoints under "Queries API".

## Sequence Diagram

```
User                    API Controller           JDOrchestrator           Agents
  |                           |                        |                      |
  |-- Generate JD Request -->|                        |                      |
  |                          |                        |                      |
  |                          |-- GenerateCommand -->|                        |
  |                          |                        |                      |
  |                          |                        |-- Clarifier -------->|
  |                          |                        |                      |
  |                          |                        |<-- Clarified Input --|
  |                          |                        |                      |
  |                          |                        |-- Generator -------->|
  |                          |                        |                      |
  |                          |                        |<-- Initial JD -------|
  |                          |                        |                      |
  |                          |                        |-- Critique --------->|
  |                          |                        |                      |
  |                          |                        |<-- Feedback ---------|
  |                          |                        |                      |
  |                          |                        |-- Compliance ------->|
  |                          |                        |                      |
  |                          |                        |<-- Compliance Check -|
  |                          |                        |                      |
  |                          |                        |-- Rewriter --------->|
  |                          |                        |                      |
  |                          |                        |<-- Final JD ---------|
  |                          |                        |                      |
  |                          |<-- JDResponse --------|                      |
  |                          |                        |                      |
  |<-- Job Description ------|                        |                      |
  |                          |                        |                      |
```

The sequence diagram illustrates the flow of job description generation:
1. User sends a request to generate a job description
2. API Controller receives the request and creates a GenerateCommand
3. JDOrchestrator coordinates the multi-agent workflow:
   - Clarifier Agent analyzes and clarifies the input
   - Generator Agent creates the initial job description
   - Critique Agent reviews tone and formatting
   - Compliance Agent checks for bias and compliance
   - Rewriter Agent incorporates feedback and produces the final version
4. The final job description is returned to the user

For a detailed system design document, see [System Design](docs/system_design.md).

## Persistence Layer

- Uses **Entity Framework Core** as the ORM for data access.
- **SQLite** is the default database provider (see `appsettings.json` for connection string).
- The main data model is `SavedJobDescription`, which stores:
  - The generated job description
  - The initial user input
  - The creation timestamp
  - All workflow stages (as a dictionary)
- The `ApplicationDbContext` class manages the database context and schema.
- Migrations can be managed using standard EF Core CLI commands.

## Prerequisites

- Docker installed on your machine
- Git installed on your machine
- Groq API key (get one from [Groq](https://console.groq.com))

## Setup with Docker

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/agentic-jd-generator.git
   cd agentic-jd-generator
   ```

2. **Build the Docker image:**
   ```bash
   docker build -t agentic-jd-generator .
   ```

3. **Run the container:**
   ```bash
   # Run with API key
   docker run -p 5000:5000 -e GROQ_API_KEY=your-api-key agentic-jd-generator
   ```

4. **Access the application:**
   - Swagger UI: http://localhost:5000/swagger
   - API Root: http://localhost:5000

### Environment Variables
- `GROQ_API_KEY`: Your Groq API key (required)
- `ASPNETCORE_ENVIRONMENT`: Set to "Development" by default
- `ASPNETCORE_URLS`: Set to "http://+:5000" by default

### Troubleshooting

If you encounter any issues:

1. **Check Docker logs:**
   ```bash
   docker logs <container_id>
   ```

2. **Verify the container is running:**
   ```bash
   docker ps
   ```

3. **Restart the container if needed:**
   ```bash
   docker stop <container_id>
   docker start <container_id>
   ```

## Configuration

- `appsettings.json`: Contains non-sensitive configuration
- User Secrets: Contains sensitive data (API keys)

### Available Configuration

```json
{
  "GroqApi": {
    "BaseUrl": "https://api.groq.com/openai/v1/chat/completions",
    "Model": "llama3-8b-8192"
  }
}
```

## API Usage

1. Open Swagger UI at http://localhost:5000/swagger
2. Use the `/api/jd` endpoint for job description generation (POST, Command)
3. Use the `/api/saved-descriptions` endpoints for retrieving saved job descriptions (GET, Query)
4. Send a POST request with the following body:
```json
{
  "initialInput": "Your job description requirements"
}
```

## Extending the System

- **To add a new command (write operation):**
  1. Create a new command and handler in the `Commands/` folder.
  2. Annotate the controller endpoint with `[ApiExplorerSettings(GroupName = "commands")]`.
  3. Use `IMediator.Send()` in the controller.
- **To add a new query (read operation):**
  1. Create a new query and handler in the `Queries/` folder.
  2. Annotate the controller endpoint with `[ApiExplorerSettings(GroupName = "queries")]`.
  3. Use `IMediator.Send()` in the controller.

