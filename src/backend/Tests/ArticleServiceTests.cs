using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WindSurfApi.Models;
using WindSurfApi.Services;
using WindSurfApi.Services.Interfaces;
using Xunit;

namespace WindSurfApi.Tests
{
    public class ArticleServiceTests
    {
        private readonly Mock<ICsvDataProvider> _csvDataProviderMock;
        private readonly Mock<ILogger<ArticleService>> _loggerMock;
        private readonly ArticleService _articleService;

        public ArticleServiceTests()
        {
            _csvDataProviderMock = new Mock<ICsvDataProvider>();
            _loggerMock = new Mock<ILogger<ArticleService>>();
            _articleService = new ArticleService(_csvDataProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetArticles_ShouldReturnGroupedArticles()
        {
            // Arrange
            var csvLines = new[]
            {
                "Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;REF001;5",
                "EPS PARIS;205;Paris Centre;ART002;Article 2;FAM1;SFAM1;5;REF002;3",
                "EPS PARIS;205;Paris Centre;ART003;Article 3;FAM1;SFAM2;8;REF003;2",
                "EPS LYON;301;Lyon Centre;ART001;Article 1;FAM1;SFAM1;8;REF001;1"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _articleService.GetArticles("EPS PARIS", "205").ToList();

            // Assert
            Assert.Single(result); // Une seule famille
            var famille = result[0];
            Assert.Equal("FAM1", famille.Famille);
            Assert.Equal(2, famille.SousFamilles.Count); // Deux sous-familles

            var sfam1 = famille.SousFamilles.First(sf => sf.SousFamille == "SFAM1");
            Assert.Equal(2, sfam1.Articles.Count);
            var art1 = sfam1.Articles.First(a => a.Code == "ART001");
            Assert.Equal("Article 1", art1.Designation);
            Assert.Equal(10, art1.Quantite);
            Assert.Equal(5, art1.QuantiteTerrain);
            Assert.Equal("REF001", art1.ReferenceFournisseur);

            var sfam2 = famille.SousFamilles.First(sf => sf.SousFamille == "SFAM2");
            Assert.Single(sfam2.Articles);
            var art3 = sfam2.Articles.First();
            Assert.Equal("ART003", art3.Code);
            Assert.Equal(2, art3.QuantiteTerrain);
        }

        [Fact]
        public async Task UpdateQuantites_ShouldUpdateCorrectLines()
        {
            // Arrange
            var csvLines = new[]
            {
                "Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;REF001;0",
                "EPS PARIS;205;Paris Centre;ART002;Article 2;FAM1;SFAM1;5;REF002;0"
            };

            var updates = new List<UpdateQuantiteRequest>
            {
                new UpdateQuantiteRequest { CodeArticle = "ART001", QuantiteTerrain = 3 },
                new UpdateQuantiteRequest { CodeArticle = "ART002", QuantiteTerrain = 2 }
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            List<string> savedLines = null;
            _csvDataProviderMock.Setup(x => x.WriteAllLinesAsync(It.IsAny<IEnumerable<string>>()))
                               .Callback<IEnumerable<string>>(lines => savedLines = lines.ToList())
                               .Returns(Task.CompletedTask);

            // Act
            await _articleService.UpdateQuantites("EPS PARIS", "205", updates);

            // Assert
            Assert.NotNull(savedLines);
            Assert.Equal(3, savedLines.Count); // Header + 2 lines
            var line1Parts = savedLines[1].Split(';');
            var line2Parts = savedLines[2].Split(';');
            Assert.Equal("3", line1Parts[9]); // Premier article
            Assert.Equal("2", line2Parts[9]); // Deuxième article
        }

        [Fact]
        public async Task UpdateQuantites_WithNonExistentArticle_ShouldThrowException()
        {
            // Arrange
            var csvLines = new[]
            {
                "Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;REF001;0"
            };

            var updates = new List<UpdateQuantiteRequest>
            {
                new UpdateQuantiteRequest { CodeArticle = "NONEXISTENT", QuantiteTerrain = 3 }
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _articleService.UpdateQuantites("EPS PARIS", "205", updates));
        }

        [Fact]
        public async Task UpdateQuantites_WithEmptyFile_ShouldThrowException()
        {
            // Arrange
            var csvLines = Array.Empty<string>();

            var updates = new List<UpdateQuantiteRequest>
            {
                new UpdateQuantiteRequest { CodeArticle = "ART001", QuantiteTerrain = 3 }
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _articleService.UpdateQuantites("EPS PARIS", "205", updates));
        }
    }
}
