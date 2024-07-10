using MassTransit;
using System.Threading.Tasks;
using Common;
using IndividualRoom.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class IndividualRoomCreatedConsumer : IConsumer<IndividualRoomCreated>
{
     private readonly IService<Rooms> _roomService;
    public IndividualRoomCreatedConsumer(IService<Rooms> roomService)
    {
        _roomService = roomService;
    }
    public async Task Consume(ConsumeContext<IndividualRoomCreated> context)
    {
        var message = context.Message;
        var room = await _roomService.GetAsync(message.Id);

        if (room != null)
        {
            return;
        }

        room = new Rooms
        {
            Id = message.Id,
            RoomNumber = message.RoomNumber,
            RoomTypeId = message.RoomTypeId,
            HotelId = message.HotelId
        };
        await _roomService.CreateAsync(room);
    }
}