using System.ComponentModel.DataAnnotations;

namespace ReservationSystem.Infrastructure.PublicHolidays;

public class PublicHolidaysOptions
{
    public const string SectionName = "PublicHolidays";

    [Required, Url]
    public string BaseUrl { get; set; } = default!;

    [Required, StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; set; } = default!;
}
