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
        int Poll;

        public string filename;

        private string scheduled_time;
        public string Scheduled_Time
        
        {
            get
            {
                if (Schedule_Set)
                {
                    return Read_Schedule(); //When the schedule's value is retrieved, it reads the schedule from the file so it can be evaluated by the timer
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
                    Formatted_Schedule = DateTime.Parse(value.ToUpper()); //JsonWriter should not engage if DateTime.Parse fails (TryParse is not available in VS 2008) Converting to upper as lowercase pm (and presumably am) is not working properly, despite providing accurate feedback
                    using (StreamWriter Schedule_Writer = new StreamWriter(String.Format("{0}{1}.json", "\\user\\", filename))) 
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
                using (StreamReader Schedule_Reader = new StreamReader(String.Format("{0}{1}.json", "\\user\\", filename)))
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
            Scheduling = new CTimer(scheduler, this, 0, 5000); //Checks if current time matches recalled schedule every 5 seconds
        }
        private void scheduler(object obj)
            {
            CrestronConsole.PrintLine(Poll.ToString());
            Poll++;
                if (DateTime.Now.Hour == Recalled_Schedule.Hour && DateTime.Now.Minute == Recalled_Schedule.Minute && Schedule_Set)                    
                        Update(this, new EventArgs());                    
            }
        }
}

