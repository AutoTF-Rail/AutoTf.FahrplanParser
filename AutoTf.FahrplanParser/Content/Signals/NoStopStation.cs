namespace AutoTf.FahrplanParser.Content.Signals;

// Wenn es eine station gibt, ohne halt, aber mit abfahrt zeit (Abzweigungen mit abfahrt zeit werden hier nicht aufgelistet)
public class NoStopStation : RowContent
{
	public NoStopStation(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
}