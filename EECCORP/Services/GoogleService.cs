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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Xml;
using System.Globalization;

namespace EECCORP.Services
{

    public class GoogleService
    {

        private CultureInfo CultureInfo = new CultureInfo("en-ca");
        private System.Globalization.Calendar _Calendar;
        private HttpContextBase HttpContextBase;
        private ApplicationDbContext _Db;
        private ApplicationDbContext Db { get { if (_Db == null) _Db = new ApplicationDbContext(); return _Db; } }

        public GoogleService(HttpContextBase httpContextBase)
        {
            HttpContextBase = httpContextBase;
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

        public List<Models.Event> GetEventsFromWindow()
        {
            DateTime start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            DateTime end = start.AddDays(21);
            return this.GetEvents(start, end);
        }

        public Models.Event GetEvent(string eventId)
        {

            CalendarService service = GetService();
            string calendarId = ConfigurationManager.AppSettings["Google:CalendarId"];
            Google.Apis.Calendar.v3.Data.Event googleEvent = service.Events.Get(calendarId, eventId).Execute();
            Models.Event result = new Models.Event();
            result.Id = googleEvent.Id;
            result.Summary = googleEvent.Summary;
            result.Description = googleEvent.Description;
            result.Start = XmlConvert.ToDateTime(googleEvent.Start.DateTimeRaw, XmlDateTimeSerializationMode.Local);

            return result;
        }

       
        public List<Models.Event> GetEvents(DateTime start, DateTime end)
        {
            //start = start == null ? DateTime.Now.StartOfWeek(DayOfWeek.Monday) : (DateTime)start;
            //end = end == null ? ((DateTime)start).AddDays(21) : (DateTime)end; 


            CalendarService service = GetService();
            string calendarId = ConfigurationManager.AppSettings["Google:CalendarId"];
            
            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(calendarId);
            //DateTime start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            request.TimeMin = start;
            request.TimeMax = end;
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

        public List<Models.Event> GetUsersEvents(ApplicationUser user)
        {
            List<Models.Event> result = new List<Models.Event>();
            List<Registration> registrations = Db.Registrations.Where(r => r.UserId == user.Id).ToList();

            // Fetch all events from google individually
            foreach (Registration registration in registrations)
            {
                Models.Event currentEvent = GetEvent(registration.EventId);
                result.Add(currentEvent);
            }

            return result;
        }

        private CalendarService GetService()
        {
            string[] Scopes = { CalendarService.Scope.CalendarReadonly };
            string ApplicationName = ConfigurationManager.AppSettings["Application:Name"];

            UserCredential credential;

            FileStream stream = new FileStream(HttpContextBase.Server.MapPath(@"~/client_secret.json"), FileMode.Open, FileAccess.Read);

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
            CalendarService service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }
    }
}