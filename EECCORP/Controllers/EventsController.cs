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

namespace EECCORP.Controllers
{
    public class EventsController : Controller
    {

        private ApplicationDbContext _Db;
        private UserStore<ApplicationUser> _UserStore;
        private UserManager<ApplicationUser> _UserManager;
        private CultureInfo CultureInfo = new CultureInfo("en-ca");
        private System.Globalization.Calendar _Calendar;


        private ApplicationDbContext Db {
            get
            {
                if (_Db == null)
                    _Db = new ApplicationDbContext();
                return _Db;
            }
        }
        private UserStore<ApplicationUser> UserStore {
            get
            {
                if (_UserStore == null)
                    _UserStore = new UserStore<ApplicationUser>(Db);
                return _UserStore;
            }
        }

        private UserManager<ApplicationUser> UserManager
        {
            get
            {
                if (_UserManager == null)
                    _UserManager = new UserManager<ApplicationUser>(UserStore);
                return _UserManager;
            }
        }

        private System.Globalization.Calendar Calendar
        {
            get
            {
                if (_Calendar == null)
                {
                    _Calendar = CultureInfo.DateTimeFormat.Calendar;
                }
                return _Calendar;
            }
        }
     
        // GET: Event
        public ActionResult Index()
        {
            //XXX Get registered events
            
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);
            if (user.IsEligible)
            {
                List<Models.Event> events = GetEvents();
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

        private int GetEventWeek(List<Models.Event> events, Models.Event currentEvent)
        {

            return 0;
        }

        // POST: Event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Models.Event[] frontEndEvents)
        {
            Dictionary<int, int> previousRegistrations = new Dictionary<int, int>();
            string userId = User.Identity.GetUserId();
            List<Models.Event> events = GetEvents();
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

        private List<Models.Event> GetEvents()
        {
            string[] Scopes = { CalendarService.Scope.CalendarReadonly };
            string ApplicationName = ConfigurationManager.AppSettings["Application:Name"];

            UserCredential credential;

            FileStream stream = new FileStream(Server.MapPath(@"~/client_secret.json"), FileMode.Open, FileAccess.Read);

            string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            string calendarId = ConfigurationManager.AppSettings["Google:CalendarId"];

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(calendarId);
            DateTime start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            request.TimeMin = start;
            request.TimeMax = start.AddDays(21);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            //CultureInfo cultureInfo = new CultureInfo("en-ca");
            //System.Globalization.Calendar calendar = cultureInfo.DateTimeFormat.Calendar;

            List<Models.Event> responseEvents = new List<Models.Event>();

            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    Models.Event currentEvent = new Models.Event();
                    currentEvent.Id = eventItem.Id;
                    currentEvent.Summary = eventItem.Summary;
                    currentEvent.Description = eventItem.Description;
                    currentEvent.Start = XmlConvert.ToDateTime(eventItem.Start.DateTimeRaw, XmlDateTimeSerializationMode.Local);
                    
                    int currentWeek = Calendar.GetWeekOfYear(currentEvent.Start, 
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Sunday);
                    
                    currentEvent.Week = currentWeek;
                    responseEvents.Add(currentEvent);
                }
            }
            return responseEvents;
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