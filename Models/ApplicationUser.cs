using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Sunday.IdentityServer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<int>
    {
        public string LoginName { get; set; }

        public string RealName { get; set; }

        public int Sex { get; set; } = 0;

        public int Age { get; set; }

        public DateTime Birth { get; set; } = DateTime.Now;

        public string Addr { get; set; }

        public string FirstQuestion { get; set; }

        public string SecondQuestion { get; set; }

        public bool TdIsDelete { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}