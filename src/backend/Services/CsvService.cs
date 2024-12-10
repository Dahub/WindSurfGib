using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace WindSurfApi.Services
{
    public class CsvService
    {
        private readonly string _csvFilePath;

        public CsvService(IConfiguration configuration)
        {
            // Utilisation d'un chemin relatif depuis la racine du projet
            _csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "source.csv");
        }

        public IEnumerable<string> GetAgences()
        {
            if (!File.Exists(_csvFilePath))
            {
                throw new FileNotFoundException($"Le fichier CSV n'a pas été trouvé à l'emplacement : {_csvFilePath}");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null, // Ignorer les lignes avec des données incorrectes
                MissingFieldFound = null, // Ignorer les champs manquants
                HeaderValidated = null, // Ignorer la validation des en-têtes
                Delimiter = ";", // Utiliser le point-virgule comme délimiteur
                HasHeaderRecord = true
            };

            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, config);
            
            var agences = new HashSet<string>();
            
            // Ignorer la première ligne qui contient les en-têtes
            csv.Read();
            
            while (csv.Read())
            {
                try
                {
                    var agence = csv.GetField(0)?.Trim(); // Première colonne
                    if (!string.IsNullOrWhiteSpace(agence))
                    {
                        agences.Add(agence.Trim());
                    }
                }
                catch
                {
                    // Ignorer les lignes qui ne peuvent pas être lues
                    continue;
                }
            }

            return agences.OrderBy(a => a);
        }

        public IEnumerable<Magasin> GetMagasins(string agence)
        {
            if (!File.Exists(_csvFilePath))
            {
                throw new FileNotFoundException($"Le fichier CSV n'a pas été trouvé à l'emplacement : {_csvFilePath}");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null,
                Delimiter = ";",
                HasHeaderRecord = true
            };

            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, config);
            
            var magasins = new HashSet<Magasin>(new MagasinComparer());
            
            // Ignorer la première ligne qui contient les en-têtes
            csv.Read();
            
            while (csv.Read())
            {
                try
                {
                    var currentAgence = csv.GetField(0)?.Trim();
                    if (currentAgence?.Equals(agence, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var codeMagasin = csv.GetField(1)?.Trim();
                        var nomMagasin = csv.GetField(2)?.Trim();
                        
                        if (!string.IsNullOrWhiteSpace(codeMagasin) && !string.IsNullOrWhiteSpace(nomMagasin))
                        {
                            magasins.Add(new Magasin 
                            { 
                                Code = codeMagasin,
                                Nom = nomMagasin
                            });
                        }
                    }
                }
                catch
                {
                    // Ignorer les lignes qui ne peuvent pas être lues
                    continue;
                }
            }

            return magasins.OrderBy(m => m.Nom);
        }

        public IEnumerable<ArticleParFamille> GetArticles(string codeMagasin)
        {
            if (!File.Exists(_csvFilePath))
            {
                throw new FileNotFoundException($"Le fichier CSV n'a pas été trouvé à l'emplacement : {_csvFilePath}");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                MissingFieldFound = null,
                HeaderValidated = null,
                Delimiter = ";",
                HasHeaderRecord = true
            };

            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, config);
            
            var articlesParFamille = new Dictionary<string, ArticleParFamille>();
            
            // Ignorer la première ligne qui contient les en-têtes
            csv.Read();
            
            while (csv.Read())
            {
                try
                {
                    var currentCodeMagasin = csv.GetField(1)?.Trim();
                    if (currentCodeMagasin?.Equals(codeMagasin, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var article = new Article
                        {
                            Code = csv.GetField(3)?.Trim(),
                            Designation = csv.GetField(4)?.Trim(),
                            Famille = csv.GetField(5)?.Trim() ?? "Sans Famille",
                            SousFamille = csv.GetField(6)?.Trim() ?? "Sans Sous-Famille",
                            Quantite = int.TryParse(csv.GetField(7), out int qte) ? qte : 0,
                            ReferenceFournisseur = csv.GetField(8)?.Trim(),
                            QuantiteTerrain = int.TryParse(csv.GetField(9), out int qteTerrain) ? qteTerrain : 0
                        };

                        if (!string.IsNullOrWhiteSpace(article.Code))
                        {
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
                }
                catch
                {
                    // Ignorer les lignes qui ne peuvent pas être lues
                    continue;
                }
            }

            // Trier les familles, sous-familles et articles
            var result = articlesParFamille.Values
                .OrderBy(f => f.Famille)
                .ToList();

            foreach (var famille in result)
            {
                famille.SousFamilles = famille.SousFamilles
                    .OrderBy(sf => sf.SousFamille)
                    .ToList();

                foreach (var sousFamille in famille.SousFamilles)
                {
                    sousFamille.Articles = sousFamille.Articles
                        .OrderBy(a => a.Code)
                        .ToList();
                }
            }

            return result;
        }
    }

    public class Magasin
    {
        public string Code { get; set; }
        public string Nom { get; set; }
    }

    // Comparer pour éviter les doublons de magasins
    public class MagasinComparer : IEqualityComparer<Magasin>
    {
        public bool Equals(Magasin x, Magasin y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return x.Code == y.Code;
        }

        public int GetHashCode(Magasin obj)
        {
            return obj.Code != null ? obj.Code.GetHashCode() : 0;
        }
    }

    public class Article
    {
        public string Code { get; set; }
        public string Designation { get; set; }
        public string Famille { get; set; }
        public string SousFamille { get; set; }
        public int Quantite { get; set; }
        public string ReferenceFournisseur { get; set; }
        public int QuantiteTerrain { get; set; }
    }

    public class ArticleParFamille
    {
        public string Famille { get; set; }
        public List<ArticleParSousFamille> SousFamilles { get; set; } = new List<ArticleParSousFamille>();
    }

    public class ArticleParSousFamille
    {
        public string SousFamille { get; set; }
        public List<Article> Articles { get; set; } = new List<Article>();
    }
}
