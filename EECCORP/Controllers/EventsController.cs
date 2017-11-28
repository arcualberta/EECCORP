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
     
        // GET: Event
        public ActionResult Index()
        {
            //XXX Get registered events
            
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);
            if (user.IsEligible)
            {
                List<Models.Event> events = GetEvents();
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
        public ActionResult Index(Registration[] registrations, int[] selected)
        {
            foreach (int index in selected)
            {
                Registration registration = registrations[index];
                registration.UserId = User.Identity.GetUserId();
                Db.Registrations.Add(registration);
            }
            Db.SaveChanges();
            ViewBag.events = GetEvents();
            //XXX Show registered events
            return View();
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
            //request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            CultureInfo cultureInfo = new CultureInfo("en-ca");
            System.Globalization.Calendar calendar = cultureInfo.DateTimeFormat.Calendar;

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
                    
                    int currentWeek = calendar.GetWeekOfYear(currentEvent.Start, 
                        CalendarWeekRule.FirstDay,
                        DayOfWeek.Sunday);
                    
                    currentEvent.Week = currentWeek;
                    responseEvents.Add(currentEvent);
                }
            }
            return responseEvents;
        }
    }
}