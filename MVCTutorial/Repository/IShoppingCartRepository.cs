using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    void Update(ShoppingCart cart);
}