using AutoTf.FahrplanParser.Content.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Content.Icons.Tunnels;

/// <summary>
/// TODO: Always empty addtionaltext?
/// </summary>
public class TunnelEnd : TunnelContent
{
	private const string FileName = "Icons/TunnelEndIcon.png";
	
	public override string GetPrint()
	{
		return "Tunnel Ende";
	}

	public override TunnelType GetTunnelType()
	{
		return TunnelType.End;
	}

	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}