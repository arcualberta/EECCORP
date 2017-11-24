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

    public class Event2
    {
        public string Description { get; set; }
        //public DateTime 
    }
}