using AutoTf.FahrplanParser.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Icons;

public class TunnelPart : IconContent
{
	private const string FileName = "Icons/TunnelPartIcon.png";
	
	public override string GetPrint()
	{
		return "Tunnel Part";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}