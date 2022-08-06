using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface ICoverTypeRepository : IRepository<CoverType>
{
    void Update(CoverType coverType);
}