using System;
using Common;
namespace Room.Service.Models;
    public class Rooms : IModel
    {
        public Guid Id { get; set; }
        public Guid RoomTypeId { get; set; }
        public int RoomNumber { get; set; } 
        public Guid HotelId { get; set; }       
    }
