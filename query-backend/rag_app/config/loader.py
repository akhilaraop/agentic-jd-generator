"""Configuration loader for the RAG system."""
import os
import yaml
import logging
from typing import Dict, Any

def load_config(config_file: str = "config.yml") -> Dict[str, Any]:
    """Load configuration from YAML file.
    
    Args:
        config_file: Name of the configuration file. Defaults to "config.yml".
        
    Returns:
        Dict containing the configuration.
    """
    config_path = os.path.join(os.path.dirname(__file__), config_file)
    
    try:
        with open(config_path, 'r') as f:
            config = yaml.safe_load(f)
        return config
    except Exception as e:
        raise RuntimeError(f"Error loading configuration from {config_path}: {str(e)}")

def setup_logging(config: Dict[str, Any]) -> None:
    """Setup logging based on configuration.
    
    Args:
        config: Configuration dictionary containing logging settings.
    """
    handlers = []
    
    for handler_config in config['logging']['handlers']:
        if handler_config['type'] == 'file':
            handlers.append(logging.FileHandler(handler_config['filename']))
        elif handler_config['type'] == 'console':
            handlers.append(logging.StreamHandler())
    
    logging.basicConfig(
        level=getattr(logging, config['logging']['level']),
        format=config['logging']['format'],
        handlers=handlers
    ) 
