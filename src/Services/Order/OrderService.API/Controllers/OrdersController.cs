using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Orders.Commands.CancelOrder;
using OrderService.Application.Orders.Commands.CreateOrder;
using OrderService.Application.Orders.Queries.GetOrderById;
using OrderService.Application.Orders.Queries.GetOrders;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    // POST api/v1/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
    {
        var orderId = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, new { orderId });

    }

    // GET api/v1/orders
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetOrdersQuery(customerId, page, pageSize), ct);
        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id), ct);
        return Ok(order);

    }

    // DELETE api/v1/orders/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken ct)
    {
        await sender.Send(new CancelOrderCommand(id, request.Reason), ct);
        return NoContent();
    }

}

public sealed record CancelOrderRequest(string Reason);
