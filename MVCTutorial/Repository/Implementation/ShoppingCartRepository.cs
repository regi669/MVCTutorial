using System.Linq.Expressions;
using MVCTutorial.Data;
using MVCTutorial.Models;

namespace MVCTutorial.Repository.Implementation;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ShoppingCartRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public int IncrementCount(ShoppingCart cart, int count)
    {
        cart.Count += count;
        return cart.Count;
    }

    public int DecrementCount(ShoppingCart cart, int count)
    {
        cart.Count -= count;
        return cart.Count;
    }
}