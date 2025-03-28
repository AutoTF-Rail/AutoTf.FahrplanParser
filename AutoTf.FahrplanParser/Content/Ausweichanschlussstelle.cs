namespace AutoTf.FahrplanParser.Content;

/// <summary>
/// Awanst &lt;Name&gt;
/// </summary>
public class Ausweichanschlussstelle : RowContent
{
	public Ausweichanschlussstelle(string name)
	{
		Name = name;
	}

	public string Name { get; set; }

	public override string GetPrint()
	{
		return $"Awanst {Name}";
	}
}