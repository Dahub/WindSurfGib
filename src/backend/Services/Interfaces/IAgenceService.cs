using System.Collections.Generic;

namespace WindSurfApi.Services.Interfaces
{
    public interface IAgenceService
    {
        IEnumerable<string> GetAgences();
    }
}
