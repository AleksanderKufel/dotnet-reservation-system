using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Application.Services;

namespace ReservationSystem.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequest request)
    {
        try
        {
            await _reservationService.CreateReservationAsync(
                request.SpecialistId,
                request.UserId,
                request.StartTime,
                request.EndTime);

            return Created(string.Empty, null);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}