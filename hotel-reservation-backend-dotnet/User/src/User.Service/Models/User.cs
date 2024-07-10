using System;
using Common;
namespace User.Service.Models{
    public class Person : IModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role {get; set; } = "user";
        public string PhoneNumber { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}