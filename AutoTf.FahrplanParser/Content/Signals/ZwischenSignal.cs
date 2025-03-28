using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Zsig
/// </summary>
public class ZwischenSignal : SignalContent
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

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;
		
		if (!TryParse(additionalText, "Zsig", "Z", out string speed, out string stationName))
			return false;

		content = new ZwischenSignal(stationName, speed);
		return true;
	}
}