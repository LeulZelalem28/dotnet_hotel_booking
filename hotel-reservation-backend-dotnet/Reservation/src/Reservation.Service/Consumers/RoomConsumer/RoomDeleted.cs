using MassTransit;
using System.Threading.Tasks;
using Common;
using IndividualRoom.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class IndividualRoomDeletedConsumer : IConsumer<IndividualRoomDeleted>
{
    private readonly IService<Rooms> _roomTypeService;
    public IndividualRoomDeletedConsumer(IService<Rooms> roomService)
    {
        _roomTypeService = roomService;
    }
    public async Task Consume(ConsumeContext<IndividualRoomDeleted> context)
    {
        var message = context.Message;
        var room = await _roomTypeService.GetAsync(message.Id);

        if (room == null)
        {
             return;
        }
        await _roomTypeService.RemoveAsync(message.Id);
       
    }
}