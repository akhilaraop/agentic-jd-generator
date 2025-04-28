#!/bin/bash
# Start both the .NET app and the query backend

dotnet AgenticJDGenerator.dll &
cd /embeddings-query-backend && uvicorn rag_app.api_server:app --host 0.0.0.0 --port 9001 &

wait
