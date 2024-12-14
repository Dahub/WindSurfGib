using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WindSurfApi.Services;
using Xunit;

namespace WindSurfApi.Tests
{
    public class CsvDataProviderTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<CsvDataProvider>> _loggerMock;
        private readonly string _tempFilePath;

        public CsvDataProviderTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<CsvDataProvider>>();
            _tempFilePath = Path.GetTempFileName();
        }

        private CsvDataProvider CreateProvider()
        {
            _configurationMock.Setup(x => x["CsvFilePath"])
                             .Returns(_tempFilePath);
            return new CsvDataProvider(_configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ReadAllLinesAsync_ShouldReturnAllLines()
        {
            // Arrange
            var testLines = new[]
            {
                "Header1;Header2",
                "Value1;Value2",
                "Value3;Value4"
            };
            await File.WriteAllLinesAsync(_tempFilePath, testLines);
            var provider = CreateProvider();

            // Act
            var result = await provider.ReadAllLinesAsync();

            // Assert
            Assert.Equal(testLines, result);
        }

        [Fact]
        public async Task WriteAllLinesAsync_ShouldWriteAllLines()
        {
            // Arrange
            var testLines = new[]
            {
                "Header1;Header2",
                "Value1;Value2",
                "Value3;Value4"
            };
            var provider = CreateProvider();

            // Act
            await provider.WriteAllLinesAsync(testLines);
            var writtenLines = await File.ReadAllLinesAsync(_tempFilePath);

            // Assert
            Assert.Equal(testLines, writtenLines);
        }

        [Fact]
        public async Task ReadAllLinesAsync_WithEmptyFile_ShouldReturnEmptyArray()
        {
            // Arrange
            await File.WriteAllTextAsync(_tempFilePath, string.Empty);
            var provider = CreateProvider();

            // Act
            var result = await provider.ReadAllLinesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task WriteAllLinesAsync_WithEmptyArray_ShouldCreateEmptyFile()
        {
            // Arrange
            var provider = CreateProvider();

            // Act
            await provider.WriteAllLinesAsync(Array.Empty<string>());
            var fileContent = await File.ReadAllTextAsync(_tempFilePath);

            // Assert
            Assert.Empty(fileContent);
        }

        public void Dispose()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
    }
}
