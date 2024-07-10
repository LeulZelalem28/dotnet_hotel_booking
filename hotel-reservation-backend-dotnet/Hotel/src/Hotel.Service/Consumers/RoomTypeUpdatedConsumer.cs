using MassTransit;
using System.Threading.Tasks;
using Common;
using RoomType.Contracts;
using Hotel.Service.Models;
namespace Hotel.Service.Consumers;

public class RoomTypeUpdatedConsumer : IConsumer<RoomTypeUpdated>
{
     private readonly IService<RoomsType> _roomTypeService;
    public RoomTypeUpdatedConsumer(IService<RoomsType> roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }
    public async Task Consume(ConsumeContext<RoomTypeUpdated> context)
    {
        var message = context.Message;
        var roomType = await _roomTypeService.GetAsync(message.Id);

        if (roomType == null)
        {
            roomType = new RoomsType
            {
                Id = message.Id,
                Type = message.Type,
                Price = message.Price,
                HotelId = message.HotelId
            };
            await _roomTypeService.CreateAsync(roomType);
        }
        else
        {
                roomType.Type = message.Type;
                roomType.Price = message.Price;
                await _roomTypeService.UpdateAsync(roomType);
        }

       
    }
}