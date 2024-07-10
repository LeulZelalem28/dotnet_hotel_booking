using System;
using Common;

namespace Room.Service.Models;

public class Hotels : IModel
{
    public Guid Id { get; set; } 
    public string HotelName { get; set; } 
    public string Description { get; set; }
}