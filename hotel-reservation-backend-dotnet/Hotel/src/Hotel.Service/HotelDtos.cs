using System;
namespace Hotel.Service.HotelDtos;

public record HotelDto(Guid Id, string Name, byte[] Image, string Address, decimal Price, string Description);

public record CreateHotelDto(string Name, IFormFile Image, string Address, string Description);

public record UpdateHotelDto(string? Name, IFormFile? Image, string? Address, string? Description);
