using FluentValidation;
using ReservationSystem.Api.Contracts;

namespace ReservationSystem.API.Validators;

public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public const int MaxPageSize = 50;

    public PaginationQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize);
    }
}
