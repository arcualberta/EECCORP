﻿using System;
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

namespace EECCORP.Controllers
{
    public class EventsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Event
        public ActionResult Index()
        {
            //XXX Get registered events
            ViewBag.events = GetEvents();
            return View();
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
                db.Registrations.Add(registration);
            }
            db.SaveChanges();
            ViewBag.events = GetEvents();
            //XXX Show registered events
            return View();
        }

        private List<Event> GetEvents()
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

            List<Event> responseEvents = new List<Event>();
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    responseEvents.Add(eventItem);
                   
                }
            }
            return responseEvents;
        }
    }
}