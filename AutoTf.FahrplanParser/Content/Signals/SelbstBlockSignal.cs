namespace AutoTf.FahrplanParser.Content.Signals;

/// <summary>
/// Sbk
/// </summary>
public class SelbstBlockSignal : RowContent
{
	public SelbstBlockSignal(string signalNummer)
	{
		SignalNummer = signalNummer;
	}

	public string SignalNummer { get; set; }
}