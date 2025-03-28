using AutoTf.FahrplanParser.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Icons;

public class LzbEnd : IconContent
{
	private const string FileName = "Icons/LzbEndeIcon.png";
	
	public override string GetPrint()
	{
		return "LZB Ende";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}