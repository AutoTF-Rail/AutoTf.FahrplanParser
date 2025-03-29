using AutoTf.FahrplanParser.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Icons;

// TODO: Remove this? Does it even matter? We know when a tunnel starts and ends, so we can "imagine" where this is? It's sort of hard to detect it cause of the middle line
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