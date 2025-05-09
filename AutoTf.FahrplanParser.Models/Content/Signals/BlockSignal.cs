using AutoTf.FahrplanParser.Content.Content.Base;

namespace AutoTf.FahrplanParser.Content.Content.Signals;

/// <summary>
/// Bksig
/// </summary>
public class BlockSignal : SignalContent
{
	private BlockSignal(string stationName, string speed)
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
			speed = $"Bk{Speed}";

		return $"Bksig{speed} {StationName}";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;
		
		if (!TryParse(additionalText, "Bksig", "Bk", out string speed, out string stationName))
			return false;

		content = new BlockSignal(stationName, speed);
		return true;
	}
}