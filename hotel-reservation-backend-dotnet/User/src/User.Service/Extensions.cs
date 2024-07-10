using User.Service.Dtos;
using User.Service.Models;

namespace User.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this Person person)
        {
            return new UserDto(person.Id, person.Username, person.Email, person.Password, person.PhoneNumber, person.CreatedDate);
        }
    }
}
