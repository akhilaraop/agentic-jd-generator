# Agentic Job Description Generator

An AI-powered job description generator that uses a multi-agent workflow to create professional, inclusive, and compliant job descriptions.

## Features

- Multi-agent workflow for job description generation
- Clarification of vague inputs
- Professional formatting and structure
- Compliance and bias checking
- Inclusive language review
- Swagger UI for API testing

## Prerequisites

- .NET 8.0 SDK
- Groq API key (get one from [Groq](https://console.groq.com))

## Local Setup

1. Clone the repository:
```bash
git clone https://github.com/yourusername/agentic-jd-generator.git
cd agentic-jd-generator
```

2. Install dependencies:
```bash
dotnet restore
```

3. Set up your Groq API key:
```bash
# Initialize user secrets
dotnet user-secrets init

# Add your Groq API key
dotnet user-secrets set "GroqApi:ApiKey" "your-groq-api-key"
```

4. Run the application:
```bash
dotnet run
```

5. Access the application:
- Swagger UI: http://localhost:5000/swagger
- API Root: http://localhost:5000

## Configuration

The application uses the following configuration:

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
2. Use the `/api/AgenticJobDescription/generate` endpoint
3. Send a POST request with the following body:
```json
{
  "initialInput": "Your job description requirements"
}
```

## Development

- The application uses a multi-agent workflow:
  1. Clarifier Agent: Asks questions for vague inputs
  2. Generator Agent: Creates initial job description
  3. Critique Agent: Reviews tone and formatting
  4. Compliance Agent: Checks for bias and compliance
  5. Rewriter Agent: Incorporates feedback

## Security Notes

- Never commit your API keys to source control
- Use user secrets for local development
- Set up proper environment variables in production

## License

MIT License - see LICENSE file for details