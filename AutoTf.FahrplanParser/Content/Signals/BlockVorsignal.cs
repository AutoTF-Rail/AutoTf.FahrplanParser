namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Bkvsig
/// </summary>
public class BlockVorsignal : RowContent
{
	public BlockVorsignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
}