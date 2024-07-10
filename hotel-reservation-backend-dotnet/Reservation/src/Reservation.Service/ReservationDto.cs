using System;

namespace Reservation.Service.ReservationDtos
{
    public record ReservationDto(Guid Id, Guid HotelId, string HotelName, Guid RoomTypeId, string Type, Guid RoomId, int RoomNumber, decimal Price, Guid UserId, string GuestName, DateTime CheckInDate, DateTime CheckOutDate);

    public record CreateReservationDto(Guid HotelId, string HotelName, Guid RoomTypeId, string Type, Guid RoomId, DateTime CheckInDate, DateTime CheckOutDate);

}