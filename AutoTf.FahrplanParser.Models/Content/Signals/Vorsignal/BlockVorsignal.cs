using AutoTf.FahrplanParser.Content.Content.Base;

namespace AutoTf.FahrplanParser.Content.Content.Signals.Vorsignal;

/// <summary>
/// Bkvsig
/// </summary>
public class BlockVorsignal : SignalContent
{
	// TODO: Verkürzungen mit "v" gekennzeichnet hinzufügen
	private BlockVorsignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"Bkvsig {SignalNummer}";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;
		
		if (!TryParse(additionalText, "Bkvsig", "U", out string _, out string signalNummer))
			return false;

		content = new BlockVorsignal(signalNummer);
		return true;
	}
}