using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WindSurfApi.Models;
using WindSurfApi.Services.Interfaces;
using WindSurfApi.Exceptions;

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
            var lines = _csvDataProvider.ReadAllLinesAsync().Result;
            
            if (lines == null || !lines.Any())
            {
                _logger.LogWarning("Le fichier CSV est vide");
                throw new InvalidBusinessDataException("Le fichier CSV est vide");
            }

            var magasins = lines.Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(';'))
                .Where(values => values.Length >= 3 && 
                    string.Equals(values[0].Trim(), agence.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(values => new Magasin
                {
                    Code = values[1].Trim(),
                    Nom = values[2].Trim()
                })
                .DistinctBy(m => m.Code)
                .OrderBy(m => m.Code)
                .ToList();

            if (!magasins.Any())
            {
                _logger.LogWarning($"Aucun magasin trouvé pour l'agence : {agence}");
                throw new ResourceNotFoundException($"Aucun magasin trouvé pour l'agence : {agence}");
            }

            return magasins;
        }
    }
}
