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
                    //return Read_Schedule(); //When the schedule's value is retrieved, it reads the schedule from the file so it can be evaluated by the timer
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

                    /*Formatted_Schedule = DateTime.Parse(value.ToUpper()); //JsonWriter should not engage if DateTime.Parse fails (TryParse is not available in VS 2008 and produces methodnotfound exception) Converting to upper as lowercase pm (and presumably am) is not working properly, despite providing accurate feedback*/
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

                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday) //checks if current day is a weekend
                {
                    if (Recalled_Schedule.Weekends_Included)
                    {
                        if (DateTime.Now.Hour == Recalled_Schedule.SetTime.Hour && DateTime.Now.Minute == Recalled_Schedule.SetTime.Minute && Schedule_Set)
                        {
                            Update(this, new EventArgs());
                        }
                    }
                }

                else //if it's not a weekend  
                {
                   if (DateTime.Now.Hour == Recalled_Schedule.SetTime.Hour && DateTime.Now.Minute == Recalled_Schedule.SetTime.Minute && Schedule_Set)
                    {
                        Update(this, new EventArgs());
                    }
                }
            }
        }

    public class Full_Schedule
    {
        public DateTime SetTime;
        public bool Weekends_Included;
    }
}

