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

namespace EECCORP.Services
{

    public class GoogleService
    {

        private CultureInfo CultureInfo = new CultureInfo("en-ca");
        private System.Globalization.Calendar _Calendar;
        private HttpContextBase HttpContextBase;

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

        public List<Models.Event> GetEvents(DateTime? _start = null, DateTime? _end = null)
        {

            //DateTime start = _start == null ? DateTime.Now.StartOfWeek(DayOfWeek.Monday) : _start.StartOfWeek(DayOfWeek.Monday);
            //DateTime end = _end == null ? start.AddDays(21) : _end; 
            DateTime start = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            DateTime end = start.AddDays(21);

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
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

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
    }
}