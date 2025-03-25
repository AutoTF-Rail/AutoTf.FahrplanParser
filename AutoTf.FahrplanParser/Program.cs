using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;

internal static class Program
{
	private static Tesseract _engine = new Tesseract("tessdata/", "deu", OcrEngineMode.Default);
	public static void Main(string[] args)
	{
		Console.WriteLine("AutoTF Fahrplan Parser");
		Console.WriteLine($"Started at {DateTime.Now.ToString("mm:ss.fff")}");

		int fileIndex = 0;
		foreach (string file in Directory.GetFiles("FahrplanData/"))
		{
			Console.WriteLine($"Reading {file}");
			Mat mat = CvInvoke.Imread(file);
			
			if (fileIndex == 0)
			{
				Rectangle trainNumRoi = new Rectangle(17, 11, 134, 44);
				Rectangle planValidityRoi = new Rectangle(348, 11, 259, 44);
				Rectangle dateRoi = new Rectangle(805, 11, 190, 44);
				Rectangle timeRoi = new Rectangle(1066, 11, 160, 44);
				
				Console.WriteLine($"Date: {ExtractText(dateRoi, mat)}");
				Console.WriteLine($"Time: {ExtractText(timeRoi, mat)}\n");
		
				Console.WriteLine($"Train Number: {ExtractText(trainNumRoi, mat)}");
				Console.WriteLine($"Plan is{(ExtractText(planValidityRoi, mat).Contains("gültig") ? "" : " not")} valid.");
				
				// Does this maybe make a problem, if we are already on "page two" by location, so the point won't be on the first page?

				List<Rectangle> locationPointRois = new List<Rectangle>()
				{
					new Rectangle(190, 432, 37, 43),
					new Rectangle(190, 477, 37, 43),
					new Rectangle(190, 523, 37, 43),
					new Rectangle(190, 568, 37, 43),
					new Rectangle(190, 614, 37, 43),
					new Rectangle(190, 659, 37, 43),
					new Rectangle(190, 705, 37, 43)
				};
				
				List<Rectangle> locationPointHektometerRois = new List<Rectangle>()
				{
					new Rectangle(259, 432, 127, 43),
					new Rectangle(259, 477, 127, 43),
					new Rectangle(259, 523, 127, 43),
					new Rectangle(259, 568, 127, 43),
					new Rectangle(259, 614, 127, 43),
					new Rectangle(259, 659, 127, 43),
					new Rectangle(259, 705, 127, 43)
				};

				for (int i = 0; i < locationPointRois.Count; i++)
				{
					Rectangle checkRoi = new Rectangle(locationPointRois[i].X + 25, locationPointRois[i].Y + 10, 6, 25);
					Mat checkMat = new Mat(mat, checkRoi);
					
					if(!IsMoreBlackThanWhite(checkMat))
						continue;

					string location = ExtractText(locationPointHektometerRois[i], mat);
					Console.WriteLine($"Found location marker at {location}.");
					break;
				}
			}

			fileIndex++;

			Rectangle nextStopRoi = new Rectangle(726, 67, 524, 33);
			Rectangle currSpeedLimitRoi = new Rectangle(124, 750, 76, 36);
			
			string nextStop = ExtractText(nextStopRoi, mat);
			string newNextStop = string.Empty;
			
			if(!string.IsNullOrEmpty(nextStop))
				newNextStop = nextStop.Split("alt: ")[1];
			
			Console.WriteLine($"Next stop: {newNextStop}");
			Console.WriteLine($"Current speed limit: {ExtractText(currSpeedLimitRoi, mat)}");
		
			Console.WriteLine($"Finished at {DateTime.Now.ToString("mm:ss.fff")}");
		
			mat.Dispose();
		}
		
		_engine.Dispose();
	}
	
	static bool IsMoreBlackThanWhite(Mat img)
	{
		Mat binaryImg = new Mat();
		CvInvoke.CvtColor(img, binaryImg, ColorConversion.Bgr2Gray);
		CvInvoke.Threshold(binaryImg, binaryImg, 128, 255, ThresholdType.Binary);

		int whitePixels = CvInvoke.CountNonZero(binaryImg);
		int totalPixels = img.Rows * img.Cols;
		int blackPixels = totalPixels - whitePixels;

		binaryImg.Dispose();
		return blackPixels > whitePixels;
	}

	private static string ExtractText(Rectangle roi, Mat mat)
	{
		using Mat roiMat = new Mat(mat, roi);
		using Pix pix = new Pix(roiMat);
		
		_engine.SetImage(pix);
		
		return _engine.GetUTF8Text();
	}
}
