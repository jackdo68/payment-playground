using Microsoft.AspNetCore.Mvc;
using PaymentApp.Models;
using PaymentApp.Services;
using PaymentApp.Services.Payment;


namespace PaymentApp.Controllers;

[ApiController]
[Route("v1/payments")]
public class PaymentsController(IPaymentService payments) : ControllerBase
{
    private readonly IPaymentService _payments = payments;

    [HttpPost("transfer")]
    public async Task<ActionResult> Transfer(TransferRequest request)
    {
        try
        {
            await _payments.TransferAsync(request.PayerId, request.PayeeId, request.Amount);
            return Ok(new { status = "completed" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}