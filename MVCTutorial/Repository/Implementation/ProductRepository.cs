using MVCTutorial.Data;
using MVCTutorial.Models;

namespace MVCTutorial.Repository.Implementation;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public void Update(Product product)
    {
        var productFromDb = _dbContext.Products.FirstOrDefault(p => p.Id == product.Id);
        if (productFromDb is not null)
        {
            productFromDb.Title = product.Title;
            productFromDb.Description = product.Description;
            productFromDb.ISBN = product.ISBN;
            productFromDb.Author = product.Author;
            productFromDb.ListPrice = product.ListPrice;
            productFromDb.Price = product.Price;
            productFromDb.Price50 = product.Price50;
            productFromDb.Price100 = product.Price100;
            productFromDb.CategoryId = product.CategoryId;
            productFromDb.CoverTypeId = product.CoverTypeId;
            if (product.ImageUrl != null)
            {
                productFromDb.ImageUrl = product.ImageUrl;
            }
        }
    }
}