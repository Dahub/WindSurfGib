using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WindSurfApi.Services.Interfaces;

namespace WindSurfApi.Services
{
    public class AgenceService : IAgenceService
    {
        private readonly ICsvDataProvider _csvDataProvider;
        private readonly ILogger<AgenceService> _logger;

        public AgenceService(ICsvDataProvider csvDataProvider, ILogger<AgenceService> logger)
        {
            _csvDataProvider = csvDataProvider;
            _logger = logger;
        }

        public IEnumerable<string> GetAgences()
        {
            try
            {
                var lines = _csvDataProvider.ReadAllLinesAsync().Result;
                return lines.Skip(1) // Skip header
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .Select(line => line.Split(';')[0]) // Agence is first column
                          .Distinct()
                          .OrderBy(a => a);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des agences");
                throw;
            }
        }
    }
}
