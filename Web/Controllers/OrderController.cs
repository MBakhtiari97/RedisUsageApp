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
            throw new Exception(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveOrder(Core.Order order, List<OrderItem> orderItems)
    {
        try
        {
            var result = await _orderServices.SaveTempOrderAsync(order, orderItems);
            return Ok(result);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}