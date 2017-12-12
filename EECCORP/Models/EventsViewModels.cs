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

        private string _Summary = "";
        private string _Description = "";


        public string Summary {
            get { return _Summary == null ? "" : _Summary; }
            set { _Summary = value; }
        }
        public string Description {
            get { return _Description == null ? "" : _Description; }
            set { _Description = value; }
        }
        public DateTime Start { get; set; }
        public string Id { get; set; }
        public bool IsSelected { get; set; }
        public int Week { get; set; }
        public ICollection<ApplicationUser> RegisteredUsers;
    }
}