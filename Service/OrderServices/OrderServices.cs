using Core;
using DataLayer;
using StackExchange.Redis;
using System.Text.Json;

namespace Service.OrderServices;

public interface IOrderServices
{
    Task<long> SaveTempOrderAsync(Core.Order order, List<OrderItem> orderItems);
    Task<Core.Order> ReadTempOrder(int orderId);
}

public class OrderServices : IOrderServices
{
    private readonly MasterDbContext _masterContext;
    private readonly IDatabase _redisDb;

    public OrderServices(IConnectionMultiplexer redis, MasterDbContext masterContext)
    {
        _redisDb = redis.GetDatabase();
        _masterContext = masterContext;
    }

    public async Task<Core.Order> ReadTempOrder(int orderId)
    {
        var cacheKey = $"tempOrder:{orderId}";
        var cachedValue = await _redisDb.StringGetAsync(cacheKey);

        if (string.IsNullOrEmpty(cachedValue))
        {
            throw new KeyNotFoundException("Data not found in Redis.");
        }

        // Deserialize the data
        var cachedData = JsonSerializer.Deserialize<dynamic>(cachedValue);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var cachedOrderData = JsonSerializer.Deserialize<AddOrderDTO>(cachedValue, options);

        var order = cachedOrderData.Order;
        var orderItems = cachedOrderData.OrderItems;

        return order;
    }

    public async Task<long> SaveTempOrderAsync(Core.Order order, List<OrderItem> orderItems)
    {
        var id = await _redisDb.StringIncrementAsync("OrderIdCounter");
        order.OrderId = id;

        foreach (var orderItem in orderItems)
        {
            orderItem.OrderId = order.OrderId;
            orderItem.OrderItemId = await _redisDb.StringIncrementAsync("OrderItemIdCounter");
        }

        // Create a composite JSON object for the order and its items
        var cacheKey = $"tempOrder:{order.OrderId}";
        var cacheValue = JsonSerializer.Serialize(new { order, orderItems });

        // Save the JSON data in Redis
        var isSaved = await _redisDb.StringSetAsync(cacheKey, cacheValue, TimeSpan.FromHours(1)); // Expire in 1 hour
        return order.OrderId; // Return whether the operation succeeded
    }
}