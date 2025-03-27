namespace AutoTf.FahrplanParser.Content;

// Wenn es eine station gibt, ohne halt, aber mit abfahrt zeit (Abzweigungen mit abfahrt zeit werden hier nicht aufgelistet)
public class NoStopStation : RowContent
{
	// TODO: Save the departure time?
	public NoStopStation(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"{Name} - No stop";
	}
}