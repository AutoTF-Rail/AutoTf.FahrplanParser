using AutoTf.FahrplanParser.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Icons;

public class LzbStart : IconContent
{
	private const string FileName = "Icons/LzbStartIcon.png";

	public override string GetPrint()
	{
		return "LZB Start";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}