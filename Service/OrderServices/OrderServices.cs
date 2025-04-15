using Core;
using DataLayer;
using StackExchange.Redis;
using System.Text.Json;

namespace Service.OrderServices;

public interface IOrderServices
{
    Task<long> SaveTempOrderAsync(Core.Order order, List<OrderItem> orderItems);
    Task<List<long>> TransferTempOrdersToDatabaseAsync();
    Task<Core.Order> ReadTempOrder(int orderId);
    Task<List<AddOrderDTO>> ReadTempOrdersAsync();
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

    public async Task<List<AddOrderDTO>> ReadTempOrdersAsync()
    {
        var tempOrders = new List<AddOrderDTO>();

        // Use SCAN to find all keys matching the prefix
        var cursor = 0;
        do
        {
            var result = await _redisDb.ExecuteAsync("SCAN", cursor, "MATCH", "tempOrder:*", "COUNT", 10000);
            var scannedData = (RedisResult[])result!;

            cursor = (int)scannedData[0]; // Update the cursor
            var keys = (RedisResult[])scannedData[1]!;

            // Fetch the data for each matching key
            foreach (var key in keys)
            {
                var cachedValue = await _redisDb.StringGetAsync((string)key!);
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var tempOrder = JsonSerializer.Deserialize<AddOrderDTO>(cachedValue!, options);
                    if (tempOrder != null)
                        tempOrders.Add(tempOrder);
                }
            }
        }
        while (cursor != 0);
        return tempOrders;
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

    public async Task<List<long>> TransferTempOrdersToDatabaseAsync()
    {
        var orders = await ReadTempOrdersAsync();
        orders.ForEach(o =>
        {
            o.Order.OrderItems = o.OrderItems;
            o.Order.OrderId = default;
            o.OrderItems.ForEach(i =>
            {
                i.OrderId = default;
                i.OrderItemId = default;
            });
        });
        await _masterContext.Order.AddRangeAsync(orders.Select(o => o.Order));
        await _masterContext.SaveChangesAsync();
        return orders.Select(o => o.Order.OrderId).ToList();
    }
}