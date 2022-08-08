using System.Linq.Expressions;
using MVCTutorial.Data;
using MVCTutorial.Models;

namespace MVCTutorial.Repository.Implementation;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderHeaderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void Update(OrderHeader orderHeader)
    {
        _dbContext.OrderHeaders.Update(orderHeader);
    }

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
        var orderHeaderFromDb = _dbContext.OrderHeaders.FirstOrDefault(o => o.Id == id);
        if (orderHeaderFromDb != null)
        {
            orderHeaderFromDb.OrderStatus = orderStatus;
            if (paymentStatus != null)
            {
                orderHeaderFromDb.PaymentStatus = paymentStatus;
            }
        }
    }
}