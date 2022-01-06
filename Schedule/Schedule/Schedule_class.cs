using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace Schedule
{
    public class Schedule_class
    {
        private CTimer Scheduling;
        private Full_Schedule Recalled_Schedule;
        private Full_Schedule Delayed_Schedule = new Full_Schedule();
        private bool Event_Delayed = false;
        //public ushort Include_Weekends;
        static Func<DateTime, String> TimeToString = time => time.ToString("h:mm tt");

        public void Init()
        {
            Scheduling = new CTimer(scheduler, this, 0, 1000); //Checks if current time matches recalled schedule every second
        }

        public string Scheduled_Time(string Input_Time, string filename, ushort Include_Weekends)
        {
            try
            {
                Full_Schedule Write_Schedule = new Full_Schedule();

                Write_Schedule.SetTime = DateTime.Parse(Input_Time.ToUpper());
                Write_Schedule.Weekends_Included = Include_Weekends == 1 ? true : false;

                using (StreamWriter Schedule_Writer = new StreamWriter(String.Format("{0}{1}.json", "\\user\\", filename)))
                {
                    Schedule_Writer.Write(JsonConvert.SerializeObject(Write_Schedule));
                }

                Delayed_Schedule = new Full_Schedule(); ; //Clears Delayed_Schedule if a new Scheduled Time is set
                Event_Delayed = false;
                return ("Schedule set");
            }
            catch
            {
                ErrorLog.Error("Error setting schedule");
                return ("Error setting schedule");
            }           
        }

        public string Read_Schedule(string filename)
        {
            Recalled_Schedule = new Full_Schedule();
            try
            {
                using (StreamReader Schedule_Reader = new StreamReader(String.Format("{0}{1}.json", "\\user\\", filename)))
                {
                    Recalled_Schedule = JsonConvert.DeserializeObject<Full_Schedule>(Schedule_Reader.ReadToEnd());
                    //Include_Weekends = Convert.ToUInt16(Recalled_Schedule.Weekends_Included);
                    //return Recalled_Schedule.SetTime.ToString("h:mm tt");
                    //return Recalled_Schedule.Simple_Time
                    if (Recalled_Schedule.Weekends_Included)
                        return (Recalled_Schedule.Simple_Time + " Weekends");
                    else
                        return (Recalled_Schedule.Simple_Time);
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
            //string simple_CurrentTime = DateTime.Now.ToShortTimeString();
            string simple_CurrentTime = TimeToString(DateTime.Now);
            bool Is_Weekend = false;

            CrestronConsole.PrintLine("simple_CurrentTime = {0}, Schedule_To_Check.Simple_Time = {1}", simple_CurrentTime, Schedule_To_Check.Simple_Time);
 
            if (CurrentDay == DayOfWeek.Saturday || CurrentDay == DayOfWeek.Sunday)
            {
                Is_Weekend = true;
            }
            //if ((Schedule_To_Check.Weekends_Included && Is_Weekend) || (Schedule_To_Check.Weekends_Included == false && Is_Weekend == false))
            if (!(Schedule_To_Check.Weekends_Included == false && Is_Weekend) ) // would only not work if weekends aren't included and it is a weekend 
            {
                if (simple_CurrentTime == Schedule_To_Check.Simple_Time)
                {
                    CrestronConsole.PrintLine("Time Match");
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
                    /*Simple_Time = _setTime.ToShortTimeString();
                    Warning_Time = _setTime.AddMinutes(-15).ToShortTimeString();*/
                    Simple_Time = TimeToString(SetTime);
                    Warning_Time = TimeToString(SetTime.AddMinutes(-15));
                }
            }

            public string Simple_Time;
            public bool Weekends_Included;
        }
    }
}