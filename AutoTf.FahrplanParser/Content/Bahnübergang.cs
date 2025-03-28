namespace AutoTf.FahrplanParser.Content;

public class Bahnübergang : RowContent
{
	public Bahnübergang(string kilometer)
	{
		Kilometer = kilometer;
	}

	public string Kilometer { get; set; }
	
	public override string GetPrint()
	{
		return $"Bü km {Kilometer} ET";
	}
}