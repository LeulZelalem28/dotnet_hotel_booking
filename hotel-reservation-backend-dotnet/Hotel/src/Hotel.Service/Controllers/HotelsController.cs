using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq; // Added using statement
using Common;
using System.Threading.Tasks;
using Hotel.Service.Models;
using Hotel.Service.HotelDtos;
using Hotel.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Serializers;

namespace Hotel.Service.Controllers;
    [ApiController]
    [Route("hotels")]
    public class HotelsController : ControllerBase
    {
        private readonly IService<Hotels> _hotelService;
        private readonly IService<RoomsType> _roomTypeService;
        ILogger<HotelsController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        public HotelsController(IService<Hotels> hotelService, IService<RoomsType> roomTypeService, IPublishEndpoint publishEndpoint, ILogger<HotelsController> logger){
            _hotelService = hotelService;
            _roomTypeService = roomTypeService;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
    
    [HttpGet()]
    public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotelsAsync()
        {
            // var hotels = (await _hotelService.GetAllAsync()).Select(hotels => hotels.AsDto());
            // return Ok(hotels);

            var hotels = (await _hotelService.GetAllAsync());
            var hotelsDto = new List<HotelDto>();
            foreach(var hotel in hotels)
            {
                var roomTypes = await _roomTypeService.GetAllAsync(roomType => roomType.HotelId == hotel.Id);
                 decimal minPrice = 0;
            //check if the hotel has room types before assigning a value to min price
                if (roomTypes.Any())
                {
                     minPrice = roomTypes.Min(roomType => roomType.Price);
                }
                var hotelDto = hotel.AsDto(minPrice);
                hotelsDto.Add(hotelDto);
            }
            return Ok(hotelsDto);
        }
     [ActionName(nameof(GetHotelByAsync))]
        [HttpGet("{id}")]
        
        public async Task <ActionResult<HotelDto>> GetHotelByAsync(Guid id)
        {
            var hotel = await _hotelService.GetAsync(id);
            //check if hotel exists
            if(hotel == null)
            {
                return NotFound("hotel doesnt exist");
            }
            var roomTypes = await _roomTypeService.GetAllAsync(roomType => roomType.HotelId == hotel.Id);
            //make minprice zero initially the hotel is not created without room types and rooms
            decimal minPrice = 0;
            //check if the hotel has room types before assigning a value to min price
            if (roomTypes.Any())
            {
             minPrice = roomTypes.Min(roomType => roomType.Price);
            }
            return Ok(hotel.AsDto(minPrice));
        }

    [HttpPost()]
        public async Task<ActionResult<HotelDto>> CreateHotelAsync([FromForm]CreateHotelDto createHotelDto)
        {
            if(string.IsNullOrEmpty(createHotelDto.Name) ||
               string.IsNullOrEmpty(createHotelDto.Address) ||
               string.IsNullOrEmpty(createHotelDto.Description))
               {
                return BadRequest("Missing required field.");
               }
            if (createHotelDto.Image == null || createHotelDto.Image.Length == 0)
           {
             return BadRequest("Image is required.");
           }

            byte[] imageBytes = null;
            using (var memoryStream = new MemoryStream())
            {
                await createHotelDto.Image.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // check if the hotel already exists
            var hotelFound = await _hotelService.GetAsync(hotel => hotel.Name == createHotelDto.Name);
            if(hotelFound != null)
            {
                // return conflict if it exists
                return Conflict("Hotel with the same name already exists.");  
            }


            var hotel = new Hotels{
               Id = Guid.NewGuid(), 
               Name = createHotelDto.Name, 
               Image = imageBytes,
               Address = createHotelDto.Address, 
               Description = createHotelDto.Description
               };
               await _hotelService.CreateAsync(hotel);
               
               //publish the hotel created
               await _publishEndpoint.Publish(new HotelCreated(hotel.Id, hotel.Name, hotel.Description));
              return CreatedAtAction(nameof(GetHotelByAsync), new {id =hotel.Id}, hotel);
        }

    [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotelAsync(Guid id, [FromForm]UpdateHotelDto updateHotelDto){
            var existingHotel = await _hotelService.GetAsync(id);
             // checking if hotel exists
            if(existingHotel == null){
                return NotFound();
            } 

            //optional editing
            if (updateHotelDto.Image != null && updateHotelDto.Image.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                 {
                    await updateHotelDto.Image.CopyToAsync(memoryStream);
                    existingHotel.Image = memoryStream.ToArray();
                 }
            }

            if( updateHotelDto.Name != null)
            {
            // check if the hotel already exists
            var hotelFound = await _hotelService.GetAsync(hotel => hotel.Name == updateHotelDto.Name);
            if(hotelFound != null)
            {
                // return conflict if it exists
                return Conflict("Hotel with the same name already exists.");  
            }
                existingHotel.Name = updateHotelDto.Name; 
            }
            if(updateHotelDto.Address != null)
            {
                existingHotel.Address = updateHotelDto.Address;
            }
            if(updateHotelDto.Description != null)
            {
                existingHotel.Description = updateHotelDto.Description;
            }

            await _hotelService.UpdateAsync(existingHotel);
            await _publishEndpoint.Publish(new HotelUpdated(existingHotel.Id, existingHotel.Name, existingHotel.Description));
            return NoContent();
        }  

    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotelAsync(Guid id){
             var hotel = await _hotelService.GetAsync(id);
            if(hotel == null){
                return NotFound();
            }   
             await _hotelService.RemoveAsync(hotel.Id);
             await _publishEndpoint.Publish(new HotelDeleted(hotel.Id));
             return NoContent();
        }    
    }