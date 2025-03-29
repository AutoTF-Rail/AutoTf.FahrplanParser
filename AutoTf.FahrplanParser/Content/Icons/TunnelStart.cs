using AutoTf.FahrplanParser.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Icons;

/// <summary>
/// Icons/tunnelStartIcon.png
/// </summary>
public class TunnelStart : IconContent
{
	private const string FileName = "Icons/TunnelStartIcon.png";

	public TunnelStart(string name)
	{
		Name = name.Replace("-T", "");
	}
	
	public string Name { get; set; }
	
	public override string GetPrint()
	{
		return $"Tunnel {Name}-T";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}