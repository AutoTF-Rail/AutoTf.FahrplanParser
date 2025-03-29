using AutoTf.FahrplanParser.Content.Content.Base;

namespace AutoTf.FahrplanParser.Content.Content.Markers;

public class ZugFunkEnde : RowContent
{
	public override string GetPrint()
	{
		return "- ZF ENDE -";
	}
	// There is no TryParse here, because it's handled by ZugFunk.cs
}