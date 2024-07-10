using System;
namespace User.Service.Dtos;

public record UserDto(Guid Id, string Username, string Email, string PhoneNumber, string Password, DateTimeOffset CreatedDate);

public record CreateUserDto(string Username, string Email, string Password, string PhoneNumber);

public record UpdateUserDto(string Username, string Email, string Password, string PhoneNumber);
public record LoginDto(string Username, string Password);