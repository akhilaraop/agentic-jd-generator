"""
FastAPI backend for the RAG system.
Exposes endpoints for embedding generation and querying, with automatic Swagger docs.
"""
from fastapi import FastAPI, HTTPException
from fastapi.openapi.utils import get_openapi
from fastapi.responses import RedirectResponse
from pydantic import BaseModel
from typing import Optional
import logging
import os

from rag_app.document_processor import DocumentProcessor
from rag_app.rag_model import RAGModel
from rag_app.config.loader import load_config, setup_logging

config = load_config()
setup_logging(config)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="Job Description RAG API",
    description="API for document embedding and querying using Retrieval-Augmented Generation (RAG).\n\nSee /docs for the interactive Swagger UI.",
    version="1.0.0",
    contact={
        "name": "Your Team",
        "email": "your-team@example.com"
    },
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_tags=[
        {
            "name": "Embeddings",
            "description": "Endpoints for generating and storing embeddings."
        },
        {
            "name": "Query",
            "description": "Endpoints for querying job description embeddings."
        }
    ]
)

# In-memory stores (can be replaced with persistent storage)
vectorstores = {}
processor = DocumentProcessor()
vectorstore = processor.load_and_embed()
vectorstores[0] = vectorstore
groq_api_key = os.getenv("GROQ_API_KEY")
if not groq_api_key:
    logger.error("GROQ_API_KEY not set in environment")
    raise HTTPException(status_code=500, detail="GROQ_API_KEY not set in environment")
rag_model = RAGModel(groq_api_key)

class EmbeddingRequest(BaseModel):
    job_id: str
    description: str

class QueryRequest(BaseModel):
    job_id: str
    query: str

@app.get("/", include_in_schema=False)
def root_redirect():
    """Redirect to the Swagger docs UI."""
    return RedirectResponse(url="/docs")

@app.post("/api/generate_embeddings", summary="Generate and store embeddings", tags=["Embeddings"])
def generate_embeddings(req: EmbeddingRequest):
    """
    Generate and store embeddings for a job description.
    - **job_id**: Unique identifier for the job
    - **description**: The job description text
    """
    try:
        logger.info(f"Generating embeddings for job_id={req.job_id}")
        from langchain.docstore.document import Document
        docs = [Document(page_content=req.description)]
        splitter = getattr(processor, 'splitter', None)
        if not splitter:
            from langchain.text_splitter import RecursiveCharacterTextSplitter
            splitter = RecursiveCharacterTextSplitter(
                chunk_size=config['document']['chunk_size'],
                chunk_overlap=config['document']['chunk_overlap']
            )
        chunks = splitter.split_documents(docs)
        from langchain_community.vectorstores import FAISS
        vectorstore = FAISS.from_documents(chunks, processor.embeddings)
        vectorstores[req.job_id] = vectorstore
        logger.info(f"Embeddings stored for job_id={req.job_id}")
        return {"status": "success", "job_id": req.job_id}
    except Exception as e:
        logger.error(f"Error generating embeddings: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/query/id", summary="Query job ID", tags=["QueryID"])
def query_job(req: QueryRequest):
    """
    Query job description embeddings and get a response.
    - **job_id**: Unique identifier for the job
    - **query**: Natural language query string
    """
    try:
        # If job_id is 0, query all documents (vectorstores[0])
        if str(req.job_id) == "0":
            logger.info(f"Query (all docs) = {req.query}")
            vectorstore = vectorstores[0]
            retriever = vectorstore.as_retriever()
            response = rag_model.get_response(retriever, req.query)
            return {"answer": response.get("answer", ""), "context": response.get("context", [])}
        # Else, keep existing logic
        logger.info(f"Querying for job_id={req.job_id}")
        if req.job_id not in vectorstores:
            raise HTTPException(status_code=404, detail="Embeddings not found for this job_id.")
        vectorstore = vectorstores[req.job_id]
        retriever = vectorstore.as_retriever()
        response = rag_model.get_response(retriever, req.query)
        return {"answer": response.get("answer", ""), "context": response.get("context", [])}
    except Exception as e:
        logger.error(f"Error querying embeddings: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/query", summary="Query All Jobs", tags=["QueryAll"])
def query_all(req: str):
    """
    Query job description embeddings and get a response.
    - **job_id**: Unique identifier for the job
    - **query**: Natural language query string
    """
    try:
        logger.info(f"Query={req}")
        vectorstore = vectorstores[0]
        retriever = vectorstore.as_retriever()
        response = rag_model.get_response(retriever, req)
        return {"answer": response.get("answer", ""), "context": response.get("context", [])}
    except Exception as e:
        logger.error(f"Error querying embeddings: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
