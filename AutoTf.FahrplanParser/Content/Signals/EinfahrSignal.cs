namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Esig
/// </summary>
// TODO: Make like a attribute instead showing the meaning in a fahrplan?
public class EinfahrSignal : RowContent
{
	public EinfahrSignal(string stationName, string speed = "40")
	{
		Speed = speed;
		StationName = stationName;
	}
	
	// TODO: Validate that the only additional content can be a station name
	public string StationName { get; set; }

	// TODO: Nur bei Hp2(langsamfahrt): Ausfahrt mit 40, solange nicht größer definiert im Fahrplan 
	public string Speed { get; set; }
}