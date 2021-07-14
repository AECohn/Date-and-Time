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
        DateTime Formatted_Schedule;
        DateTime Recalled_Schedule;
        bool Schedule_Set;

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
                    Formatted_Schedule = DateTime.Parse(value); //JsonWriter should not engage if DateTime.Parse fails (TryParse is not available in VS 2008)
                    using (StreamWriter Schedule_Writer = new StreamWriter("\\user\\Auto_Shutdown_Schedule.json"))
                    {
                        Schedule_Writer.Write(JsonConvert.SerializeObject(Formatted_Schedule));
                    }

                    scheduled_time = value; // The string gets the value it was input with, when the string is read from it presents the value of the stored time

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
             try
            {
                using (StreamReader Schedule_Reader = new StreamReader("\\user\\Auto_Shutdown_Schedule.json"))
                {
                    Recalled_Schedule = JsonConvert.DeserializeObject<DateTime>(Schedule_Reader.ReadToEnd());
                    return Recalled_Schedule.ToString("h:mm tt"); 
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
            Scheduling = new CTimer(scheduler, this, 0, 5000);
        }
        private void scheduler(object obj)
            {
                if (DateTime.Now.Hour == Recalled_Schedule.Hour && DateTime.Now.Minute == Recalled_Schedule.Minute && Schedule_Set)                    
                        Update(this, new EventArgs());                    
            }
        }
}

