namespace AutoTf.FahrplanParser.Content.Signals.Vorsignal;

/// <summary>
/// Avsig
/// </summary>
public class AusfahrVorsignal : RowContent
{
	public AusfahrVorsignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"Bkvsig {SignalNummer}";
	}
}