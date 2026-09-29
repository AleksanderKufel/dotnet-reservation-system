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

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.CreateReservationAsync(
            request.SpecialistId,
            UserId,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.Id },
            ReservationResponse.FromDomain(reservation));
    }

    [HttpGet("{id:guid}")]
    public async Task<ReservationResponse> GetById(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.GetForUserAsync(id, UserId, cancellationToken);

        return ReservationResponse.FromDomain(reservation);
    }
}
