using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace Schedule
{
    public class Schedule_class
    {
        CTimer Scheduling;
        Full_Schedule Recalled_Schedule;
        Full_Schedule Delayed_Schedule = new Full_Schedule();
        bool Schedule_Set;
        bool Event_Delayed = false;
        public ushort Include_Weekends;
        public string filename;


        public void Init()
        {
            Scheduling = new CTimer(scheduler, this, 0, 1000); //Checks if current time matches recalled schedule every second
        }

        private string scheduled_time;
        public string Scheduled_Time
        {
            get
            {
                if (Schedule_Set) 
                { 
                    return Read_Schedule();
                }
                else 
                { 
                    return "Invalid Format"; 
                }
            }
            set
            {
                Schedule_Set = true;
                try
                {
                    Full_Schedule Write_Schedule = new Full_Schedule();

                    Write_Schedule.SetTime = DateTime.Parse(value.ToUpper());
                    Write_Schedule.Weekends_Included = Include_Weekends == 1 ? true : false;

                    using (StreamWriter Schedule_Writer = new StreamWriter(String.Format("{0}{1}.json", "\\user\\", filename)))
                    {
                        Schedule_Writer.Write(JsonConvert.SerializeObject(Write_Schedule));
                    }

                    scheduled_time = value; // The string gets the value it was input with, when the string is read from, it presents the value of the stored time
                    Delayed_Schedule = new Full_Schedule(); ; //Clears Delayed_Schedule if a new Scheduled Time is set
                    Event_Delayed = false;
                }
                catch
                {
                    ErrorLog.Error("Error Setting Schedule");
                    Schedule_Set = false; //if schedule is set incorrectly, the event will not be sent to Simpl+, rather than reporting an error and still maintaining the previous schedule
                }
            }

        }
       
        public string Read_Schedule()
        {
            Recalled_Schedule = new Full_Schedule();
            try
            {
                using (StreamReader Schedule_Reader = new StreamReader(String.Format("{0}{1}.json", "\\user\\", filename)))
                {
                    Recalled_Schedule = JsonConvert.DeserializeObject<Full_Schedule>(Schedule_Reader.ReadToEnd());
                    Include_Weekends = Convert.ToUInt16(Recalled_Schedule.Weekends_Included);
                    return Recalled_Schedule.SetTime.ToString("h:mm tt");
                }
            }
            catch (Exception exception)
            {
                ErrorLog.Error(exception.Message);
                return "Read_Error"; //if Schedule Set is True, but there is an issue reading the file
            }

        }

        public event EventHandler Update;
        public event EventHandler Warning;
        
        public string Delay_Schedule(ushort Minutes_Delayed)
        {
            if (Event_Delayed)
            {
                Delayed_Schedule.SetTime = Delayed_Schedule.SetTime.AddMinutes(Convert.ToDouble(Minutes_Delayed));
            }
            else
            {
                Delayed_Schedule.SetTime = Recalled_Schedule.SetTime.AddMinutes(Convert.ToDouble(Minutes_Delayed));
                Delayed_Schedule.Weekends_Included = Recalled_Schedule.Weekends_Included;
            }
            Event_Delayed = true;
            return Delayed_Schedule.SetTime.ToString("h:mm tt");

        }

        private void Schedule_Checker(Full_Schedule Schedule_To_Check)
        {
            DayOfWeek CurrentDay = DateTime.Now.DayOfWeek;
            string simple_CurrentTime = DateTime.Now.ToShortTimeString();
            bool Is_Weekend = false;

            if (CurrentDay == DayOfWeek.Saturday || CurrentDay == DayOfWeek.Sunday)
            {
                Is_Weekend = true;
            }
            if ((Schedule_To_Check.Weekends_Included && Is_Weekend) || (Schedule_To_Check.Weekends_Included == false && Is_Weekend == false))
            {
                if (simple_CurrentTime == Schedule_To_Check.Simple_Time)
                {                   
                        Update(this, new EventArgs());
                        Event_Delayed = false;
                        Delayed_Schedule = new Full_Schedule(); ; //Clears Delayed_Schedule when event elapses
                }

                else if (simple_CurrentTime == Schedule_To_Check.Warning_Time)
                {
                    Warning(this, new EventArgs());
                }
            }

           
        }


        private void scheduler(object obj)
        {
            if (Event_Delayed)
            {
                Schedule_Checker(Delayed_Schedule);
            }
            else
            {
                Schedule_Checker(Recalled_Schedule);
            }


        }

        public class Full_Schedule
        {
            public string Warning_Time;
            private DateTime _setTime;
            public DateTime SetTime
            {
                get
                {
                    return _setTime;
                }

                set
                {
                    _setTime = value;
                    Simple_Time = _setTime.ToShortTimeString();
                    Warning_Time = _setTime.AddMinutes(-15).ToShortTimeString();
                }
            }
            public string Simple_Time;
            public bool Weekends_Included;
        }
    }
}

