using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; // Added using statement
using Common;
using System.Threading.Tasks;
using Room.Service.Models;
using Room.Service.RoomDtos;
using Room.Service;
using RoomType.Contracts;
using IndividualRoom.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Serializers;

namespace Hotel.Service.Controllers;
[ApiController]
[Route("rooms")]
public class RoomsController : ControllerBase
{
    private readonly IService<Rooms> _roomService;
    private readonly IService<RoomsType> _roomTypeService;
    private readonly IService<Hotels> _hotelService;
    private readonly IPublishEndpoint _publishEndpoint;
    ILogger<RoomsController> _logger;
    public RoomsController(IService<Rooms> roomService, IService<RoomsType> roomTypeService, IService<Hotels> hotelService, IPublishEndpoint publishEndpoint, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _roomTypeService = roomTypeService;
        _hotelService = hotelService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetRoomsAsync()
    {
        var rooms = (await _roomService.GetAllAsync());
        var roomTypes = await _roomTypeService.GetAllAsync();
        var hotels = await _hotelService.GetAllAsync();
        var roomsDtos = rooms.Select(room =>
        {         
        var roomType = roomTypes.SingleOrDefault(roomType => roomType.Id == room.RoomTypeId);
        var associatedHotel = hotels.SingleOrDefault(hotel => hotel.Id == roomType.HotelId);
         
        if (roomType == null || associatedHotel == null)
        {
            return room.AsDto("Unknown", "Unknown");
        }
        return room.AsDto(roomType.Type, associatedHotel.HotelName);
        });      
        return Ok(roomsDtos);
    }
    [ActionName(nameof(GetRoomByIdAsync))]
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> GetRoomByIdAsync(Guid id)
    {
        //get the room of id
        var room = await _roomService.GetAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        //get the room type of that id
        var roomType = await _roomTypeService.GetAsync(room.RoomTypeId);
        //get the hotel of that room type so that u can get name
        var hotels = await _hotelService.GetAllAsync(); 
        var associatedHotel  = hotels.SingleOrDefault(hotel => hotel.Id == roomType.HotelId);
        //pass the room type as parameter to the dto
        return Ok(room.AsDto(roomType.Type, associatedHotel.HotelName));
    }


    [HttpGet("type")]
    public async Task<ActionResult<IEnumerable<RoomTypeDto>>> GetRoomTypesAsync()
    {
        var roomsTypes = await _roomTypeService.GetAllAsync();
        var hotels = await _hotelService.GetAllAsync(); 
        var roomsTypeDtos = roomsTypes.Select(roomType =>
        {
            var associatedHotel  = hotels.SingleOrDefault(hotel => hotel.Id == roomType.HotelId);
            return roomType.AsDto(associatedHotel.HotelName);
        }
        );
        return Ok(roomsTypeDtos);
    }
    [ActionName(nameof(GetRoomTypeByIdAsync))]
    [HttpGet("types/{id}")]
    public async Task<ActionResult<RoomTypeDto>> GetRoomTypeByIdAsync(Guid id)
    {
        var roomType = await _roomTypeService.GetAsync(id);
        if (roomType == null)
        {
            return NotFound();
        }
        //get the hotel which corresponds to the roomtype
        var hotel = await _hotelService.GetAsync(roomType.HotelId);
        return Ok(roomType.AsDto(hotel.HotelName));
    }
    [HttpPost("type")]
    public async Task<ActionResult<RoomTypeDto>> CreateRoomTypeAsync([FromForm] CreateRoomTypeDto createRoomTypeDto)
    {
        if (string.IsNullOrEmpty(createRoomTypeDto.Type))
        {
            return BadRequest("Type is required.");
        }
        else if (createRoomTypeDto.Price == 0)
        {
            return BadRequest("Price must be greater than 0.");
        }
        else if (string.IsNullOrEmpty(createRoomTypeDto.Description))
        {
            return BadRequest("Description is required.");
        }
        else if (createRoomTypeDto.HotelId == Guid.Empty)
        {
            return BadRequest("HotelId is required.");
        }
        if (createRoomTypeDto.Image == null || createRoomTypeDto.Image.Length == 0)
        {

            return BadRequest("Image is required.");
        }

        byte[] imageBytes = null;
        if (createRoomTypeDto.Image != null && createRoomTypeDto.Image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await createRoomTypeDto.Image.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }
        }
        // check if room type exists
        var foundRoomType = await _roomTypeService.GetAsync(room => room.Type == createRoomTypeDto.Type);
        if (foundRoomType != null)
        {
            return Conflict("room type already exists");
        }

        var roomTypeGuid = Guid.NewGuid(); //create a guid or the roomtype Id cause it is used twice
                                           //create room type
        var roomType = new RoomsType
        {
            Id = roomTypeGuid,
            Type = createRoomTypeDto.Type,
            Image = imageBytes,
            Description = createRoomTypeDto.Description,
            Price = createRoomTypeDto.Price,
            HotelId = createRoomTypeDto.HotelId
        };
        await _roomTypeService.CreateAsync(roomType);
        await _publishEndpoint.Publish(new RoomTypeCreated(roomType.Id, roomType.Type, roomType.Price, roomType.HotelId));

        // optional functionality to insert a quantity of rooms to create
        if (createRoomTypeDto.Quantity != 0)
        {
            if (createRoomTypeDto.startingRoomNum == 0)
            {
                return BadRequest("Please insert starting room number.");
            }


            //find a better way to do this later
            // Loop through to create the rooms 
            for (int i = 0; i < createRoomTypeDto.Quantity; i++)
            {
                var room = new Rooms
                {
                    Id = Guid.NewGuid(),
                    RoomTypeId = roomTypeGuid,
                    RoomNumber = createRoomTypeDto.startingRoomNum.Value + i,
                    HotelId = createRoomTypeDto.HotelId
                };
                //create the rooms
                await _roomService.CreateAsync(room);

                //publish using rabbitmq so that it can be accessed by other microservi  ce          
                await _publishEndpoint.Publish(new IndividualRoomCreated(room.Id, room.RoomTypeId, room.RoomNumber, room.HotelId));
            }
        }
         //get the hotel of that room type so that u can get name
        var hotel = await _hotelService.GetAsync(roomType.HotelId);

        return CreatedAtAction(nameof(GetRoomTypeByIdAsync), new { id = roomType.Id }, roomType.AsDto(hotel.HotelName));
    }

