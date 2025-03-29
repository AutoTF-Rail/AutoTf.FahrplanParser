using AutoTf.FahrplanParser.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Icons;

public class TunnelEnd : IconContent
{
	private const string FileName = "Icons/TunnelEndIcon.png";
	
	public override string GetPrint()
	{
		return "Tunnel Ende";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}