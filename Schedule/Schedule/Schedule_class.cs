using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace DGI_Schedule
{
    public class Schedule_class
    {
        private int currentSecond;
        private CTimer Scheduling;
        private Full_Schedule Recalled_Schedule;
        private Full_Schedule Delayed_Schedule;
        private bool Event_Delayed = false;
        private ushort _Warning_Time_Input;
        private ushort Warning_Time;
        //private static Func<DateTime, String> TimeToString = time => time.ToString("h:mm tt");
        public delegate void DateTimeTransmit(ushort timer, SimplSharpString Date_and_Time, SimplSharpString Date, SimplSharpString Time);
        public DateTimeTransmit Transmit_DateTime { get; set; }
        bool Warning_Active = false;

        public Schedule_class()
        {
        }

        public void Init(ushort Warning_Time_Input)
        {
            _Warning_Time_Input = Warning_Time_Input;
            Scheduling = new CTimer(scheduler, this, 0, 1000); //Checks if current time matches recalled schedule every second
        }

        public string Scheduled_Time(string Input_Time, string filename, ushort Include_Weekends)
        {
            try
            {
                Full_Schedule Write_Schedule = new Full_Schedule();

                Write_Schedule.ScheduleTime = DateTime.Parse(Input_Time.ToUpper());
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
                    if (Recalled_Schedule.Weekends_Included)
                    {
                        return (Recalled_Schedule.Simple_Time + " Weekends");
                    }
                    else
                    {
                        return (Recalled_Schedule.Simple_Time);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorLog.Error(exception.Message);
                return "Read_Error"; //if Schedule Set is True, but there is an issue reading the file
            }
        }

        public event EventHandler Update, Warning;

        public string Delay_Schedule(ushort Minutes_Delayed)
        {
            if (Event_Delayed)
            {
                Delayed_Schedule.ScheduleTime = Delayed_Schedule.ScheduleTime.AddMinutes(Convert.ToDouble(Minutes_Delayed));
            }
            else
            {
                Delayed_Schedule.ScheduleTime = Recalled_Schedule.ScheduleTime.AddMinutes(Convert.ToDouble(Minutes_Delayed));
                Delayed_Schedule.Weekends_Included = Recalled_Schedule.Weekends_Included;
            }
            Warning_Active = false;
            Event_Delayed = true;
            return Delayed_Schedule.ScheduleTime.ToString("h:mm tt");
        }

        private void Schedule_Checker(Full_Schedule Schedule_To_Check)
        {
            ushort timeSecond = (ushort)DateTime.Now.Second;
            DayOfWeek CurrentDay = DateTime.Now.DayOfWeek;
            string simple_CurrentTime = DateTime.Now.ToString("h:mm tt");
            string simple_WarningTime = Schedule_To_Check.ScheduleTime.AddMinutes(-(double)_Warning_Time_Input).ToString("h:mm tt");
            CrestronConsole.PrintLine(simple_WarningTime);
            bool Is_Weekend = false;

            string Output_DateTime = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt");
            string Output_Date = DateTime.Now.ToString("MMMM dd, yyyy");
            string Output_Time = DateTime.Now.ToString("h:mm tt");
             
            Transmit_DateTime(Warning_Time, Output_DateTime, Output_Date, Output_Time);

            if (timeSecond != currentSecond)
            {
                if (Warning_Active)
                {
                    Warning_Time = timeSecond;
                }
                currentSecond = timeSecond;
            }


            if (CurrentDay == DayOfWeek.Saturday || CurrentDay == DayOfWeek.Sunday)
            {
                Is_Weekend = true;
            }
            if (!(Schedule_To_Check.Weekends_Included == false && Is_Weekend)) // would only not work if weekends aren't included and it is a weekend
            {
                if (simple_CurrentTime == Schedule_To_Check.Simple_Time)
                {
                    Update(this, new EventArgs());
                    Event_Delayed = false;
                    Delayed_Schedule = new Full_Schedule(); ; //Clears Delayed_Schedule when event elapses
                    Warning_Active = false;
                }
                else if (simple_CurrentTime == simple_WarningTime && Warning_Active == false)
                {
                    Warning_Time = (ushort)(_Warning_Time_Input * 60);
                    Warning(this, new EventArgs());
                    Warning_Active = true;

                }
                if (Warning_Active)
                {
                    Warning_Time -= 1;

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

        
    }

    internal class Full_Schedule
    {
        
        private DateTime _scheduleTime;
        
       
        public DateTime ScheduleTime
        {
            get
            {
                return _scheduleTime;
            }

            set
            {
                _scheduleTime = value;
                Simple_Time = ScheduleTime.ToString("h:mm tt");
            }
        }

        public string Simple_Time;
        public bool Weekends_Included;

        public Full_Schedule()
        {
        }

        
    
    }
}