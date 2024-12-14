using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using WindSurfApi.Services;
using WindSurfApi.Services.Interfaces;
using Xunit;

namespace WindSurfApi.Tests
{
    public class AgenceServiceTests
    {
        private readonly Mock<ICsvDataProvider> _csvDataProviderMock;
        private readonly Mock<ILogger<AgenceService>> _loggerMock;
        private readonly AgenceService _agenceService;

        public AgenceServiceTests()
        {
            _csvDataProviderMock = new Mock<ICsvDataProvider>();
            _loggerMock = new Mock<ILogger<AgenceService>>();
            _agenceService = new AgenceService(_csvDataProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetAgences_ShouldReturnDistinctAgences()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;ART001;Article 1;FAM1;SFAM1;10;0;REF001",
                "EPS PARIS;206;ART002;Article 2;FAM1;SFAM1;5;0;REF002",
                "EPS LYON;301;ART003;Article 3;FAM1;SFAM2;8;0;REF003",
                "EPS LYON;302;ART001;Article 1;FAM1;SFAM1;8;0;REF001"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _agenceService.GetAgences().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a == "EPS PARIS");
            Assert.Contains(result, a => a == "EPS LYON");
        }

        [Fact]
        public void GetAgences_WithEmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var csvLines = new[] { "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur" };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _agenceService.GetAgences().ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetAgences_ShouldReturnSortedList()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;ART001;Article 1;FAM1;SFAM1;10;0;REF001",
                "EPS LYON;301;ART003;Article 3;FAM1;SFAM2;8;0;REF003",
                "EPS BORDEAUX;401;ART002;Article 2;FAM1;SFAM1;5;0;REF002"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _agenceService.GetAgences().ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("EPS BORDEAUX", result[0]);
            Assert.Equal("EPS LYON", result[1]);
            Assert.Equal("EPS PARIS", result[2]);
        }

        [Fact]
        public void GetAgences_WithDuplicateAgences_ShouldReturnDistinctValues()
        {
            // Arrange
            var csvLines = new[]
            {
                "Agence;Magasin;Code;Designation;Famille;SousFamille;Quantite;QuantiteTerrain;ReferenceFournisseur",
                "EPS PARIS;205;ART001;Article 1;FAM1;SFAM1;10;0;REF001",
                "EPS PARIS;205;ART002;Article 2;FAM1;SFAM1;5;0;REF002",
                "EPS PARIS;206;ART003;Article 3;FAM1;SFAM2;8;0;REF003"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act
            var result = _agenceService.GetAgences().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("EPS PARIS", result[0]);
        }
    }
}
