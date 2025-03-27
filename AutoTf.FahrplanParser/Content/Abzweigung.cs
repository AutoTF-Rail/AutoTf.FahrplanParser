namespace AutoTf.FahrplanParser.Content;

/// <summary>
/// Abzw
/// </summary>
public class Abzweigung : RowContent
{
	public Abzweigung(string name)
	{
		Name = name;
	}
	
	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"Abzw {Name}";
	}
}