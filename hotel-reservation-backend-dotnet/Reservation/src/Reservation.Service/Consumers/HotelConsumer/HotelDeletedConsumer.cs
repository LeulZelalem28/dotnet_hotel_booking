using MassTransit;
using System.Threading.Tasks;
using Common;
using Hotel.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class HotelDeletedConsumer : IConsumer<HotelDeleted>
{
     private readonly IService<Hotels> _hotelService;
     private readonly IService<Rooms> _roomService;
        private readonly IService<RoomsType> _roomTypeService;
    public HotelDeletedConsumer(IService<Hotels> hotelService, IService<Rooms> roomService, IService<RoomsType> roomTypeService)
    {
        _hotelService = hotelService;
        _roomService = roomService;
        _roomTypeService = roomTypeService;
    }
    public async Task Consume(ConsumeContext<HotelDeleted> context)
    {
        var message = context.Message;
        var hotel = await _hotelService.GetAsync(message.HotelId);

        if (hotel == null)
        {
             return;
        }
        await _hotelService.RemoveAsync(message.HotelId);

        //delete all the room types of the hotel
        var roomTypes = await _roomTypeService.GetAllAsync(roomType => roomType.HotelId == message.HotelId);
        foreach (var roomType in roomTypes)
        {
            await _roomTypeService.RemoveAsync(roomType.Id);
        }

        //delete all the rooms of the hotel
        var rooms = await _roomService.GetAllAsync(room => room.HotelId == message.HotelId);
        foreach (var room in rooms)
        {
            await _roomService.RemoveAsync(room.Id);
        }
       
    }
}