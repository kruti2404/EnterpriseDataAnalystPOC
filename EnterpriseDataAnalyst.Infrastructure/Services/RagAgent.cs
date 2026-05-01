using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class RagAgent : IRagAgent
{
    public Task<IReadOnlyList<DocumentChunk>> RetrieveContextAsync(string question)
    {
        // Mocking RAG retrieval for the POC
        var chunks = new List<DocumentChunk>();
        
        if (question.ToLower().Contains("west"))
        {
            chunks.Add(new DocumentChunk 
            { 
                SourceId = "doc_sales_report_q3", 
                Content = "In Q3, the West region faced severe supply chain disruptions leading to a 30% drop in overall inventory availability.", 
                RelevanceScore = 0.95 
            });
            chunks.Add(new DocumentChunk 
            { 
                SourceId = "doc_competitor_analysis", 
                Content = "A major competitor launched a new product line in the West coast, taking some market share.", 
                RelevanceScore = 0.88 
            });
        }
        else
        {
            chunks.Add(new DocumentChunk 
            { 
                SourceId = "doc_general_update", 
                Content = "Overall sales are stable across most regions. New marketing campaigns are planned for next quarter.", 
                RelevanceScore = 0.70 
            });
        }

        return Task.FromResult<IReadOnlyList<DocumentChunk>>(chunks);
    }
}