    [HttpPost()]
    public async Task<ActionResult<RoomDto>> CreateRoomAsync(IFormFile imageFile, [FromForm] CreateRoomDto createRoomDto)
    {
// check if the fields have value
      if (createRoomDto.RoomTypeId == Guid.Empty)
    {
        return BadRequest("RoomTypeId is required.");
    }

    if (createRoomDto.RoomNumber == 0)
    {
        return BadRequest("RoomNumber is required.");
    }

    if (createRoomDto.HotelId == Guid.Empty)
    {
        return BadRequest("HotelId is required.");
    }

        // Check if the provided roomTypeId exists
        var foundRoomTypeId = await _roomTypeService.GetAsync(roomType => roomType.Id == createRoomDto.RoomTypeId);
        if (foundRoomTypeId == null)
        {
            return NotFound("The room type does not exist.");
        }

        // Check if the provided hotelId exists
        var foundHotelId = await _hotelService.GetAsync(hotel => hotel.Id == createRoomDto.HotelId);
        if (foundHotelId == null)
        {
            return NotFound("The hotel does not exist.");
        }

        //check if room number exists first
        var foundRoomNumber = await _roomService.GetAsync(room => room.RoomNumber == createRoomDto.RoomNumber);
        if (foundRoomNumber != null)
        {
            return Conflict("room number already exists");
        }

        var room = new Rooms
        {
            Id = Guid.NewGuid(),
            RoomTypeId = createRoomDto.RoomTypeId,
            RoomNumber = createRoomDto.RoomNumber,
            HotelId = createRoomDto.HotelId
        };


        //get room types
        var roomType = await _roomTypeService.GetAsync(room.RoomTypeId);
        //get the hotel of that room type so that u can get name
        var hotel = await _hotelService.GetAsync(room.HotelId); 
       


        //create the hotel in database
        await _roomService.CreateAsync(room);

    
        //publish using rabbitmq so that it can be accessed by other microservices 
        await _publishEndpoint.Publish(new IndividualRoomCreated(room.Id, room.RoomTypeId, room.RoomNumber, room.HotelId));
        return CreatedAtAction(nameof(GetRoomByIdAsync), new { id = room.Id }, room.AsDto(roomType.Type, hotel.HotelName));
    }
    [HttpPut("type/{id}")]
    public async Task<IActionResult> UpdateRoomTypeAsync(Guid id, [FromForm] UpdateRoomTypeDto updateRoomTypeDto)
    {
        var existingRoomType = await _roomTypeService.GetAsync(id);
        // checking if room type exists exists
        if (existingRoomType == null)
        {
            return NotFound();
        }

        //optional editing
        if (updateRoomTypeDto.Image != null && updateRoomTypeDto.Image.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await updateRoomTypeDto.Image.CopyToAsync(memoryStream);
                existingRoomType.Image = memoryStream.ToArray();
            }
        }
        if (updateRoomTypeDto.Type != null)
        {
            // check if room type of the same name exists
            var foundRoomType = await _roomTypeService.GetAsync(room => room.Type == updateRoomTypeDto.Type);
            if (foundRoomType != null)
            {
                return Conflict("room type already exists");
            }
            existingRoomType.Type = updateRoomTypeDto.Type;
        }
        if (updateRoomTypeDto.Description != null)
        {
            existingRoomType.Description = updateRoomTypeDto.Description;
        }
        if (updateRoomTypeDto.Price != null)
        {
            existingRoomType.Price = updateRoomTypeDto.Price.Value;
        }

