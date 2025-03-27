namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Bkvsig
/// </summary>
public class BlockVorsignal : RowContent
{
	public BlockVorsignal(string stationName, string speed)
	{
		StationName = stationName;
		Speed = speed;
	}

	public string StationName { get; set; }
	public string Speed { get; set; }
}