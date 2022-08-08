using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface IApplicationUserRepository : IRepository<ApplicationUser>
{
    void Update(ApplicationUser user);
}