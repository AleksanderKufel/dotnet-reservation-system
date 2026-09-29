using Microsoft.AspNetCore.Mvc;
using ReservationSystem.Api.Contracts;
using ReservationSystem.Application.Services;

namespace ReservationSystem.Api.Controllers;

[ApiController]
[Route("api/specialists")]
public class SpecialistsController : ControllerBase
{
    private readonly SpecialistService _specialistService;

    public SpecialistsController(SpecialistService specialistService)
    {
        _specialistService = specialistService;
    }

    [HttpGet]
    public async Task<IReadOnlyList<SpecialistResponse>> GetAll(CancellationToken cancellationToken)
    {
        var specialists = await _specialistService.GetAllAsync(cancellationToken);

        return specialists
            .Select(s => new SpecialistResponse(s.Id, s.Name, s.Specialization))
            .ToList();
    }
}
