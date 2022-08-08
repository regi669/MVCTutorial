namespace MVCTutorial.Repository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }
    ICoverTypeRepository CoverType { get; }
    IProductRepository Product { get; }
    ICompanyRepository Companies { get; }
    IApplicationUserRepository Users { get; }
    IShoppingCartRepository ShoppingCart { get; }
    void Save();
}