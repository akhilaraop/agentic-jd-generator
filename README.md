
# AI Job Description Manager

An AI-powered system to generate, refine, store, and semantically search job descriptions using a multi-agent architecture with vector-based retrieval.

---

## System Architecture

```mermaid
---
config:
  look: neo
---
flowchart TD
    subgraph MainApp["Main Application"]
        A1["Generate Job Description"]
        A2["Store Job Description"]
        A3["Read Job Description"]
        A4["Query Job Description"]
    end

    subgraph API["API Layer"]
        B1["Generate JD API"]
        B2["Store JD API"]
        B3["Read JD API"]
    end

    subgraph Agents["JD Agents"]
        C1["JD Agent Orchestrator"]
        C2["Clarifier Agent"]
        C3["Generator Agent"]
        C4["Critique Agent"]
        C5["Compliance Agent"]
        C6["Rewrite Agent"]
        C7["Finalizer Agent"]
    end

    subgraph DataStore["Data Storage"]
        D1["SQLite Database"]
        E1["Document Processor"]
        E2["Query Handler"]
    end

    subgraph EmbeddingsBackend["Embeddings Backend"]
        F1["FastAPI Backend"]
        F2["Document Processor"]
        F3["HuggingFace Embeddings"]
        F4["FAISS Vector Store"]
        F5["RAG Model"]
        F6["Groq Llama3-8B API"]
    end

    %% Main App Flow
    A1 -->|Generate| B1
    B1 -->|Orchestrate| C1
    C1 -->|Clarify| C2
    C1 -->|Generate| C3
    C1 -->|Critique| C4
    C1 -->|Compliance| C5
    C1 -->|Rewrite| C6
    C1 -->|Finalize| C7

    %% Storage Flow
    A2 -->|Save| B2
    B2 -->|Store| D1
    D1 -->|Process| E1
    E1 -->|Generate Embeddings| F1

    %% Reading Flow
    A3 -->|Read| B3
    B3 -->|Fetch| D1

    %% Query Flow
    A4 -->|Query| E2
    E2 -->|Search| F1

    %% Embeddings Backend Flow
    F1 -->|Load PDFs| F2
    F2 -->|Split & Embed| F3
    F3 -->|Store| F4
    F1 -->|Query| F5
    F5 -->|Retrieve| F4
    F5 -->|Call| F6
    F6 -->|Return Answer| F5
    F5 -->|Return Result| F1

    %% Styling
    style MainApp fill:#f9f,stroke:#333,stroke-width:1px
    style API fill:#bbf,stroke:#333,stroke-width:1px
    style Agents fill:#bfb,stroke:#333,stroke-width:1px
    style DataStore fill:#fdd,stroke:#333,stroke-width:1px
    style EmbeddingsBackend fill:#ffd,stroke:#333,stroke-width:1px 

```

## Components

### Job Descriptions Experts Portal
- **Generate Job Description:** Start new JD creation
- **Store Job Description:** Save JDs to storage
- **Read Job Description:** Retrieve stored JDs
- **Query Job Description:** Semantic search through JDs

### Agentic JD Generator APIs
- **Generate JD API:** Triggers JD agent orchestration
- **Store JD API:** Persists JDs to the database
- **Read JD API:** Retrieves stored JDs

### JD Agents
- **JD Agent Orchestrator:** Manages the workflow between agents
- **Clarifier Agent:** Clarifies ambiguous input
- **Generator Agent:** Creates initial draft
- **Critique Agent:** Reviews and suggests improvements
- **Compliance Agent:** Validates against compliance standards
- **Rewrite Agent:** Improves flow and style
- **Finalizer Agent:** Prepares JD for final storage

### Storage and Query
- **SQL Lite Database:** Structured storage for JDs
- **Document Processor:** Creates embeddings for JDs
- **VectorDB:** Stores embeddings for semantic query
- **Query Handler:** Processes user queries and retrieves matches

### External Systems
- **LLM Embeddings Generator:** Generates vector embeddings 

---

## Features

- Multi-agent job description generation pipeline
- Embeddings-based semantic search
- CQRS (Command Query Responsibility Separation) based architecture
- Compliance, Critique, and Rewrite workflow
- Easy API-based integrations
- Modular and extensible design

---

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- SQLite
- FAISS or Chroma for vector storage
- LLM services for embeddings (OpenAI / Huggingface)
- Docker (for local development)

---



