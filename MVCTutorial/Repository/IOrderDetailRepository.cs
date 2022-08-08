using MVCTutorial.Models;

namespace MVCTutorial.Repository;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    void Update(OrderDetail orderDetail);
}