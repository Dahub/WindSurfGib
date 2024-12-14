using System.Collections.Generic;
using WindSurfApi.Models;

namespace WindSurfApi.Services.Interfaces
{
    public interface IMagasinService
    {
        IEnumerable<Magasin> GetMagasins(string agence);
    }
}
