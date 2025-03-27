namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Bksig
/// </summary>
public class BlockSignal : RowContent
{
	public BlockSignal(string stationName, string speed)
	{
		StationName = stationName;
		Speed = speed;
	}

	public string StationName { get; set; }
	public string Speed { get; set; }
}