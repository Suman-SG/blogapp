namespace blogapp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System;
    using System.Collections.Generic;



    public enum AccessStatus { None, Pending, Approved, Rejected }
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        public string? ProfileImagePath { get; set; } // Make it nullable


        public DateTime DateTime { get; set; } = DateTime.Now;

        // 🟢 Navigation properties
        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }


        public AccessStatus BlogAccessStatus { get; set; } = AccessStatus.None;
    }
}
