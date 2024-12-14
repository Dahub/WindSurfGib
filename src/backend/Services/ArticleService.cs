using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WindSurfApi.Models;
using WindSurfApi.Services.Interfaces;

namespace WindSurfApi.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ICsvDataProvider _csvDataProvider;
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(ICsvDataProvider csvDataProvider, ILogger<ArticleService> logger)
        {
            _csvDataProvider = csvDataProvider;
            _logger = logger;
        }

        public IEnumerable<ArticleParFamille> GetArticles(string agence, string magasin)
        {
            try
            {
                var lines = _csvDataProvider.ReadAllLinesAsync().Result;
                var articlesParFamille = new Dictionary<string, ArticleParFamille>();

                foreach (var line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var values = line.Split(';');
                        if (values.Length < 9) 
                        {
                            _logger.LogWarning($"Ligne ignorée car elle ne contient pas assez de colonnes: {line}");
                            continue;
                        }

                        if (string.Equals(values[0].Trim(), agence.Trim(), StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(values[1].Trim(), magasin.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            var article = new Article
                            {
                                Code = values[3].Trim(),
                                Designation = values[4].Trim(),
                                Famille = values[5].Trim(),
                                SousFamille = values[6].Trim(),
                                Quantite = int.TryParse(values[7].Trim(), out int quantite) ? quantite : 0,
                                QuantiteTerrain = int.TryParse(values[9].Trim(), out int quantiteTerrain) ? quantiteTerrain : 0,
                                ReferenceFournisseur = values[8].Trim()
                            };

                            if (!articlesParFamille.TryGetValue(article.Famille, out var famille))
                            {
                                famille = new ArticleParFamille { Famille = article.Famille };
                                articlesParFamille[article.Famille] = famille;
                            }

                            var sousFamille = famille.SousFamilles.FirstOrDefault(sf => sf.SousFamille == article.SousFamille);
                            if (sousFamille == null)
                            {
                                sousFamille = new ArticleParSousFamille { SousFamille = article.SousFamille };
                                famille.SousFamilles.Add(sousFamille);
                            }

                            sousFamille.Articles.Add(article);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erreur lors du traitement de la ligne: {line}");
                        continue;
                    }
                }

                var result = articlesParFamille.Values
                    .OrderBy(f => f.Famille)
                    .Select(f => new ArticleParFamille
                    {
                        Famille = f.Famille,
                        SousFamilles = f.SousFamilles
                            .OrderBy(sf => sf.SousFamille)
                            .Select(sf => new ArticleParSousFamille
                            {
                                SousFamille = sf.SousFamille,
                                Articles = sf.Articles.OrderBy(a => a.Code).ToList()
                            })
                            .ToList()
                    })
                    .ToList();

                _logger.LogInformation($"Retour de {result.Count} familles d'articles pour l'agence {agence}, magasin {magasin}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des articles pour l'agence {agence}, magasin {magasin}");
                throw;
            }
        }

        public async Task UpdateQuantites(string agence, string magasin, List<UpdateQuantiteRequest> articles)
        {
            try
            {
                var lines = await _csvDataProvider.ReadAllLinesAsync();
                if (lines == null || !lines.Any())
                {
                    throw new Exception("Le fichier CSV est vide");
                }

                var updatedLines = new List<string> { lines[0] }; 
                var articlesUpdated = new HashSet<string>();

                foreach (var line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(';');
                    if (values.Length < 9) continue;

                    var currentLine = line;
                    if (string.Equals(values[0].Trim(), agence.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(values[1].Trim(), magasin.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var articleCode = values[3].Trim();
                        var articleToUpdate = articles.FirstOrDefault(a => 
                            string.Equals(a.CodeArticle.Trim(), articleCode, StringComparison.OrdinalIgnoreCase));

                        if (articleToUpdate != null)
                        {
                            values[9] = articleToUpdate.QuantiteTerrain.ToString();
                            currentLine = string.Join(";", values);
                            articlesUpdated.Add(articleCode);
                            _logger.LogInformation($"Article mis à jour : {articleCode}, nouvelle quantité terrain : {articleToUpdate.QuantiteTerrain}");
                        }
                    }
                    updatedLines.Add(currentLine);
                }

                var nonUpdatedArticles = articles
                    .Where(a => !articlesUpdated.Contains(a.CodeArticle))
                    .Select(a => a.CodeArticle)
                    .ToList();

                if (nonUpdatedArticles.Any())
                {
                    var message = $"Les articles suivants n'ont pas été trouvés : {string.Join(", ", nonUpdatedArticles)}";
                    _logger.LogError(message);
                    throw new Exception(message);
                }

                await _csvDataProvider.WriteAllLinesAsync(updatedLines);
                _logger.LogInformation($"Mise à jour réussie pour {articlesUpdated.Count} articles");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la mise à jour des quantités pour l'agence {agence}, magasin {magasin}");
                throw;
            }
        }
    }
}
