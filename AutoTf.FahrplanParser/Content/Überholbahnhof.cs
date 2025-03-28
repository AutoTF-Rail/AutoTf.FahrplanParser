namespace AutoTf.FahrplanParser.Content;

/// <summary>
/// Üst &lt;Name&gt;
/// </summary>
public class Überholbahnhof : RowContent
{
	public Überholbahnhof(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"Übf {Name}";
	}
}