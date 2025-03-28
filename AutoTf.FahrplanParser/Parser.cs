using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

public class Parser : ParserBase
{
	public Parser(Tesseract engine) : base(engine) { }

	public string Date(Mat mat) => ExtractTextClean(RegionMappings.Date, mat, Engine).Replace("\n", "");

	public string Time(Mat mat) => ExtractTextClean(RegionMappings.Time, mat, Engine).Replace("\n", "");

	public string TrainNumber(Mat mat) => ExtractTextClean(RegionMappings.TrainNumber, mat, Engine).Replace("\n", "");

	public string PlanValid(Mat mat) => ExtractTextClean(RegionMappings.PlanValidity, mat, Engine).Replace("\n", "");

	public string Delay(Mat mat) => ExtractTextClean(RegionMappings.Delay, mat, Engine).Replace("\n", "");

	public string Hektometer(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.Hektometer(row), mat, Engine).Replace("\n", "");

	public string AdditionalText(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.AdditionalText(row), mat, Engine).Replace("\n", "");

	public string Arrival(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.Arrival(row), mat, Engine).Replace("\n", "");

	public string Departure(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.Departure(row), mat, Engine).Replace("\n", "");

	public string SpeedLimit(Mat mat, Rectangle row) => ExtractTextClean(RegionMappings.SpeedLimit(row), mat, Engine).Replace("\n", "");

	public string? Location(Mat mat)
	{
		// TODO: Does this maybe make a problem, if we are already on "page two" by location, so the point won't be on the first page?
		for (int i = 0; i < RegionMappings.LocationPoints.Count; i++)
		{
			Rectangle checkRoi = new Rectangle(RegionMappings.LocationPoints[i].X + 25, RegionMappings.LocationPoints[i].Y + 10, 6, 25);
			Mat checkMat = new Mat(mat, checkRoi);
					
			if(!checkMat.IsMoreBlackThanWhite())
				continue;

			return ExtractText(RegionMappings.LocationPointsHektometer[i], mat, Engine).TrimEnd();
		}

		return null;
	}

	public bool TryParseIcon(Mat mat, Rectangle row, out RowContent? content)
	{
		content = null;
		
		try
		{
			Mat iconArea = GetIconArea(mat, row);

			if (LzbStart.TryParseIcon(iconArea))
				content = new LzbStart();
			else if (LzbEnd.TryParseIcon(iconArea))
				content = new LzbEnd();
			else if (YenMarker.TryParseIcon(iconArea))
				content = new YenMarker();
			else if (Stumpfgleis.TryParseIcon(iconArea))
				content = new Stumpfgleis();

			iconArea.Dispose();

			return content == null;
		}
		catch
		{
			// TODO: log?
			return false;
		}
	}
}