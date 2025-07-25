﻿using System.ComponentModel.DataAnnotations;

namespace diji_card_alt.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; }   

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
    }
}
