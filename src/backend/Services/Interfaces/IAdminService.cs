using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WindSurfApi.Services.Interfaces
{
    public interface IAdminService
    {
        Task DeleteCsvFile();
        Task<byte[]> DownloadCsvFile();
        Task MergeCsvFile(IFormFile file);
    }
}
