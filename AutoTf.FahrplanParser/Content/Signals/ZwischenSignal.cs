namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Zsig
/// </summary>
public class ZwischenSignal : RowContent
{
	public ZwischenSignal(string stationName, string speed)
	{
		StationName = stationName;
		Speed = speed;
	}

	public string StationName { get; set; }
	public string Speed { get; set; }
	
	public override string GetPrint()
	{
		string speed = string.Empty;
		
		if (Speed != "40")
			speed = $" Z{Speed}";

		return $"Zsig{speed} {StationName}";
	}
}