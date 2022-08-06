using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface IProductRepository : IRepository<Product>
{
    void Update(Product product);
}