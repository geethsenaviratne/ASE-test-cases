namespace Ecommerce.Domain.Interfaces;

public interface IInventoryService
{
    int GetAvailableQuantity(string sku);
    void ReserveStock(string sku, int quantity);
    void ReleaseStock(string sku, int quantity);
}