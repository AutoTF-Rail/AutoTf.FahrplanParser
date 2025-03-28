using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content.Markers;

/// <summary>
/// Anst &lt;Name&gt;
/// </summary>
public class Anschlussstelle : RowContent
{
	private const string Tag = "Anst";

	private Anschlussstelle(string name)
	{
		Name = name;
	}

	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"{Tag} {Name}";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;

		if (!additionalText.Contains(Tag))
			return false;

		additionalText = additionalText.Replace(Tag, "").Trim();

		content = new Anschlussstelle(additionalText);
		return true;
	}
}