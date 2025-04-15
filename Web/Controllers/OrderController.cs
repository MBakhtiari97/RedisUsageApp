using Core;
using Microsoft.AspNetCore.Mvc;
using Service.OrderServices;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderServices _orderServices;

    public OrderController(ILogger<OrderController> logger, IOrderServices orderServices)
    {
        _orderServices = orderServices;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        try
        {
            var result = await _orderServices.ReadTempOrder(orderId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveOrder(AddOrderDTO addOrder)
    {
        try
        {
            var result = await _orderServices.SaveTempOrderAsync(addOrder.Order, addOrder.OrderItems);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    [HttpPatch()]
    public async Task<IActionResult> TransferTemporaryOrders()
    {
        try
        {
            var result = await _orderServices.TransferTempOrdersToDatabaseAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}