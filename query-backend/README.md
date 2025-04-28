# Job Description Backend (API)

This is the backend service for the Job Description Chat system. It provides REST APIs for document embedding and querying using FastAPI.

## Features
- Generate and store embeddings for job descriptions
- Query stored embeddings with natural language
- Designed for containerized, standalone deployment

## Requirements
- Python 3.9+
- See `requirements.txt` for dependencies

## Running Locally

```bash
pip install -r requirements.txt
uvicorn rag_app.api_server:app --reload --host 0.0.0.0 --port 9061
```

## Running in Docker

Build the image:
```bash
docker build -t query-backend .
```
Run the container:
```bash
docker run -p 9061:9061 --env-file .env  query-backend
```

## Environment Variables
- `GROQ_API_KEY` (required)
- `BACKEND_URL` (optional, default: http://localhost:9061)

## API Endpoints
- `POST /api/generate_embeddings`  
  Body: `{ "job_id": "string", "description": "string" }`
- `POST /api/query`  
  Body: `{ "job_id": "string", "query": "string" }`

---
