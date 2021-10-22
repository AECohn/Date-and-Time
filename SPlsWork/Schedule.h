namespace Schedule;
        // class declarations
         class Schedule_class;
     class Schedule_class 
    {
        // class delegates

        // class events
        EventHandler Update ( Schedule_class sender, EventArgs e );

        // class functions
        STRING_FUNCTION Read_Schedule ();
        SIGNED_LONG_INTEGER_FUNCTION GetHashCode ();
        STRING_FUNCTION ToString ();

        // class variables
        STRING filename[];

        // class properties
        STRING Scheduled_Time[];
    };

