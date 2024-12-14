using System.Collections.Generic;

namespace WindSurfApi.Models
{
    public class Magasin
    {
        public string Code { get; set; }
        public string Nom { get; set; }
    }

    public class MagasinComparer : IEqualityComparer<Magasin>
    {
        public bool Equals(Magasin x, Magasin y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Code == y.Code;
        }

        public int GetHashCode(Magasin obj)
        {
            return obj.Code != null ? obj.Code.GetHashCode() : 0;
        }
    }
}
