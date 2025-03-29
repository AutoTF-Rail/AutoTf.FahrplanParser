using AutoTf.FahrplanParser.Content.Content.Base;
using Emgu.CV;

namespace AutoTf.FahrplanParser.Content.Content.Icons;

/// <summary>
/// Sideways T (Icons/StumpfgleisIcon.png)
/// </summary>
public class Stumpfgleis : IconContent
{
	private const string FileName = "Icons/StumpfgleisIcon.png";
	
	public override string GetPrint()
	{
		return "Stumpfgleis";
	}
	
	public static bool TryParseIcon(Mat area)
	{
		return TryParseIcon(FileName, area);
	}
}