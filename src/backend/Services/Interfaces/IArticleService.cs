using System.Collections.Generic;
using System.Threading.Tasks;
using WindSurfApi.Models;

namespace WindSurfApi.Services.Interfaces
{
    public interface IArticleService
    {
        IEnumerable<ArticleParFamille> GetArticles(string agence, string magasin);
        Task UpdateQuantites(string agence, string magasin, List<UpdateQuantiteRequest> articles);
    }
}
