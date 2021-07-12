using System;
using System.Text;
using Crestron.SimplSharp;
                      				
namespace Schedule
{
    public class Schedule_class
    {
        CTimer Scheduling;
        DateTime Formatted_Schedule;
        bool Schedule_Set;

        private string scheduled_time;
        public string Scheduled_Time
        
        {
            get { return scheduled_time; }
            set 
            { 
                //scheduled_time = value;
                Schedule_Set = true;
                try
                {
                    Formatted_Schedule = DateTime.Parse(value);
                    scheduled_time = Formatted_Schedule.ToString("h:mm tt");
                }
                catch
                {
                    scheduled_time = "invalid format";
                    Schedule_Set = false;
                }
                    
            }

        }

        public event EventHandler Update;
        public Schedule_class()
        {
            Scheduling = new CTimer(scheduler, this, 0, 5000);
        }
        private void scheduler(object obj)
                {
                    if (DateTime.Now.Hour == Formatted_Schedule.Hour && DateTime.Now.Minute == Formatted_Schedule.Minute && Schedule_Set)                    
                            Update(this, new EventArgs());                    
                }
        }
}

