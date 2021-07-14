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
            get { return scheduled_time; }
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

                    Read_Schedule();
                    scheduled_time = Recalled_Schedule.ToString("h:mm tt");                    
                }
                catch
                {
                    scheduled_time = "invalid format";
                    Schedule_Set = false; //if schedule is set incorrectly, the scheduling will break, rather than reporting an error and still maintaining the previous schedule
                }

                
                    
            }

        }

        public void Read_Schedule()
        {
             try
            {
                using (StreamReader Schedule_Reader = new StreamReader("\\user\\Shutdown_Schedule.json"))
                {
                    Recalled_Schedule = JsonConvert.DeserializeObject<DateTime>(Schedule_Reader.ReadToEnd());
                }
            }
            catch (Exception exception)
            {
                ErrorLog.Error(exception.Message);
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

