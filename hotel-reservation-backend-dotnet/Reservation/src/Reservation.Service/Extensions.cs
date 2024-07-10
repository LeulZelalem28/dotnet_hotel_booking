using Reservation.Service.Models;
using Reservation.Service.ReservationDtos;

namespace Reservation.Service;

    public static class Extensions
    {
        public static ReservationDto AsDto(this RoomReservation reservation, string hotelName, string roomType, decimal price)
        {
            return new ReservationDto(
                reservation.Id,
                reservation.HotelId,
                hotelName,
                reservation.RoomTypeId,
                roomType,
                reservation.RoomId,
                reservation.roomNumber,
                price,
                reservation.UserId,
                reservation.GuestName,
                reservation.CheckInDate,
                reservation.CheckOutDate
            );
        }
    }
