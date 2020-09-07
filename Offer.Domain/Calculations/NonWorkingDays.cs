using System;
using System.Collections.Generic;

namespace Offer.Domain.Calculations
{
    public class NonWorkingDayCalendar
    {
        private List<DateTime> m_NonWorkingDays = new List<DateTime>();

        public List<DateTime> NonWorkingDays
        {
            get { return m_NonWorkingDays; }
            set { m_NonWorkingDays = value; }
        }
    }

    // TODO: Umesto ovoga treba napraviti normalno dobavljanje kalendara neradnih dana
    public static class NonWorkingDays 
    {
        private static Dictionary<string, NonWorkingDayCalendar> m_AllCalendars = PopulateCalendars();

        public static Dictionary<string, NonWorkingDayCalendar> AllCalendars { get => m_AllCalendars; set => m_AllCalendars = value; }

        private static Dictionary<string, NonWorkingDayCalendar> PopulateCalendars()
        {
            if (AllCalendars == null)
            {
                Dictionary<string, NonWorkingDayCalendar>  allCalendars = new Dictionary<string, NonWorkingDayCalendar>();

                /*
                // sad samo za test upucavam vikende
                NonWorkingDayCalendar calendar = new NonWorkingDayCalendar();
                DateTime saturday = new DateTime(2010, 1, 2);
                DateTime sunday = new DateTime(2010, 1, 3);
                DateTime end = new DateTime(2100, 1, 1);
                while (saturday < end)
                {
                    calendar.NonWorkingDays.Add(saturday);
                    calendar.NonWorkingDays.Add(sunday);
                    saturday = saturday.AddDays(7);
                    sunday = saturday.AddDays(1);
                }
                allCalendars.Add("WeekendsOnly", calendar);
                */

                return allCalendars;
            }
            else
            {
                return AllCalendars;
            }
        }

    }
}
