using MassTransit;
using System.Threading.Tasks;
using Common;
using Hotel.Contracts;
using Reservation.Service.Models;
namespace Reservation.Service.Consumers;

public class HotelCreatedConsumer : IConsumer<HotelCreated>
{
     private readonly IService<Hotels> _hotelService;
    public HotelCreatedConsumer(IService<Hotels> hotelService)
    {
        _hotelService = hotelService;
    }
    public async Task Consume(ConsumeContext<HotelCreated> context)
    {
        var message = context.Message;
        var hotel = await _hotelService.GetAsync(message.HotelId);

        if (hotel != null)
        {
            return;
        }

        hotel = new Hotels
        {
            Id = message.HotelId,
            Name = message.Name,
            Description = message.Description
        };
        await _hotelService.CreateAsync(hotel);
    }
}