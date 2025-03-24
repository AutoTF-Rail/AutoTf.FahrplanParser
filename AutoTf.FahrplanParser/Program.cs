using Emgu.CV;
using Emgu.CV.OCR;

internal static class Program
{
	public static void Main(string[] args)
	{
		while (true)
		{
			Console.WriteLine("AutoTF Fahrplan Parser");
			Console.Write("File path:");
			string? path = Console.ReadLine();
		
			if (path == null)
			{
				Console.WriteLine("Please enter a valid path.");
				continue;
			}

			if (!File.Exists(path))
			{
				Console.WriteLine("Please enter a valid path.");
				continue;
			}
			
			using Tesseract tes = new Tesseract("tessdata/", "deu", OcrEngineMode.Default);
			using Pix pic = new Pix(CvInvoke.Imread("FahrplanExample.png"));

			string text = tes.GetOsdText();
			
			Console.WriteLine(text);
			
		}
	}
}
