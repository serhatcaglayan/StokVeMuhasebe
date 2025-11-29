using coreAden.Models;
using PagedList;
using System.Collections.Generic;

namespace coreAden.Core.Interfaces
{
    public interface IMusteriService
    {
        PagedList<Musteriler> GetMusteriler(int page, int pageSize, string q = null);
        Musteriler GetMusteriById(int id);
        void AddMusteri(Musteriler musteri);
        void UpdateMusteri(Musteriler musteri);
        void DeleteMusteri(int id);
        bool MusteriExists(int id);
    }
}
