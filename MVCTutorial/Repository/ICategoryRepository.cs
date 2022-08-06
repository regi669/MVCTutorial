using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface ICategoryRepository : IRepository<Category>
{
    void Update(Category category);
}