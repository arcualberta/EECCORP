using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECCORP.Models
{
    public class IsEligibleViewModel
    {
        public string IsFoAStudentQuestion { get; set; } = "Are you a student in the Faculty of Arts?";
        public bool IsFoAStudent { get; set; } = false;
        public string IsEnrolledQuestion { get; set; } = "Are you currently enrolled in, or have you previously taken any Economics courses at the UofA?";
        public bool IsEnrolled { get; set; } = false;
    }

    public class Event
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public string Id { get; set; }
        public bool IsSelected { get; set; }
        public int Week { get; set; }
        public ICollection<ApplicationUser> RegisteredUsers;
    }
}