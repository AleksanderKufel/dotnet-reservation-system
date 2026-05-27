using FluentValidation;
using ReservationSystem.Api.Contracts;

namespace ReservationSystem.API.Validators;

public class CreateReservationRequestValidator
    : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.SpecialistId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime);
    }
}