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
        bool Schedule_Set;
        public ushort Include_Weekends;

        public string filename;



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
                    Write_Schedule.Simple_Time = Write_Schedule.SetTime.ToShortTimeString();
                    Write_Schedule.Weekends_Included = Include_Weekends == 1 ? true : false;

                    using (StreamWriter Schedule_Writer = new StreamWriter(String.Format("{0}{1}.json", "\\user\\", filename)))
                    {
                        Schedule_Writer.Write(JsonConvert.SerializeObject(Write_Schedule));
                    }

                    scheduled_time = value; // The string gets the value it was input with, when the string is read from, it presents the value of the stored time

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
        public Schedule_class()
        {
            Scheduling = new CTimer(scheduler, this, 0, 1000); //Checks if current time matches recalled schedule every second
        }


        private void scheduler(object obj)
        {
            DayOfWeek CurrentDay = DateTime.Now.DayOfWeek;
            string simple_CurrentTime = DateTime.Now.ToShortTimeString();
            bool Is_Weekend = false;

            if (CurrentDay == DayOfWeek.Saturday || CurrentDay == DayOfWeek.Sunday)
            {
                Is_Weekend = true;
            }

            if (simple_CurrentTime == Recalled_Schedule.Simple_Time)
            {
                if ((Recalled_Schedule.Weekends_Included && Is_Weekend) || (Recalled_Schedule.Weekends_Included == false && Is_Weekend == false))
                {
                    Update(this, new EventArgs());
                }
            }

        }

        public class Full_Schedule
        {
            public DateTime SetTime;
            public string Simple_Time;
            public bool Weekends_Included;
        }
    }
}

