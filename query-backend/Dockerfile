# Backend Dockerfile for Job Description API
FROM python:3.9-slim

WORKDIR /app

# Copy backend code
COPY rag_app /app/rag_app
COPY requirements.txt /app/
COPY .env /app/
COPY job_descriptions /app/job_descriptions

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Expose backend port
EXPOSE 9061

# Start FastAPI server
CMD ["uvicorn", "rag_app.api_server:app", "--host", "0.0.0.0", "--port", "9061"]
