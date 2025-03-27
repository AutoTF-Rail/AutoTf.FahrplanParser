namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Bkvsig
/// </summary>
public class BlockVorsignal : RowContent
{
	// TODO: Verkürzungen mit "v" gekennzeichnet hinzufügen
	public BlockVorsignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"Bkvsig {SignalNummer}";
	}
}