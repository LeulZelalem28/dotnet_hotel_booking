using MassTransit;
using System.Threading.Tasks;
using Common;
using RoomType.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class RoomTypeDeletedConsumer : IConsumer<RoomTypeDeleted>
{
    private readonly IService<RoomsType> _roomTypeService;
    public RoomTypeDeletedConsumer(IService<RoomsType> roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }
    public async Task Consume(ConsumeContext<RoomTypeDeleted> context)
    {
        var message = context.Message;
        var roomType = await _roomTypeService.GetAsync(message.Id);

        if (roomType == null)
        {
             return;
        }
        await _roomTypeService.RemoveAsync(message.Id);
       
    }
}