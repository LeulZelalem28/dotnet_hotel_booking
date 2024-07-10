using System;
namespace RoomType.Contracts;
 
public record RoomTypeCreated(Guid Id, string Type, decimal Price, Guid HotelId);

public record RoomTypeUpdated(Guid Id, string Type, decimal Price, Guid HotelId);

public record RoomTypeDeleted(Guid Id);