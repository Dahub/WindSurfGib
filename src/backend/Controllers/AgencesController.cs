using Microsoft.AspNetCore.Mvc;
using WindSurfApi.Services.Interfaces;
using WindSurfApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WindSurfApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencesController : ControllerBase
    {
        private readonly IAgenceService _agenceService;
        private readonly IMagasinService _magasinService;
        private readonly IArticleService _articleService;
        private readonly ILogger<AgencesController> _logger;

        public AgencesController(
            IAgenceService agenceService,
            IMagasinService magasinService,
            IArticleService articleService,
            ILogger<AgencesController> logger)
        {
            _agenceService = agenceService;
            _magasinService = magasinService;
            _articleService = articleService;
            _logger = logger;
        }

        /// <summary>
        /// Récupère la liste des agences uniques
        /// </summary>
        /// <returns>Liste des noms d'agences</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult GetAgences()
        {
            try
            {
                var agences = _agenceService.GetAgences();
                return Ok(agences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des agences");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des agences" });
            }
        }

        [HttpGet("{agence}/magasins")]
        public IActionResult GetMagasins(string agence)
        {
            try
            {
                var magasins = _magasinService.GetMagasins(agence);
                if (!magasins.Any())
                {
                    return NotFound($"Aucun magasin trouvé pour l'agence : {agence}");
                }
                return Ok(magasins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des magasins pour l'agence {agence}");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des magasins" });
            }
        }

        [HttpGet("{agence}/magasins/{magasin}/articles")]
        public IActionResult GetArticles(string agence, string magasin)
        {
            try
            {
                var articles = _articleService.GetArticles(agence, magasin);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération des articles pour l'agence {agence}, magasin {magasin}");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des articles" });
            }
        }

        [HttpPost("{agence}/magasins/{magasin}/articles")]
        public async Task<IActionResult> UpdateQuantites(string agence, string magasin, [FromBody] List<UpdateQuantiteRequest> articles)
        {
            try
            {
                _logger.LogInformation($"Réception d'une demande de mise à jour pour l'agence '{agence}', magasin '{magasin}'");
                _logger.LogInformation($"Nombre d'articles à mettre à jour : {articles?.Count ?? 0}");
                if (articles != null)
                {
                    foreach (var article in articles)
                    {
                        _logger.LogInformation($"Article à mettre à jour : Code='{article.CodeArticle}', Quantité={article.QuantiteTerrain}");
                    }
                }

                // Vérifier d'abord si le magasin existe
                var magasins = _magasinService.GetMagasins(agence);
                if (!magasins.Any(m => m.Code.Equals(magasin, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning($"Magasin non trouvé. Agence='{agence}', Magasin='{magasin}'");
                    return NotFound($"Le magasin avec le code {magasin} n'existe pas pour l'agence {agence}");
                }

                await _articleService.UpdateQuantites(agence, magasin, articles);
                _logger.LogInformation("Mise à jour des quantités réussie");
                return Ok(new { message = "Quantités mises à jour avec succès" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la mise à jour des quantités. Agence='{agence}', Magasin='{magasin}'");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la mise à jour des quantités" });
            }
        }
    }
}
