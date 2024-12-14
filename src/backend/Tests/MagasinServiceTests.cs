using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using WindSurfApi.Models;
using WindSurfApi.Services;
using WindSurfApi.Services.Interfaces;
using Xunit;

namespace WindSurfApi.Tests
{
    public class MagasinServiceTests
    {
        private readonly Mock<ICsvDataProvider> _csvDataProviderMock;
        private readonly Mock<ILogger<MagasinService>> _loggerMock;
        private readonly MagasinService _magasinService;

        public MagasinServiceTests()
        {
            _csvDataProviderMock = new Mock<ICsvDataProvider>();
            _loggerMock = new Mock<ILogger<MagasinService>>();
            _magasinService = new MagasinService(_csvDataProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetMagasins_ShouldReturnDistinctMagasins()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;NomMagasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;0;REF001",
                "EPS PARIS;205;Paris Centre;ART002;Article 2;FAM1;SFAM1;5;0;REF002",
                "EPS PARIS;206;Paris Sud;ART003;Article 3;FAM1;SFAM2;8;0;REF003",
                "EPS LYON;301;Lyon Centre;ART001;Article 1;FAM1;SFAM1;8;0;REF001"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _magasinService.GetMagasins("EPS PARIS").ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Code == "205" && m.Nom == "Paris Centre");
            Assert.Contains(result, m => m.Code == "206" && m.Nom == "Paris Sud");
        }

        [Fact]
        public void GetMagasins_WithNonExistentAgence_ShouldReturnEmptyList()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;NomMagasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;0;REF001"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _magasinService.GetMagasins("AGENCE INCONNUE").ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetMagasins_WithEmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var csvLines = new[] { "Agence;Magasin;NomMagasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur" };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _magasinService.GetMagasins("EPS PARIS").ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetMagasins_ShouldBeCaseInsensitive()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;NomMagasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;0;REF001"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _magasinService.GetMagasins("eps Paris").ToList();

            // Assert
            Assert.Single(result);
            Assert.Contains(result, m => m.Code == "205" && m.Nom == "Paris Centre");
        }
    }
}
