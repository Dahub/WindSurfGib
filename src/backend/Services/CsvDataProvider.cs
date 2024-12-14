using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WindSurfApi.Services.Interfaces;

namespace WindSurfApi.Services
{
    public class CsvDataProvider : ICsvDataProvider
    {
        private readonly string _csvFilePath;
        private readonly ILogger<CsvDataProvider> _logger;

        public CsvDataProvider(IConfiguration configuration, ILogger<CsvDataProvider> logger)
        {
            _csvFilePath = configuration["CsvFilePath"];
            _logger = logger;
        }

        public async Task<string[]> ReadAllLinesAsync()
        {
            try
            {
                return await File.ReadAllLinesAsync(_csvFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la lecture du fichier CSV");
                throw;
            }
        }

        public async Task WriteAllLinesAsync(IEnumerable<string> lines)
        {
            try
            {
                await File.WriteAllLinesAsync(_csvFilePath, lines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'écriture dans le fichier CSV");
                throw;
            }
        }
    }
}
