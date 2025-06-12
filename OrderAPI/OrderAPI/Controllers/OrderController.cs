
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderAPI.Models;


[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest order)
    {
        if (order == null)
            return BadRequest("order details are required");

        var response = await _orderService.CreateOrderAsync(order);
        return Ok(response);
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
            return BadRequest("OrderId is required.");

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound("Order not found.");

        return Ok(order);
    }
}
