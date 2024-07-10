using System;
using Common;

namespace Reservation.Service.Models;
public class RoomsType : IModel
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; } 
    public Guid HotelId { get; set; }     
}