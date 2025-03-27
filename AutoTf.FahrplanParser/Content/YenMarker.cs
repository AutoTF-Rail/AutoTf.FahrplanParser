namespace AutoTf.FahrplanParser.Content;

// TODO: Rename? To: "Weichenbereich ende"? or so?
/// <summary>
/// ¥
/// </summary>
public class YenMarker : RowContent
{
	public override string GetPrint()
	{
		return "\u00a5";
	}
}