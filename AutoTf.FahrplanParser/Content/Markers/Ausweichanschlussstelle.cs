using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content.Markers;

/// <summary>
/// Awanst &lt;Name&gt;
/// </summary>
public class Ausweichanschlussstelle : RowContent
{
	private const string Tag = "Awanst";

	private Ausweichanschlussstelle(string name)
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

		content = new Ausweichanschlussstelle(additionalText);
		return true;
	}
}