using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content;

// Wenn es eine station gibt, ohne halt, aber mit abfahrt zeit (Abzweigungen mit abfahrt zeit werden hier nicht aufgelistet)
public class NoStopStation : RowContent
{
	// TODO: Save the departure time?
	private NoStopStation(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"{Name} - No stop";
	}

	public static bool TryParse(string additionalText, string arrival, string departure, out RowContent? content)
	{
		content = null;
		
		// TODO: Gibt es stationen die notiert sind, aber auch keine abfahrts zeit haben?
		if (string.IsNullOrWhiteSpace(additionalText) || string.IsNullOrWhiteSpace(departure) && !string.IsNullOrWhiteSpace(arrival))
			return false;

		content = new NoStopStation(additionalText);
		return true;
	}
}