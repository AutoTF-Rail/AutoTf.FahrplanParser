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
	
	public override string GetPrint()
	{
		string speed = string.Empty;
		
		if (Speed != "40")
			speed = $" A{Speed}";

		return $"Asig{speed} {StationName}";
	}
}