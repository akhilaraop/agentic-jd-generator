"""Document processing module for the RAG system."""
import os
import logging
from typing import Any

from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.document_loaders import PyPDFDirectoryLoader
from langchain_community.vectorstores import FAISS
from langchain_huggingface import HuggingFaceEmbeddings

from rag_app.config.loader import load_config, setup_logging

# Load configuration and setup logging
config = load_config()
setup_logging(config)
logger = logging.getLogger(__name__)

class DocumentProcessor:
    """Handles document loading, splitting, and embedding."""

    def __init__(self, path: str = config['document']['default_path']) -> None:
        """Initialize the DocumentProcessor with the path to the directory containing PDF documents.

        Args:
            path: Path to the directory containing PDF documents. Defaults to "./job_descriptions".
        """
        logger.info("Initializing DocumentProcessor with path:"+ path)
        self.path = path
        self.loader = PyPDFDirectoryLoader(self.path)
        self.embeddings = HuggingFaceEmbeddings(model_name="all-MiniLM-L6-v2")

        # Initialize embeddings with proper device handling
        try:
            logger.info("Initializing embeddings model")
            self.embeddings = HuggingFaceEmbeddings(
                model_name=config['document']['embedding_model']
            )
        except Exception as e:
            logger.error(f"Error initializing embeddings model: {str(e)}")
            raise

    def load_and_embed(self) -> Any:
        """Load documents from the specified directory and create embeddings.

        Returns:
            FAISS vector store containing the document embeddings.
        """
        try:
            # Load documents
            logger.info("Loading documents...")
            docs = self.loader.load()
            
            # Split documents
            logger.info("Splitting documents into chunks...")
            splitter = RecursiveCharacterTextSplitter(
                chunk_size=config['document']['chunk_size'],
                chunk_overlap=config['document']['chunk_overlap']
            )
            chunks = splitter.split_documents(docs)
            
            # Create vector store
            logger.info("Creating vector store...")
            vectorstore = FAISS.from_documents(chunks, self.embeddings)
            logger.info("Vector store created successfully")
            
            return vectorstore
        except Exception as e:
            logger.error(f"Error in load_and_embed: {str(e)}")
            raise
