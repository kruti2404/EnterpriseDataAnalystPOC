using System.Threading.Tasks;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IAiService
{
    Task<T> GenerateJsonAsync<T>(string prompt);
    Task<string> GenerateTextAsync(string prompt);
}
