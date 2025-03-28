namespace AutoTf.FahrplanParser.Content;

/// <summary>
/// Anst &lt;Name&gt;
/// </summary>
public class Anschlussstelle : RowContent
{
	public Anschlussstelle(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"Anst {Name}";
	}
}