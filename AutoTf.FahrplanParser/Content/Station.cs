namespace AutoTf.FahrplanParser.Content;

public class Station : RowContent
{
	public string Name { get; set; }
	public string Arrival { get; set; }
	public string Departure { get; set; }
	
	public override string GetPrint()
	{
		return $"{Name} Arrival: {Arrival} Departure: {Departure}";
	}
}