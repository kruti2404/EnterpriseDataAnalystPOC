using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IWebSearchAgent
{
    Task<List<WebSearchResult>> SearchAsync(string query);
}
