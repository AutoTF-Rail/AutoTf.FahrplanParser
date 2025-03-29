using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Content;
using AutoTf.FahrplanParser.Content.Content.Base;
using AutoTf.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser.Demo;

internal static class Program
{
	// TODO: "Prepare" the images by making the dark writings a bit more stronger to smooth out errors due to bad quality?
	// TODO: Resizing the files to respective sizes to the ROI's? Or resize the ROI to the image?
	public static Task Main(string[] args)
	{
		Console.WriteLine("AutoTF Fahrplan Parser");
		Console.WriteLine($"Started at {DateTime.Now.ToString("mm:ss.fff")}");
		
		// We can't just go by hektometer keys, because hektometers might repeat
		List<KeyValuePair<string, string>> speedChanges = new List<KeyValuePair<string, string>>();

		List<KeyValuePair<string, RowContent>> rows = new List<KeyValuePair<string, RowContent>>();

		ProcessFolder("ExampleOne", ref  rows, ref speedChanges);
		ProcessFolder("ExampleTwo", ref rows, ref speedChanges);
		
		PrintResults(speedChanges, rows);

		return Task.CompletedTask;
	}

	private static void ProcessFolder(string folderName, ref List<KeyValuePair<string, RowContent>> rows, ref List<KeyValuePair<string, string>> speedChanges)
	{
		int fileIndex = 0;
		List<string> files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "FahrplanData", folderName)).ToList();
		files.Sort();
		
		foreach (string file in files)
		{
			ProcessFileAsync(file, fileIndex++, ref rows, ref speedChanges);
		}
		
		Console.WriteLine($"Finished at {DateTime.Now:mm:ss.fff}");
	}

	private static void PrintResults(List<KeyValuePair<string, string>> speedChanges, List<KeyValuePair<string, RowContent>> rows)
	{
		Console.WriteLine(Environment.NewLine + Environment.NewLine);

		foreach (KeyValuePair<string,string> speedChange in speedChanges)
		{
			Console.WriteLine($"[{speedChange.Key}] Speed change {speedChange.Value}");
		}
		
		foreach (KeyValuePair<string, RowContent> station in rows.Where(x => x.Value is Station))
		{
			Station stationVar = (Station)station.Value;
			Console.WriteLine($"[{station.Key}] Arrive at {stationVar.Arrival} and depart at {stationVar.Departure} from {stationVar.Name}.");
		}
		
		Console.WriteLine(Environment.NewLine + Environment.NewLine);
		
		foreach (KeyValuePair<string,RowContent> row in rows)
		{
			string content = row.Value.GetPrint();
			
			Console.WriteLine($"[{row.Key}] {content}");
		}
		
		Console.WriteLine(Environment.NewLine + Environment.NewLine);
		Console.WriteLine("Unknown:");
		
		foreach (KeyValuePair<string,RowContent> row in rows.Where(x => x.Value is UnknownContent))
		{
			string content = row.Value.GetPrint();
			
			Console.WriteLine($"[{row.Key}] {content}");
		}
	}

	private static void ProcessFileAsync(string file, int fileIndex, ref List<KeyValuePair<string, RowContent>> rows, ref List<KeyValuePair<string, string>> speedChanges)
	{
		using Tesseract engine = new Tesseract(Path.Combine(AppContext.BaseDirectory, "tessdata"), "deu", OcrEngineMode.Default);
		Parser parser = new Parser(engine);
		
		Mat mat = CvInvoke.Imread(file);
		
		if (fileIndex == 0)
		{
			Console.WriteLine($"Date: {parser.Date(mat)} - {parser.Time(mat)}");
			Console.WriteLine($"Train Number: {parser.TrainNumber(mat)}");
			Console.WriteLine($"Plan is{(parser.PlanValid(mat).Contains("gültig") ? "" : " not")} valid.\n");
			Console.WriteLine($"Current delay: {parser.Delay(mat)}.");
				
			string? location = parser.Location(mat);
			if (location == null)
				Console.WriteLine("Could not find location point.");
			else 
				Console.WriteLine($"Estimated location: {location}.\n\n");
		}
		
		parser.ReadPage(parser, mat, ref rows, ref speedChanges);
		
		mat.Dispose();
	}
}