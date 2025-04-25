using AutoTf.FahrplanParser.Content.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Content.Icons.Tunnels;

/// <summary>
/// Icon representing the end of a tunnel
/// <remarks>TODO: It could be that there is additinal text along side this. e.g. ZF change</remarks>
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