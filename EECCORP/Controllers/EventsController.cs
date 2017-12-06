using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Configuration;
using EECCORP.Models;
using EECCORP.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Xml;
using System.Globalization;
using EECCORP.Services;

namespace EECCORP.Controllers
{
    public class EventsController : Controller
    {

        private ApplicationDbContext _Db;
        private UserStore<ApplicationUser> _UserStore;
        private UserManager<ApplicationUser> _UserManager;
        private CultureInfo CultureInfo = new CultureInfo("en-ca");
        private System.Globalization.Calendar _Calendar;
        private GoogleService _GoogleService;

        private ApplicationDbContext Db { get { if (_Db == null) _Db = new ApplicationDbContext(); return _Db; } }
        private UserStore<ApplicationUser> UserStore { get { if (_UserStore == null) _UserStore = new UserStore<ApplicationUser>(Db); return _UserStore; } }
        private UserManager<ApplicationUser> UserManager { get { if (_UserManager == null) _UserManager = new UserManager<ApplicationUser>(UserStore); return _UserManager; } }
        private System.Globalization.Calendar Calendar { get { if (_Calendar == null) { _Calendar = CultureInfo.DateTimeFormat.Calendar; } return _Calendar; } }
        private GoogleService GoogleService { get { if (_GoogleService == null) { _GoogleService = new GoogleService(HttpContext); } return _GoogleService; } }


        // GET: Event
        public ActionResult Index()
        {
            //XXX Get registered events
            
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);
            if (user.IsEligible)
            {
                List<Models.Event> events = GoogleService.GetEventsFromWindow();
                List<Registration> registrations = new List<Registration>();
                foreach (Models.Event currentEvent in events)
                {
                    Registration registration = Db.Registrations.SingleOrDefault(i => i.EventId == currentEvent.Id);
                    currentEvent.IsSelected = registration != null;                    
                }
                return View(events);
            } else
            {
                IsEligibleViewModel eligible = new IsEligibleViewModel();
                return RedirectToAction("Eligibility", eligible);
            }            
        }

        // POST: Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Models.Event[] frontEndEvents)
        {
            Dictionary<int, int> previousRegistrations = new Dictionary<int, int>();
            string userId = User.Identity.GetUserId();
            List<Models.Event> events = GoogleService.GetEventsFromWindow();
            bool onlyTwoEventsPerWeek = true;

            foreach (Models.Event currentEvent in frontEndEvents)
            {

                Models.Registration prevRegistration = Db.Registrations.SingleOrDefault(i => i.UserId == userId && i.EventId == currentEvent.Id);
                if (currentEvent.IsSelected)
                {
                    // check only 2 registrations per week
                    // process week from fetched events
                    DateTime start = GetEventStart(events, currentEvent);
                    int currentWeek = Calendar.GetWeekOfYear(start,
                       CalendarWeekRule.FirstDay,
                       DayOfWeek.Sunday);
                    if (!previousRegistrations.ContainsKey(currentWeek))
                    {
                        previousRegistrations[currentWeek] = 0;
                    }

                    ++previousRegistrations[currentWeek];

                    if (previousRegistrations[currentWeek] > 2)
                    {
                        onlyTwoEventsPerWeek = false;
                        break;
                    }

                    if (prevRegistration == null)
                    {
                        Registration registration = new Registration
                        {
                            UserId = userId,
                            EventId = currentEvent.Id
                        };
                        
                        Db.Registrations.Add(registration);
                    }
                } else
                {
                    if (prevRegistration != null)
                    {
                        Db.Registrations.Remove(prevRegistration);
                    }
                }
            }

            //Db.SaveChanges();
            // Save if we only have 2 events per week
            if (onlyTwoEventsPerWeek)
            {
                Db.SaveChanges();
            }
            else
            {
                // XXX Handle gracefully              
            }

            return RedirectToAction("Index");

        }

        // GET: Eligibility
        public ActionResult Eligibility()
        {
            IsEligibleViewModel eligible = new IsEligibleViewModel();
            return View(eligible);
        }

        // POST: Eligibility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eligibility(IsEligibleViewModel response)
        {
            if (response.IsEnrolled && response.IsFoAStudent)
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                user.IsEligible = true;
                UserManager.UpdateAsync(user);
                return RedirectToAction("Index");
            }
            return View();
        }
        
        private DateTime GetEventStart(List<Models.Event> events, Models.Event currentEvent)
        {
            foreach(Models.Event thisEvent in events)
            {
                //if(thisEvent.Id)
                if(thisEvent.Id == currentEvent.Id)
                {
                    return thisEvent.Start;
                }
            }
            return new DateTime(0, 0, 0);
        }
    }
}