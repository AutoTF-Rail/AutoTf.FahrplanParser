namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Sbk
/// </summary>
public class SelbstBlockSignal : RowContent
{
	public SelbstBlockSignal(string stationName, string speed)
	{
		StationName = stationName;
		Speed = speed;
	}

	public string StationName { get; set; }
	public string Speed { get; set; }
}