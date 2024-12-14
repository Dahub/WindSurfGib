using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WindSurfApi.Models;
using WindSurfApi.Services.Interfaces;

namespace WindSurfApi.Services
{
    public class MagasinService : IMagasinService
    {
        private readonly ICsvDataProvider _csvDataProvider;
        private readonly ILogger<MagasinService> _logger;

        public MagasinService(ICsvDataProvider csvDataProvider, ILogger<MagasinService> logger)
        {
            _csvDataProvider = csvDataProvider;
            _logger = logger;
        }

        public IEnumerable<Magasin> GetMagasins(string agence)
        {
            try
            {
                var lines = _csvDataProvider.ReadAllLinesAsync().Result;
                return lines.Skip(1) // Skip header
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .Select(line => line.Split(';'))
                          .Where(values => values.Length >= 3 && 
                                 string.Equals(values[0].Trim(), agence.Trim(), StringComparison.OrdinalIgnoreCase))
                          .Select(values => new Magasin 
                          { 
                              Code = values[1], 
                              Nom = values[2] 
                          })
                          .Distinct(new MagasinComparer())
                          .OrderBy(m => m.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des magasins pour l'agence {agence}");
                throw;
            }
        }
    }
}
