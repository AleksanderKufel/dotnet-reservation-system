using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Application.Services;
using System.Security.Claims;

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
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _reservationService.CreateReservationAsync(
            request.SpecialistId,
            userId,
            request.StartTime,
            request.EndTime,
            cancellationToken);
        return Created(string.Empty, null);
    }
}