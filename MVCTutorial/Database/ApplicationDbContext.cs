﻿using Microsoft.EntityFrameworkCore;
using MVCTutorial.Entities;

namespace MVCTutorial.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<Category> Categories { get; set; }
}