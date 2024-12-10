namespace WindSurfApi.Models
{
    public class Article
    {
        public string Code { get; set; }
        public string Designation { get; set; }
        public int Quantite { get; set; }
        public string ReferenceFournisseur { get; set; }
        public int QuantiteTerrain { get; set; }
        public string SousFamille { get; set; }
        public string Famille { get; set; }
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
