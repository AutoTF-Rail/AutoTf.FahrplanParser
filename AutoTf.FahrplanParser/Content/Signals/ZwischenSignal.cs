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
}