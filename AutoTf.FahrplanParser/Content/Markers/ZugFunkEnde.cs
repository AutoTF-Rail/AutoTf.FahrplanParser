using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content.Markers;

public class ZugFunkEnde : RowContent
{
	public override string GetPrint()
	{
		return "- ZF ENDE -";
	}
	// There is no TryParse here, because it's handled by ZugFunk.cs
}