using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Interfaces;

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(string orderId);
    IReadOnlyCollection<Order> GetAll();
    IReadOnlyCollection<Order> GetByDateRange(DateTime startDate, DateTime endDate);
}