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
                "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;ART001;Article 1;FAM1;SFAM1;10;0;REF001",
                "EPS PARIS;205;ART002;Article 2;FAM1;SFAM1;5;0;REF002",
                "EPS PARIS;205;ART003;Article 3;FAM1;SFAM2;8;0;REF003",
                "EPS LYON;301;ART001;Article 1;FAM1;SFAM1;8;0;REF001"
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
            Assert.Contains(sfam1.Articles, a => a.Code == "ART001");
            Assert.Contains(sfam1.Articles, a => a.Code == "ART002");

            var sfam2 = famille.SousFamilles.First(sf => sf.SousFamille == "SFAM2");
            Assert.Single(sfam2.Articles);
            Assert.Contains(sfam2.Articles, a => a.Code == "ART003");
        }

        [Fact]
        public async Task UpdateQuantites_ShouldUpdateCorrectLines()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;ART001;Article 1;FAM1;SFAM1;10;0;REF001",
                "EPS PARIS;205;ART002;Article 2;FAM1;SFAM1;5;0;REF002"
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
            Assert.Contains("3", savedLines[1].Split(';')[7]); // Premier article
            Assert.Contains("2", savedLines[2].Split(';')[7]); // Deuxième article
        }

        [Fact]
        public async Task UpdateQuantites_WithNonExistentArticle_ShouldThrowException()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;ART001;Article 1;FAM1;SFAM1;10;0;REF001"
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
