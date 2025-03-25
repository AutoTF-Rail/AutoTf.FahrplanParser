using System.Drawing;
using AutoTf.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

public class Parser
{
	private readonly Tesseract _engine;

	public Parser(Tesseract engine)
	{
		_engine = engine;
	}

	public string Date(Mat mat)
	{
		return ExtractTextClean(RegionMappings.Date, mat, _engine).Replace("\n", "");
	}

	public string Time(Mat mat)
	{
		return ExtractTextClean(RegionMappings.Time, mat, _engine).Replace("\n", "");
	}

	public string TrainNumber(Mat mat)
	{
		return ExtractTextClean(RegionMappings.TrainNumber, mat, _engine).Replace("\n", "");
	}

	public string PlanValid(Mat mat)
	{
		return ExtractTextClean(RegionMappings.PlanValidity, mat, _engine).Replace("\n", "");
	}

	public string Delay(Mat mat)
	{
		return ExtractTextClean(RegionMappings.Delay, mat, _engine).Replace("\n", "");
	}

	public string Hektometer(Mat mat, Rectangle row)
	{
		return ExtractTextClean(RegionMappings.Hektometer(row), mat, _engine).Replace("\n", "");
	}

	public string AdditionalText(Mat mat, Rectangle row)
	{
		return ExtractTextClean(RegionMappings.AdditionalText(row), mat, _engine).Replace("\n", "");
	}

	public string Arrival(Mat mat, Rectangle row)
	{
		return ExtractTextClean(RegionMappings.Arrival(row), mat, _engine).Replace("\n", "");
	}

	public string Departure(Mat mat, Rectangle row)
	{
		return ExtractTextClean(RegionMappings.Departure(row), mat, _engine).Replace("\n", "");
	}

	public string? Location(Mat mat)
	{
		// TODO: Does this maybe make a problem, if we are already on "page two" by location, so the point won't be on the first page?
		for (int i = 0; i < RegionMappings.LocationPoints.Count; i++)
		{
			Rectangle checkRoi = new Rectangle(RegionMappings.LocationPoints[i].X + 25, RegionMappings.LocationPoints[i].Y + 10, 6, 25);
			Mat checkMat = new Mat(mat, checkRoi);
					
			if(!checkMat.IsMoreBlackThanWhite())
				continue;

			return ExtractText(RegionMappings.LocationPointsHektometer[i], mat, _engine).TrimEnd();
		}

		return null;
	}

	private static string ExtractText(Rectangle roi, Mat mat, Tesseract engine)
	{
		using Mat roiMat = new Mat(mat, roi);
		using Pix pix = new Pix(roiMat);
		
		engine.SetImage(pix);
		
		return engine.GetUTF8Text().Trim();
	}

	private static string ExtractTextClean(Rectangle roi, Mat mat, Tesseract engine) => ExtractText(roi, mat, engine).Replace("\n", "");
}