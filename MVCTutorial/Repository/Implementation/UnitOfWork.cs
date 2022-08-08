using MVCTutorial.Data;

namespace MVCTutorial.Repository.Implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    public ICategoryRepository Category { get; }
    
    public ICoverTypeRepository CoverType { get; }

    public IProductRepository Product { get; }
    
    public ICompanyRepository Companies { get; }
    public IApplicationUserRepository Users { get; }
    public IShoppingCartRepository ShoppingCart { get; }
    public IOrderHeaderRepository OrderHeader { get; }
    public IOrderDetailRepository OrderDetail { get; }
    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        Category = new CategoryRepository(_dbContext);
        CoverType = new CoverTypeRepository(_dbContext);
        Product = new ProductRepository(_dbContext);
        Companies = new CompanyRepository(_dbContext);
        Users = new ApplicationUserRepository(_dbContext);
        ShoppingCart = new ShoppingCartRepository(_dbContext);
        OrderHeader = new OrderHeaderRepository(_dbContext);
        OrderDetail = new OrderDetailRepository(_dbContext);
    }
    
    public void Save()
    {
        _dbContext.SaveChanges();
    }
}