using System.Collections.Generic;
using System.Threading.Tasks;

namespace WindSurfApi.Services.Interfaces
{
    public interface ICsvDataProvider
    {
        Task<string[]> ReadAllLinesAsync();
        Task WriteAllLinesAsync(IEnumerable<string> lines);
    }
}
