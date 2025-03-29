using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content.Markers;

/// <summary>
/// LZB-Bk Num
/// </summary>
public class LzbBlock : RowContent
{
	private LzbBlock(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"LZB-Bk {SignalNummer}";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;

		if (!additionalText.Contains("LZB-Bk"))
			return false;

		additionalText = additionalText.Replace("LZB-Bk", "").Trim();

		content = new LzbBlock(additionalText);
		return true;
	}
}