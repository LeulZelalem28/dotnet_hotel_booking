using MassTransit;
using System.Threading.Tasks;
using Common;
using IndividualRoom.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class IndividualRoomUpdatedConsumer : IConsumer<IndividualRoomUpdated>
{
     private readonly IService<Rooms> _roomService;
    public IndividualRoomUpdatedConsumer(IService<Rooms> roomService)
    {
        _roomService = roomService;
    }
    public async Task Consume(ConsumeContext<IndividualRoomUpdated> context)
    {
        var message = context.Message;
        var room = await _roomService.GetAsync(message.Id);

        if (room == null)
        {
            room = new Rooms
            {
                Id = message.Id,
                RoomNumber = message.RoomNumber,
                RoomTypeId = message.RoomTypeId
            };
            await _roomService.CreateAsync(room);
        }
        else
        {
                room.RoomNumber = message.RoomNumber;
                await _roomService.UpdateAsync(room);
        }

       
    }
}