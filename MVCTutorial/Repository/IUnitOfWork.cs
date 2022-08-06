namespace MVCTutorial.Repository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; }
    ICoverTypeRepository CoverType { get; }
    void Save();
}