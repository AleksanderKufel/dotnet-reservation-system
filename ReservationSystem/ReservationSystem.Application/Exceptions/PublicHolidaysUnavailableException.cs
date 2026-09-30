namespace ReservationSystem.Application.Exceptions;

public sealed class PublicHolidaysUnavailableException : Exception
{
    public PublicHolidaysUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
