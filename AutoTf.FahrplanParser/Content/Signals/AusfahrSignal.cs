namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Asig
/// </summary>
public class AusfahrSignal : RowContent
{
	public AusfahrSignal(string stationName, string speed = "40")
	{
		StationName = stationName;
		Speed = speed;
	}

	public string StationName { get; set; }
	public string Speed { get; set; }
}