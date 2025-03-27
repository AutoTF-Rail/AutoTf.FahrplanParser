namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Sbk
/// </summary>
public class SelbstBlockSignal : RowContent
{
	// TODO: Selbst blockvorsignal?
	public SelbstBlockSignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
	
	public override string GetPrint()
	{
		return $"Sbk {SignalNummer}";
	}
}