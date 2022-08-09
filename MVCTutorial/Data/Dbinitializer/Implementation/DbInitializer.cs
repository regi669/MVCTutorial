using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCTutorial.Models;
using MVCTutorial.Utility;

namespace MVCTutorial.Data.Dbinitializer.Implementation;

public class DbInitializer : IDbInitializer
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public DbInitializer(UserManager<IdentityUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }
    public void Initialize()
    {
        try
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                _dbContext.Database.Migrate();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        if (!_roleManager.RoleExistsAsync(Util.ROLE_ADMIN).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(Util.ROLE_ADMIN)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(Util.ROLE_EMPLOYEE)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(Util.ROLE_USER_COMP)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(Util.ROLE_USER_INDI)).GetAwaiter().GetResult();

            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "admin@admin.admin",
                Email = "admin@admin.admin",
                Name = "Admin",
                PhoneNumber = "1112223333",
                StreetAdress = "Admin",
                State = "Admin",
                PostalCode = "111222333",
                City = "Admin"
            }, "Admin1!").GetAwaiter().GetResult();
            
            ApplicationUser user = _dbContext.Users.FirstOrDefault(u => u.Email == "admin@admin.admin");
            _userManager.AddToRoleAsync(user, Util.ROLE_ADMIN).GetAwaiter().GetResult();
        }
    }
}