# Enterprise Data Analyst Platform POC

An intelligent, full-stack data analysis platform designed to act as a virtual data analyst for enterprise databases. Built on a modern **.NET Core** and **Angular** stack, this application leverages a sophisticated Multi-Agent AI architecture powered by the **Ollama Cloud API**.

### 🧠 How It Works
The system utilizes a 5-tier agent architecture to process user requests:
1. **Orchestrator:** Plans the workflow and delegates tasks.
2. **Data Agent:** Constructs dynamic SQL queries and retrieves relevant database records.
3. **RAG Agent:** Retrieves necessary contextual information and domain knowledge.
4. **Analysis Agent:** Processes the raw data to extract meaningful business insights and formats structured chart data.
5. **Validation Agent:** Cross-references the AI-generated insights against actual database results to ensure strict data accuracy.

### 🚀 Key Features
*   Automated SQL generation and execution from natural language.
*   Automated charting and UI visualization via Angular.
*   Seamless CSV data ingestion for rapid Proof-of-Concept testing.
*   Strict validation layer to prevent AI hallucinations.
