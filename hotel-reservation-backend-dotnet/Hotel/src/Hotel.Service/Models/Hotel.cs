using System;
using Common;
namespace Hotel.Service.Models;
    public class Hotels : IModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte[] Image { get; set; } 
        public string Address { get; set; }
        public decimal Price { get; set; }
        public string Description {get; set;}
    }
