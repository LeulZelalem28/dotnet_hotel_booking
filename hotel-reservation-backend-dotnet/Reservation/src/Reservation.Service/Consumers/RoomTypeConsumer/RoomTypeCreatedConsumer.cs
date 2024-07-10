using MassTransit;
using System.Threading.Tasks;
using Common;
using RoomType.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class RoomTypeCreatedConsumer : IConsumer<RoomTypeCreated>
{
     private readonly IService<RoomsType> _roomTypeService;
    public RoomTypeCreatedConsumer(IService<RoomsType> roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }
    public async Task Consume(ConsumeContext<RoomTypeCreated> context)
    {
        var message = context.Message;
        var roomType = await _roomTypeService.GetAsync(message.Id);

        if (roomType != null)
        {
            return;
        }

        roomType = new RoomsType
        {
            Id = message.Id,
            Type = message.Type,
            Price = message.Price,
            HotelId = message.HotelId
        };
        await _roomTypeService.CreateAsync(roomType);
    }
}