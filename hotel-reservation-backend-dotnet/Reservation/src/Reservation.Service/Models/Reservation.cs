using System;
using Common;
namespace Reservation.Service.Models;
public class RoomReservation : IModel
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Guid HotelId { get; set; }
    public string HotelName { get; set; }
    public int roomNumber { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; }
    public Guid RoomTypeId { get; set; }
    public string GuestName { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}