using AutoTf.FahrplanParser.Content.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Content.Icons;

// TODO: Rename? To: "Weichenbereich ende"? or so?
/// <summary>
/// ¥
/// </summary>
public class YenMarker : IconContent
{
	private const string FileName = "Icons/YenIcon.png";
	
	public override string GetPrint()
	{
		return "\u00a5";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}