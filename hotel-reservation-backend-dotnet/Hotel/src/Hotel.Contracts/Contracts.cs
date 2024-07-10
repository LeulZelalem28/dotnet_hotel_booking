using System;
namespace Hotel.Contracts;
 
public record HotelCreated(Guid HotelId, string Name, string Description);

public record HotelUpdated(Guid HotelId, string Name, string Description);

public record HotelDeleted(Guid HotelId);