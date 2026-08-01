using Microsoft.AspNetCore.Mvc;
using PaymentApp.Application.DTOs;
using PaymentApp.Application.Interfaces;
using PaymentApp.Domain.Exceptions;

namespace PaymentApp.Api.Controllers;

[ApiController]
[Route("v1/payment")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService payments, ILogger<PaymentController> logger)
    {
        _payments = payments;
        _logger = logger;
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<TransferResponse>> Transfer(TransferRequest request)
    {
        if (request.Amount > 10_000)
        {
            _logger.LogWarning(
                "Large transfer: user {Payer} -> user {Payee}, amount {Amount}",
                request.PayerUserId, request.PayeeUserId, request.Amount);
        }

        try
        {
            await _payments.TransferAsync(
                request.PayerUserId,
                request.PayeeUserId,
                request.Amount);

            return Ok(new TransferResponse("completed", 0, 0));
        }
        catch (UserNotFoundException ex)
        {
            return NotFound(new { code = ex.Code, message = ex.Message });
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new { code = ex.Code, message = ex.Message });
        }
        catch (InvalidTransferException ex)
        {
            return BadRequest(new { code = ex.Code, message = ex.Message });
        }
    }
}