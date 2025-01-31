public class Rootobject
{
    public Item[] items { get; set; }
}

public class Item
{
    public Forecast[] forecasts { get; set; }
}

public class Forecast
{
    public string date { get; set; }
    public string forecast { get; set; }
    public Temperature temperature { get; set; }
    public Relative_Humidity relative_humidity { get; set; }
    public Wind wind { get; set; }
}

public class Temperature
{
    public int low { get; set; }
    public int high { get; set; }
}

public class Relative_Humidity
{
    public int low { get; set; }
    public int high { get; set; }
}

public class Wind
{
    public Speed speed { get; set; }
    public string direction { get; set; }
}

public class Speed
{
    public int low { get; set; }
    public int high { get; set; }
}
