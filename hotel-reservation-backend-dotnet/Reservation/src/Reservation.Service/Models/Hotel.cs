using System;
using Common;

namespace Reservation.Service.Models;

public class Hotels : IModel
{
    public Guid Id { get; set; } 
    public string Name { get; set; } 
    public string Description { get; set; }
}