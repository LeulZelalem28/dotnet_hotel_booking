using Room.Service.Models;
using Room.Service.RoomDtos;

namespace Room.Service
{
    public static class Extensions
    {
        public static RoomDto AsDto(this Rooms room, string Name, string hotelName)
        {
            return new RoomDto(
                room.Id,
                room.RoomTypeId,
                Name,
                room.RoomNumber,
                room.HotelId,
                hotelName
            );
        }
        public static RoomTypeDto AsDto(this RoomsType roomsType, string hotelName)
        {
            return new RoomTypeDto(
                roomsType.Id,
                roomsType.Type ?? string.Empty,
                roomsType.Image ?? new byte[0],
                roomsType.Description ?? string.Empty,
                roomsType.Price, 
                roomsType.HotelId,
                hotelName
            );
        }
    }
}