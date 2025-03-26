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

	private static string ExtractTextClean(Rectangle roi, Mat mat, Tesseract engine) => ExtractText(roi, mat, engine).Replace("\n", "");

	private static string ExtractText(Rectangle roi, Mat mat, Tesseract engine)
	{
		using Mat roiMat = new Mat(mat, roi);
		using Pix pix = new Pix(roiMat);
		
		engine.SetImage(pix);
		
		return engine.GetUTF8Text().Trim();
	}

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

	public bool IsLzbStart(Mat mat, Rectangle row)
	{
		Mat lzbStartIcon = CvInvoke.Imread("Icons/LzbStartIcon.png", ImreadModes.Grayscale);
		
		Rectangle roi = new Rectangle(row.X + 386, row.Y, 60, 44);
		Mat area = new Mat(mat, roi);
		CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);
		
		if (area.NumberOfChannels != 1)
			CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);
		
		if (lzbStartIcon.NumberOfChannels != 1)
			CvInvoke.CvtColor(lzbStartIcon, lzbStartIcon, ColorConversion.Bgr2Gray);
		
		int resultCols = area.Cols - lzbStartIcon.Cols + 1;
		int resultRows = area.Rows - lzbStartIcon.Rows + 1;
		Mat result = new Mat(resultRows, resultCols, DepthType.Cv32F, 1);
		
		CvInvoke.MatchTemplate(area, lzbStartIcon, result, TemplateMatchingType.CcoeffNormed);
		
		double minVal = 0;
		double maxVal = 0;
		Point minLoc = Point.Empty;
		Point maxLoc = Point.Empty;
		
		CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

		double threshold = 0.8;
		Console.WriteLine("Maxval: " + maxVal);

		return maxVal >= threshold;
	}
}