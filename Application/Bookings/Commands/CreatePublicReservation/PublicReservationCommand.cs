using Contracts.Bookings;

namespace Application.Bookings.Commands.CreatePublicReservation;

public sealed record PublicReservationCommand(PublicReservationRequest Request);
