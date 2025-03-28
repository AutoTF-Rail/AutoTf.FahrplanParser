namespace AutoTf.FahrplanParser.Content;

/// <summary>
/// Üst &lt;Name&gt;
/// </summary>
public class Überleitstelle : RowContent
{
	public Überleitstelle(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"Üst {Name}";
	}
}