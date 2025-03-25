using System.Drawing;
using Emgu.CV;
using Emgu.CV.OCR;

internal static class Program
{
	private static Tesseract _engine = new Tesseract("tessdata/", "deu", OcrEngineMode.Default);
	public static void Main(string[] args)
	{
		Console.WriteLine("AutoTF Fahrplan Parser");

		foreach (string file in Directory.GetFiles("FahrplanData/"))
		{
			Console.WriteLine($"Reading {file}");
			
			Mat mat = CvInvoke.Imread(file);

			Rectangle trainNumRoi = new Rectangle(17, 11, 134, 44);
			Rectangle planValidityRoi = new Rectangle(348, 11, 259, 44);
			Rectangle dateRoi = new Rectangle(805, 11, 190, 44);
			Rectangle timeRoi = new Rectangle(1066, 11, 160, 44);
			Rectangle nextStopRoi = new Rectangle(1003, 69, 244, 29);
			Rectangle currSpeedLimitRoi = new Rectangle(124, 750, 76, 36);
		
			Console.WriteLine($"Started at {DateTime.Now.ToString("mm:ss.fff")}");
		
			Console.WriteLine($"Date: {ExtractText(dateRoi, mat)}");
			Console.WriteLine($"Time: {ExtractText(timeRoi, mat)}\n");
		
			Console.WriteLine($"Train Number: {ExtractText(trainNumRoi, mat)}");
			Console.WriteLine($"Plan is{(ExtractText(planValidityRoi, mat).Contains("gültig") ? "" : " not")} valid.");
			Console.WriteLine($"Next stop: {ExtractText(nextStopRoi, mat)}");
			Console.WriteLine($"Current speed limit: {ExtractText(currSpeedLimitRoi, mat)}");
		
			Console.WriteLine($"Finished at {DateTime.Now.ToString("mm:ss.fff")}");
		
			mat.Dispose();
		}
	}

	private static string ExtractText(Rectangle roi, Mat mat)
	{
		using Mat roiMat = new Mat(mat, roi);
		using Pix pix = new Pix(roiMat);
		
		_engine.SetImage(pix);
		
		return _engine.GetUTF8Text();
	}
}
