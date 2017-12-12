using EECCORP.Models;
using EECCORP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECCORP.Extensions;


namespace EECCORP.Controllers
{
    public class DashboardController : Controller
    {
        private GoogleService _GoogleService;
        private GoogleService GoogleService { get { if (_GoogleService == null) { _GoogleService = new GoogleService(HttpContext); } return _GoogleService; } }
        private ApplicationDbContext _Db;
        private ApplicationDbContext Db { get { if (_Db == null) _Db = new ApplicationDbContext(); return _Db; } }
        private HelperService _HelperService;
        private HelperService HelperService
        {
            get
            {
                if (_HelperService == null)
                {
                    _HelperService = new HelperService();
                }
                return _HelperService;
            }
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            DateTime start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            DateTime end = start.AddDays(21);
            List<Models.Event> events = GoogleService.GetEvents(start, end);
            SetViewbag(events, start, end);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(DateTime? start = null, DateTime? end = null)
        {
            start = start == null ? DateTime.Now.StartOfWeek(DayOfWeek.Monday) : (DateTime)start;
            end = end == null ? ((DateTime)start).AddDays(21) : (DateTime)end;

            List<Models.Event> events = GoogleService.GetEvents((DateTime)start, (DateTime)end);
            SetViewbag(events, start, end);

            return View();
        }

        public ActionResult Event(string id)
        {
            Models.Event currentEvent = GoogleService.GetEvent(id);
            currentEvent.RegisteredUsers = GetRegisteredUsers(currentEvent);
            return View("View", currentEvent);
        }

        // GET: DownloadReport
        public FileContentResult DownloadEventReport(string id)
        {
            Models.Event currentEvent = GoogleService.GetEvent(id);
            string csv = GetEventReport(currentEvent);
            string fileName = HelperService.GetReportFileName((string)currentEvent.Summary);
            return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", fileName);
        }

        public FileContentResult DownloadEventsReport(DateTime? start = null, DateTime? end = null)
        {
            start = start == null ? DateTime.Now.StartOfWeek(DayOfWeek.Monday) : (DateTime)start;
            end = end == null ? ((DateTime)start).AddDays(21) : (DateTime)end;
            List<Models.Event> events = GoogleService.GetEvents((DateTime)start, (DateTime)end);

            string csv = GetEventsReport(events);
            string fileName = HelperService.GetReportFileName("Events");
            return File(new System.Text.UTF8Encoding().GetBytes(csv), "text/csv", fileName);
        }
     
        private string GetEventsReport(List<Models.Event> events)
        {

            events = AddUsersToEvents(events);
            string report = "";
            foreach(Models.Event currentEvent in events)
            {
                report += HelperService.GetCSVLine(new[] {
                    currentEvent.Summary.ToString(),
                    currentEvent.Start.ToString("dd/M/yyyy"),
                    currentEvent.RegisteredUsers.Count().ToString()
                });

            }
            return report;
        }

        private string GetEventReport(Models.Event currentEvent)
        {
            
            ICollection<Models.ApplicationUser> registeredUsers = GetRegisteredUsers(currentEvent.Id);
            
            string result = "";         
            foreach (Models.ApplicationUser user in registeredUsers)
            {
                result += HelperService.GetCSVLine(new[] { user.Name, user.Email });
            }

            return result;
        }

        private void SetViewbag(List<Models.Event> events, DateTime? start = null, DateTime? end = null)
        {
            ViewBag.events = AddUsersToEvents(events);
            //ViewBag.start = start != null ? ((DateTime)start).ToString("dd/M/yyyy") : null;
            //ViewBag.end = end != null ? ((DateTime)end).ToString("dd/M/yyyy") : null;
            ViewBag.start = start;
            ViewBag.end = end;
        }

        private ICollection<ApplicationUser> GetRegisteredUsers(Models.Event currentEvent)
        {
            currentEvent.RegisteredUsers = new List<ApplicationUser>();
            // Get event registrations
            List<Registration> registrations = Db.Registrations.Where(r => r.EventId == currentEvent.Id).ToList();
            return registrations.Select(m => m.User).Distinct().ToList();            
        }

        private ICollection<ApplicationUser> GetRegisteredUsers(string eventId)
        {
            List<Registration> registrations = Db.Registrations.Where(r => r.EventId == eventId).ToList();
            return registrations.Select(m => m.User).Distinct().ToList();
        }

        private List<Models.Event> AddUsersToEvents(List<Event> events)
        {
            foreach (Models.Event currentEvent in events) {
                currentEvent.RegisteredUsers = GetRegisteredUsers(currentEvent);
            }

            return events;
        }
    }
}