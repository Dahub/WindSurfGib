using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WindSurfApi.Exceptions;
using WindSurfApi.Services.Interfaces;

namespace WindSurfApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly ICsvDataProvider _csvDataProvider;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ICsvDataProvider csvDataProvider, ILogger<AdminService> logger)
        {
            _csvDataProvider = csvDataProvider;
            _logger = logger;
        }

        public async Task DeleteCsvFile()
        {
            try
            {
                await _csvDataProvider.DeleteFileAsync();
                _logger.LogInformation("Fichier CSV supprimé avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du fichier CSV");
                throw new InvalidBusinessDataException("Impossible de supprimer le fichier CSV");
            }
        }

        public async Task<byte[]> DownloadCsvFile()
        {
            try
            {
                var content = await _csvDataProvider.ReadAllLinesAsync();
                if (content == null || !content.Any())
                {
                    throw new ResourceNotFoundException("Le fichier CSV est introuvable ou vide");
                }

                return System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, content));
            }
            catch (ResourceNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du téléchargement du fichier CSV");
                throw new InvalidBusinessDataException("Impossible de télécharger le fichier CSV");
            }
        }

        public async Task MergeCsvFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidBusinessDataException("Aucun fichier n'a été fourni");
            }

            try
            {
                // Lire le fichier existant
                var existingLines = (await _csvDataProvider.ReadAllLinesAsync()).ToList();
                var header = existingLines.FirstOrDefault();
                if (string.IsNullOrEmpty(header))
                {
                    header = "Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain";
                    existingLines = new List<string> { header };
                }

                // Lire le nouveau fichier
                var newLines = new List<string>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    string line;
                    bool isFirstLine = true;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (isFirstLine)
                        {
                            // Vérifier que l'en-tête correspond
                            if (!string.Equals(line.Trim(), header.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidBusinessDataException("Le format du fichier uploadé ne correspond pas au format attendu");
                            }
                            isFirstLine = false;
                            continue;
                        }
                        newLines.Add(line);
                    }
                }

                // Créer un dictionnaire pour le dédoublonnement
                var uniqueLines = new Dictionary<string, string>();
                
                // Traiter d'abord les lignes existantes
                foreach (var line in existingLines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';');
                    if (parts.Length < 4) continue;
                    
                    var key = $"{parts[0].Trim()};{parts[1].Trim()};{parts[3].Trim()}"; // agence;magasin;article
                    uniqueLines[key] = line;
                }

                // Puis les nouvelles lignes (écrasent les anciennes en cas de doublon)
                foreach (var line in newLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(';');
                    if (parts.Length < 4) continue;
                    
                    var key = $"{parts[0].Trim()};{parts[1].Trim()};{parts[3].Trim()}"; // agence;magasin;article
                    uniqueLines[key] = line;
                }

                // Reconstruire le fichier final
                var mergedLines = new List<string> { header };
                mergedLines.AddRange(uniqueLines.Values.OrderBy(l => l));

                // Sauvegarder le résultat
                await _csvDataProvider.WriteAllLinesAsync(mergedLines);
                _logger.LogInformation($"Fusion réussie : {uniqueLines.Count} lignes uniques");
            }
            catch (InvalidBusinessDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la fusion du fichier CSV");
                throw new InvalidBusinessDataException("Impossible de fusionner le fichier CSV");
            }
        }
    }
}
