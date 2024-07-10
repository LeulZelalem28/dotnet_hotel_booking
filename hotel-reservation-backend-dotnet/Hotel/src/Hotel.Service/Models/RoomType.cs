using System;
using Common;

namespace Hotel.Service.Models;
public class RoomsType : IModel
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; } 
    public Guid HotelId { get; set; }     
}