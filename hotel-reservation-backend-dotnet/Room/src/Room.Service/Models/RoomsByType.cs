using System;
using Common;

namespace Room.Service.Models;
public class RoomsType : IModel
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public byte[] Image { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public Guid HotelId { get; set; } 
    public string HotelName {get; set; }       
}