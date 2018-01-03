using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECCORP.Models
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public int RegistrationCount { get; set; }
        public bool IsAdmin { get; set; }
        public bool Delete { get; set; }
        public ICollection<Models.Event> Events { get; set; }

        public UserDetailsViewModel(
            string id, 
            string name, 
            string imageUrl,
            int registrationCount,
            bool isAdmin,
            string email,
            List<Models.Event> events
            )
        {
            this.Id = id;
            Name = name;
            ImageUrl = imageUrl;
            RegistrationCount = registrationCount;
            IsAdmin = isAdmin;
            Email = email;
            Events = events;            
            Delete = false;
        }
    }
}