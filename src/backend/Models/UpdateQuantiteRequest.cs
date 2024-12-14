namespace WindSurfApi.Models;

public class UpdateQuantiteRequest
{
    public string CodeArticle { get; set; }
    public int QuantiteTerrain { get; set; }
}

public class UpdateQuantitesRequest
{
    public string Agence { get; set; }
    public string Magasin { get; set; }
    public List<UpdateQuantiteRequest> Articles { get; set; }
}
