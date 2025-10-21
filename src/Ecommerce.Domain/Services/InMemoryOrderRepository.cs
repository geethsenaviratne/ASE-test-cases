using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Services;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<string, Order> _orders = new();

    public void Save(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        _orders[order.OrderId] = order;
    }

    public Order? GetById(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return null;

        return _orders.TryGetValue(orderId, out var order) ? order : null;
    }

    public IReadOnlyCollection<Order> GetAll()
    {
        return _orders.Values.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Order> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return _orders.Values
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToList()
            .AsReadOnly();
    }
}