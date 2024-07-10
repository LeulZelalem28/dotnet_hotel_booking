using Hotel.Service.Models;
using Hotel.Service.HotelDtos;

namespace Hotel.Service
{
    public static class Extensions
    {
        public static HotelDto AsDto(this Hotels hotel, decimal Price)
        {
            return new HotelDto(
                hotel.Id,
                hotel.Name,
                hotel.Image,
                hotel.Address,
                Price,
                hotel.Description
            );
        }
    }
}