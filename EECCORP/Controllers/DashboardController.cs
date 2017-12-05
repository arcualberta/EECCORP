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

        private void SetViewbag(List<Models.Event> events, DateTime? start = null, DateTime? end = null)
        {
            ViewBag.events = events;
            ViewBag.registrations = GetRegistrations(events);
            ViewBag.users = GetEventsUsers(ViewBag.registrations);


            ViewBag.start = start != null ? ((DateTime)start).ToString("dd/M/yyyy") : null;
            ViewBag.end = end != null ? ((DateTime)end).ToString("dd/M/yyyy") : null;
        }

        private List<Registration> GetRegistrations(List<Models.Event>events)
        {
            IEnumerable<string> eventIds = events.Select(x => x.Id).ToList();
            List<Registration> registrations = Db.Registrations.Where(r => eventIds.Contains(r.EventId)).ToList();
            return registrations;
        }

        private List<dynamic> GetEventsUsers(List<Registration> registrations)
        {
            
            IEnumerable<string> userIds = registrations.Select(u => u.UserId).ToList();

            List<ApplicationUser> users = Db.Users.Where(m => userIds.Contains(m.Id)).ToList();

            List<dynamic> result = new List<dynamic>();

            foreach (ApplicationUser user in users)
            {
                List<Registration> userRegistrations = registrations.Where(r => user.Id == r.UserId).ToList();
                dynamic userResult = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                };
                result.Add(userResult);
            }
            
            return result;
        }
    }
}