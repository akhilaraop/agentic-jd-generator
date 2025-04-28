
# Agentic Job Description Generator

An AI-powered system to generate, refine, store, and semantically search job descriptions using a multi-agent architecture with vector-based retrieval.

---

## System Architecture

```mermaid
---
config:
  look: neo
---
flowchart TD
 subgraph subGraph0["Job Descriptions Experts Portal"]
        A1["Generate Job Description"]
        A2["Store Job Description"]
        A3["Read Job Description"]
        A4["Query Job Description"]
  end
 subgraph subGraph1["Agentic Job Description Generator"]
        B1["Generate JD API"]
        B2["Store JD API"]
        B3["Read JD API"]
  end
 subgraph subGraph2["JD Agents"]
        C1["JD Agent Orchestrator"]
        C2["Clarifier Agent"]
        C3["Generator Agent"]
        C4["Critique Agent"]
        C5["Compliance Agent"]
        C6["Rewrite Agent"]
        C7["Finalizer Agent"]
  end
 subgraph subGraph3["Job Description Data Store"]
        D1["SQL Lite"]
  end
 subgraph subGraph4["JD Vector Store"]
        G1["VectorDB"]
  end
 subgraph subGraph5["Job Description Embeddings and Query"]
        E1["Document Processor"]
        E2["Query Handler"]
  end
 subgraph subGraph6["External System"]
        F1["LLM Embeddings Generator"]
  end
    A1 -- Generate Job Description --> B1
    B1 -- Generate Job Description --> C1
    C1 -- Clarify --> C2
    C1 -- Generate --> C3
    C1 -- Critique --> C4
    C1 -- Compliance --> C5
    C1 -- Rewrite --> C6
    C1 -- Final Polish --> C7
    A2 -- Save Job Description --> B2
    B2 -- Save Job Description --> D1
    D1 -- AsyncEvent::Process Job Description --> E1
    E1 -- Generate Embeddings --> F1
    E1 -- Store Embeddings --> G1
    A3 -- Read Job Description --> B3
    B3 -- Read Job Description --> D1
    A4 -- User Query --> E2
    E2 -- Generate Embeddings --> F1
    E2 -- GetResults --> G1
    D1@{ shape: cyl}
    G1@{ shape: cyl}
    style A1 fill:#f9f,stroke:#333,stroke-width:1px
    style A2 fill:#f9f,stroke:#333,stroke-width:1px
    style A3 fill:#f9f,stroke:#333,stroke-width:1px
    style A4 fill:#f9f,stroke:#333,stroke-width:1px
    style B1 fill:#bbf,stroke:#333,stroke-width:1px
    style B2 fill:#bbf,stroke:#333,stroke-width:1px
    style B3 fill:#bbf,stroke:#333,stroke-width:1px
    style C1 fill:#bfb,stroke:#333,stroke-width:1px
    style C2 fill:#bfb,stroke:#333,stroke-width:1px
    style C3 fill:#bfb,stroke:#333,stroke-width:1px
    style C4 fill:#bfb,stroke:#333,stroke-width:1px
    style C5 fill:#bfb,stroke:#333,stroke-width:1px
    style C6 fill:#bfb,stroke:#333,stroke-width:1px
    style C7 fill:#bfb,stroke:#333,stroke-width:1px
    style D1 fill:#fdd,stroke:#333,stroke-width:1px
    style G1 fill:#fdd,stroke:#333,stroke-width:1px
    style E1 fill:#ffd,stroke:#333,stroke-width:1px
    style E2 fill:#ffd,stroke:#333,stroke-width:1px
```

---

## 📚 Components

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
- **LLM Embeddings Generator:** Generates vector embeddings (using LLM APIs like OpenAI, Huggingface)

---

## 🚀 Features

- Multi-agent job description generation pipeline
- Embeddings-based semantic search
- CQRS (Command Query Responsibility Separation) based architecture
- Compliance, Critique, and Rewrite workflow
- Easy API-based integrations
- Modular and extensible design

---

## 🛠️ Tech Stack

- .NET 8 / ASP.NET Core Web API
- SQLite
- FAISS or Chroma for vector storage
- LLM services for embeddings (OpenAI / Huggingface)
- Docker (for local development)

---

## 🏁 Quick Start

1. **Clone**

```bash
git clone https://github.com/akhilaraop/agentic-jd-generator.git
cd agentic-jd-generator
```

2. **Set environment variables**

- Add `.env` file or configure API keys for embeddings generation

3. **Run**

```bash
dotnet build
dotnet run
```

4. **Access APIs**

Visit: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 📈 Roadmap

- 🔐 Secure authentication (OAuth2)
- 📊 Dashboard to visualize JD usage and analytics
- 🏛️ Organization-specific JD compliance templates
- 🌐 Multi-language JD generation
- 🧠 Fine-tuning agent prompts for industry-specific jobs

---

## 📄 License

This project is licensed under the MIT License.

