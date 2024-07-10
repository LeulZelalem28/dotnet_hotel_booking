using System;
namespace IndividualRoom.Contracts;

public record IndividualRoomCreated(Guid Id, Guid RoomTypeId, int RoomNumber, Guid HotelId);

public record IndividualRoomUpdated(Guid Id, Guid RoomTypeId, int RoomNumber);

public record IndividualRoomDeleted(Guid Id);
