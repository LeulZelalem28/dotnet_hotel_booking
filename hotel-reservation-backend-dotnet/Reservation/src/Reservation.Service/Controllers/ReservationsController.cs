using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; // Added using statement
using Common;

using System.Threading.Tasks;
using Reservation.Service.Models;
using Reservation.Service.ReservationDtos;
using Hotel.Contracts;

using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Reservation.Service.Controllers;
[ApiController]
[Route("Reservation")]
public class ReservationsController : ControllerBase
{
    private readonly IService<Hotels> _hotelService;
    private readonly IService<RoomsType> _roomTypeService;
    private readonly IService<RoomReservation> _roomReservationService;
    private readonly IService<Rooms> _roomService;
    ILogger<ReservationsController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    public ReservationsController(IService<RoomReservation> roomReservationService, IService<Hotels> hotelService, IService<RoomsType> roomTypeService, IService<Rooms> roomService, IPublishEndpoint publishEndpoint, ILogger<ReservationsController> logger)
    {
        _roomReservationService = roomReservationService;
        _hotelService = hotelService;
        _roomTypeService = roomTypeService;
        _roomService = roomService;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservationsAsync()
    {
        var reservations = await _roomReservationService.GetAllAsync();
        //check if null
        if (reservations == null)
        {
            return NotFound("no reservations"); // Or return an appropriate response for empty reservations
        }
        var roomTypes = await _roomTypeService.GetAllAsync();
        var hotels = await _hotelService.GetAllAsync();
        var reservationDtos = reservations.Select(reservation =>
        {
            var roomType = roomTypes.SingleOrDefault(roomType => roomType.Id == reservation.RoomTypeId);
            var hotel = hotels.SingleOrDefault(hotel => hotel.Id == reservation.HotelId);
            return reservation.AsDto(hotel.Name, roomType.Type, roomType.Price);
        }
        );
        return Ok(reservationDtos);
    }
    [ActionName(nameof(GetReservationAsync))]
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetReservationAsync(Guid id)
    {
        //Get the reservation with that id
        var reservation = await _roomReservationService.GetAsync(id);
        //Check if the reservation exists
        if (reservation == null)
        {
             return NotFound("Your reservation cannot be found");
        }
        //Get the corresponding room type from the reservation
        var roomType = await _roomTypeService.GetAsync(reservation.RoomTypeId);
        //Get the corresponding hotel from the reservation
        var hotel = await _hotelService.GetAsync(reservation.HotelId);
        if (reservation == null)
        {
            return NotFound("Your reservation can not be found");
        }
        return reservation.AsDto(hotel.Name, roomType.Type, roomType.Price);
    }

    [HttpPost()]
    public async Task<ActionResult<ReservationDto>> MakeReservationAsync([FromForm] CreateReservationDto createReservationDto)
    {
        //check if the dates are entered
        if (createReservationDto.CheckInDate == DateTime.MinValue || createReservationDto.CheckOutDate == DateTime.MinValue)
        {
            return BadRequest("Check-in and check-out dates are required.");
        }

        //check if the dates have not passed
        if (createReservationDto.CheckInDate <= DateTime.Now)
        {
            return BadRequest("Check-in date must be in the future.");
        }

        //check if the check in date is after the checkout date
        if (createReservationDto.CheckOutDate <= createReservationDto.CheckInDate)
        {
            return BadRequest("Check-out date must be after the check-in date.");
        }


        //check if room is available

        //Get the room type
        var roomType = await _roomTypeService.GetAsync(roomType => roomType.Id == createReservationDto.RoomTypeId);

        //check unavailable rooms by filtering through the reservations and get the unavailable rooms
        var unavailableRoomNumbers = (await _roomReservationService.GetAllAsync(reservation =>
            reservation.RoomTypeId == roomType.Id && createReservationDto.CheckInDate >= reservation.CheckInDate && createReservationDto.CheckOutDate <= reservation.CheckOutDate

            )).Select(reservation => reservation.roomNumber).ToList(); ;

        //Get the available rooms
        var availableRooms = await _roomService.GetAllAsync(room => !unavailableRoomNumbers.Contains(room.RoomNumber));

        //pick a random room
        var random = new Random();
        var roomIdToBeReserved = availableRooms.OrderBy(_ => random.Next()).FirstOrDefault()?.Id ?? Guid.Empty;

        //if no rooms are found return all rooms are reserved
        if (roomIdToBeReserved == null)
        {
            return BadRequest("All rooms are currently reserved.");
        }

        //Get the user info from jwt
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        var userClaims = identity.Claims;

        var reservation = new RoomReservation
        {
            Id = Guid.NewGuid(),
            HotelId = createReservationDto.HotelId,
            HotelName = createReservationDto.HotelName,
            //parse the string to a guid because it is a string
            UserId = userClaims.FirstOrDefault(o => o.Type == "UserId") != null ? Guid.Parse(userClaims.FirstOrDefault(o => o.Type == "UserId")?.Value) : Guid.Empty,
            RoomTypeId = createReservationDto.RoomTypeId,
            RoomId = roomIdToBeReserved,
            GuestName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
            CheckInDate = createReservationDto.CheckInDate,
            CheckOutDate = createReservationDto.CheckOutDate
        };
        await _roomReservationService.CreateAsync(reservation);
        return CreatedAtAction(nameof(GetReservationAsync), new { id = reservation.Id }, reservation);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelReservationAsync(Guid id)
    {
        var reservation = await _roomReservationService.GetAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }
        await _roomReservationService.RemoveAsync(reservation.Id);
        return NoContent();
    }
}