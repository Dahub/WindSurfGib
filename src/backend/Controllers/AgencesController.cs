using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WindSurfApi.Services;

namespace WindSurfApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencesController : ControllerBase
    {
        private readonly CsvService _csvService;

        public AgencesController(CsvService csvService)
        {
            _csvService = csvService;
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
                var agences = _csvService.GetAgences();
                return Ok(agences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur est survenue : {ex.Message}");
            }
        }

        [HttpGet("{agence}/magasins")]
        public IActionResult GetMagasins(string agence)
        {
            try
            {
                var magasins = _csvService.GetMagasins(agence);
                if (!magasins.Any())
                {
                    return NotFound($"Aucun magasin trouvé pour l'agence : {agence}");
                }
                return Ok(magasins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur est survenue : {ex.Message}");
            }
        }

        [HttpGet("{agence}/magasins/{codeMagasin}/articles")]
        public ActionResult<IEnumerable<ArticleParFamille>> GetArticles(string agence, string codeMagasin)
        {
            try
            {
                // Vérifier d'abord si le magasin existe pour cette agence
                var magasins = _csvService.GetMagasins(agence);
                if (!magasins.Any(m => m.Code.Equals(codeMagasin, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound($"Le magasin avec le code {codeMagasin} n'existe pas pour l'agence {agence}");
                }

                var articles = _csvService.GetArticles(codeMagasin);
                return Ok(articles);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Une erreur est survenue lors de la récupération des articles : {ex.Message}");
            }
        }
    }
}
