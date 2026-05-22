using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Application.Services;

namespace ReservationSystem.Api.Controllers;

[Authorize]
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
    public async Task<IActionResult> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {

        await _reservationService.CreateReservationAsync(
            request.SpecialistId,
            request.UserId,
            request.StartTime,
            request.EndTime,
            cancellationToken);
        return Created(string.Empty, null);
    }
}