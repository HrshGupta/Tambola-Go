using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ticket
{
    public int ticket_id ;
    public string ticket_name ;
    public string ticket_image ;
}


[System.Serializable]
public class EventData
{
    public string event_status ;
    public string event_id ;
    public string event_name ;
    public string event_date ;
    public string event_time ;
    public string number_of_ticket ;
    public List<Ticket> tickets ;
}

public class CreatedEvent
{
    public int status ;
    public string message ;
    public EventData events ;
}

[System.Serializable]
public class CreateData
{
    public string event_date;
    public string event_time;
    public string user_id;
    public string category_id;
    public string sub_category_id;
    public string cost_of_ticket;
    public string number_of_ticket;
}