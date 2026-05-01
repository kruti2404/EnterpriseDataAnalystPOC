using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IRagAgent
{
    Task<IReadOnlyList<DocumentChunk>> RetrieveContextAsync(string question);
}
