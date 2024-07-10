using System;
namespace Room.Service.RoomDtos;

// for the rooms
public record RoomDto(Guid RoomId, Guid RoomTypeId, string Type, int RoomNumber, Guid HotelId, string hotelName);

public record CreateRoomDto(Guid RoomTypeId, int RoomNumber, Guid HotelId);

public record UpdateRoomDto(int? RoomNumber);


// for the room types
public record RoomTypeDto(Guid Id, string Type, byte[] Image, string Description, decimal Price, Guid HotelId, string HotelName);

public record CreateRoomTypeDto(string Type, IFormFile Image, string Description, decimal Price, Guid HotelId, int? Quantity, int? startingRoomNum);

public record UpdateRoomTypeDto(string? Type, string? Description, decimal? Price,  IFormFile? Image);