        await _roomTypeService.UpdateAsync(existingRoomType);
        //publish 
        await _publishEndpoint.Publish(new RoomTypeUpdated(existingRoomType.Id, existingRoomType.Type, existingRoomType.Price, existingRoomType.HotelId));
        return NoContent();
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoomAsync(Guid id, [FromForm] UpdateRoomDto updateRoomDto)
    {
        var existingRoom = await _roomService.GetAsync(id);
        // checking if room exists
        if (existingRoom == null)
        {
            return NotFound();
        }

        //optional editing
        if (updateRoomDto.RoomNumber != null)
        {
            var foundRoom = await _roomService.GetAsync(room => room.RoomNumber == updateRoomDto.RoomNumber);
            if (foundRoom != null)
            {
                return Conflict("room number already exists");
            }
            existingRoom.RoomNumber = updateRoomDto.RoomNumber ?? existingRoom.RoomNumber;;
        }
        await _roomService.UpdateAsync(existingRoom);
        await _publishEndpoint.Publish(new IndividualRoomUpdated(existingRoom.Id, existingRoom.RoomTypeId, existingRoom.RoomNumber));
        return NoContent();
    }
    [HttpDelete("type/{id}")]
    public async Task<IActionResult> DeleteRoomTypeAsync(Guid id)
    {
        var roomType = await _roomTypeService.GetAsync(id);
        if (roomType == null)
        {
            return NotFound();
        }
        await _roomTypeService.RemoveAsync(roomType.Id);
        //publish
        await _publishEndpoint.Publish(new RoomTypeDeleted(roomType.Id));
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoomAsync(Guid id)
    {
        var room = await _roomService.GetAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        await _roomService.RemoveAsync(room.Id);
        //publish 
        await _publishEndpoint.Publish(new IndividualRoomDeleted(room.Id));
        return NoContent();
    }
}