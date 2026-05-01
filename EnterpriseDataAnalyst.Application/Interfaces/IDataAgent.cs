using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IDataAgent
{
    Task<List<SalesSummary>> FetchDataAsync(Plan plan);
}
