using MassTransit;
using System.Threading.Tasks;
using Common;
using Hotel.Contracts;
using Room.Service.Models;
namespace Room.Service.Consumers;

public class HotelUpdatedConsumer : IConsumer<HotelUpdated>
{
     private readonly IService<Hotels> _hotelService;
    public HotelUpdatedConsumer(IService<Hotels> hotelService)
    {
        _hotelService = hotelService;
    }
    public async Task Consume(ConsumeContext<HotelUpdated> context)
    {
        var message = context.Message;
        var hotel = await _hotelService.GetAsync(message.HotelId);

        if (hotel == null)
        {
             hotel = new Hotels
            {
            Id = message.HotelId,
            HotelName = message.Name,
            Description = message.Description
            };
            await _hotelService.CreateAsync(hotel);
        }
        else
        {
            hotel.HotelName = message.Name;
            hotel.Description = message.Description;
            await _hotelService.UpdateAsync(hotel);
        }

       
    }
}