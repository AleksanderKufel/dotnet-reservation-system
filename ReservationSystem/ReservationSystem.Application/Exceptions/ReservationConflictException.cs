using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationSystem.Application.Exceptions
{
    public sealed class ReservationConflictException
    : Exception
    {
        public ReservationConflictException(
            string message)
            : base(message)
        {
        }
    }
}
