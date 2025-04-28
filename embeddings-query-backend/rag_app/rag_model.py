"""RAG model implementation for the chat system.

This module implements the Retrieval-Augmented Generation (RAG) model using
the Llama3-8B model through Groq's API. It handles document retrieval and
response generation based on the retrieved context.
"""
import logging
from typing import Any, Dict, List, Optional

from langchain.chains import create_retrieval_chain
from langchain.chains.combine_documents import create_stuff_documents_chain
from langchain_core.prompts import ChatPromptTemplate
from langchain_groq import ChatGroq
from rag_app.config.loader import load_config, setup_logging

# Load configuration and setup logging
config = load_config()
setup_logging(config)
logger = logging.getLogger(__name__)


class RAGModel:
    """Handles the interaction with the Llama3-8B model through Groq's API.
    
    This class manages the RAG pipeline, including:
    - Initialization of the language model
    - Document retrieval and context management
    - Response generation based on retrieved context
    
    Attributes:
        llm: The ChatGroq language model instance
        prompt: The template for generating responses
    """

    def __init__(self, groq_api_key: str) -> None:
        """Initialize the RAGModel with the Groq API key.
        
        Args:
            groq_api_key: API key for the Groq service.
            
        Raises:
            ValueError: If the API key is invalid or the model fails to initialize.
        """
        logger.info("Initializing RAGModel")
        try:
            # Initialize model with configuration
            model_config = config['rag_model']
            self.llm = ChatGroq(
                groq_api_key=groq_api_key,
                model_name=model_config['model_name'],
                temperature=model_config['temperature'],
                max_tokens=model_config['max_tokens']
            )
            logger.info(f"ChatGroq model initialized with {model_config['model_name']}")
            
            # Create prompt template from configuration
            self.prompt = ChatPromptTemplate.from_template(model_config['prompt_template'])
            logger.info("Prompt template created successfully")
        except Exception as e:
            logger.error(f"Error initializing RAGModel: {str(e)}", exc_info=True)
            raise

    def get_response(self, retriever: Any, user_input: str) -> Dict[str, Any]:
        """Get the response from the model using the retriever and user input.
        
        This method:
        1. Creates a retrieval chain using the provided retriever
        2. Combines the retrieved documents with the user's question
        3. Generates a response using the language model
        
        Args:
            retriever: The document retriever to use for context retrieval.
            user_input: The user's question or input text.
            
        Returns:
            Dict[str, Any]: A dictionary containing:
                - "answer": The generated response text
                - "context": List of retrieved document chunks used for the response
                
        Raises:
            Exception: If the model fails to generate a response or if the retriever fails.
        """
        logger.info(f"Getting response for input: {user_input}")
        try:
            logger.info("Creating retrieval chain")
            chain = create_retrieval_chain(
                retriever, create_stuff_documents_chain(self.llm, self.prompt)
            )
            
            logger.info("Invoking chain with user input")
            response = chain.invoke({"input": user_input})
            
            if not response or "answer" not in response:
                logger.error("Invalid response format from model")
                raise ValueError("Invalid response format from model")
                
            logger.info("Response generated successfully")
            return response
            
        except Exception as e:
            logger.error(f"Error generating response: {str(e)}", exc_info=True)
            raise
