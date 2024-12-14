using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using WindSurfApi.Models;
using WindSurfApi.Services;
using WindSurfApi.Services.Interfaces;
using WindSurfApi.Exceptions;
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
        public void GetMagasins_ShouldReturnMagasins()
        {
            // Arrange
            var csvLines = new[]
            {
                "Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;REF001;5",
                "EPS PARIS;206;Paris Sud;ART001;Article 1;FAM1;SFAM1;8;REF001;3"
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
        public void GetMagasins_WithNonExistentAgence_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var csvLines = new[]
            {
                "Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain",
                "EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;REF001;5"
            };

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act & Assert
            var exception = Assert.Throws<ResourceNotFoundException>(() => 
                _magasinService.GetMagasins("AGENCE INCONNUE"));
            Assert.Equal("Aucun magasin trouvé pour l'agence : AGENCE INCONNUE", exception.Message);
        }

        [Fact]
        public void GetMagasins_WithEmptyFile_ShouldThrowInvalidBusinessDataException()
        {
            // Arrange
            var csvLines = Array.Empty<string>();

            _csvDataProviderMock.Setup(x => x.ReadAllLinesAsync())
                               .ReturnsAsync(csvLines);

            // Act & Assert
            var exception = Assert.Throws<InvalidBusinessDataException>(() => 
                _magasinService.GetMagasins("EPS PARIS"));
            Assert.Equal("Le fichier CSV est vide", exception.Message);
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
