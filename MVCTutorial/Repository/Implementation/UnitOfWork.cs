﻿using MVCTutorial.DataAccess.Data;

namespace MVCTutorial.Repository.Implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    public ICategoryRepository Category { get; }
    
    public ICoverTypeRepository CoverType { get; }

    public IProductRepository Product { get; }

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        Category = new CategoryRepository(_dbContext);
        CoverType = new CoverTypeRepository(_dbContext);
        Product = new ProductRepository(_dbContext);
    }
    
    public void Save()
    {
        _dbContext.SaveChanges();
    }
}