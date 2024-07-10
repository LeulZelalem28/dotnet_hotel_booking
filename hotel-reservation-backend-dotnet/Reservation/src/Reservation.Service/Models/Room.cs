using System;
using Common;

namespace Reservation.Service.Models;
public class Rooms : IModel
    {
        public Guid Id { get; set; }
        public int RoomNumber { get; set; }    
        public Guid RoomTypeId { get; set; } 
        public Guid HotelId { get; set; }  
    }
