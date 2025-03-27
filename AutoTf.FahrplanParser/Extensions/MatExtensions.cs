using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace AutoTf.FahrplanParser.Extensions;

public static class MatExtensions
{
	public static bool IsMoreBlackThanWhite(this Mat img)
	{
		Mat binaryImg = new Mat();
		CvInvoke.Threshold(img, binaryImg, 128, 255, ThresholdType.Binary);

		int whitePixels = CvInvoke.CountNonZero(binaryImg);
		int totalPixels = img.Rows * img.Cols;
		int blackPixels = totalPixels - whitePixels;
		
		binaryImg.Dispose();
		
		return blackPixels > whitePixels;
	}

	public static bool ContainsYellow(this Mat mat, Rectangle roi)
	{
		Console.WriteLine("Starting yellow check");
		Mat roiMat = new Mat(mat, roi);

		Mat hsv = new Mat();
		CvInvoke.CvtColor(roiMat, hsv, ColorConversion.Bgr2Hsv);

		ScalarArray lowerYellow = new ScalarArray(new MCvScalar(25, 100, 100));
		ScalarArray upperYellow = new ScalarArray(new MCvScalar(35, 255, 255));

		Mat mask = new Mat();
		CvInvoke.InRange(hsv, lowerYellow, upperYellow, mask);

		int nonZeroCount = CvInvoke.CountNonZero(mask);

		int threshold = roi.Width * roi.Height / 20;

		Console.WriteLine("Finished yellow check");
		return nonZeroCount > threshold;
	}
}