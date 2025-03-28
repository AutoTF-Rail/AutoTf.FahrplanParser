using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Base;
using AutoTf.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

internal static class Program
{
	// TODO: "Prepare" the images by making the dark writings a bit more stronger to smooth out errors due to bad quality?
	public static Task Main(string[] args)
	{
		Console.WriteLine("AutoTF Fahrplan Parser");
		Console.WriteLine($"Started at {DateTime.Now.ToString("mm:ss.fff")}");
		
		// We can't just go by hektometer keys, because hektometers might repeat
		List<KeyValuePair<string, string>> speedChanges = new List<KeyValuePair<string, string>>();

		List<KeyValuePair<string, RowContent>> rows = new List<KeyValuePair<string, RowContent>>();

		int fileIndex = 0;
		List<string> files = Directory.GetFiles("FahrplanData/").ToList();
		files.Sort();
		
		foreach (string file in files)
		{
			ProcessFileAsync(file, fileIndex++, rows, speedChanges);
		}
		
		Console.WriteLine($"Finished at {DateTime.Now.ToString("mm:ss.fff")}");
		
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

		return Task.CompletedTask;
	}

	private static void ProcessFileAsync(string file, int fileIndex, List<KeyValuePair<string, RowContent>> rows, List<KeyValuePair<string, string>> speedChanges)
	{
		using Tesseract engine = new Tesseract("tessdata/", "deu", OcrEngineMode.Default);
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

		List<Rectangle> rowsRoi = [..RegionMappings.Rows];
		rowsRoi.Reverse();

		List<RowContent> additionalContent = new List<RowContent>();
		
		string additionalSpeed = string.Empty;

		for (int i = 0; i < rowsRoi.Count; i++)
		{
			Rectangle row = rowsRoi[i];

			string hektometer = parser.Hektometer(mat, row);
			string additionalText = parser.AdditionalText(mat, row);
			string arrivalTime = parser.Arrival(mat, row);
			string departureTime = parser.Departure(mat, row);
			
			// If we don't have a hektometer, we will add it's info to the next one
			if (string.IsNullOrWhiteSpace(hektometer))
			{
				// TODO: Does this case ever happen? Having a speed limit change at a unknown hektometer? If not, this can be removed
				string speedlimit = parser.SpeedLimit(mat, row);
				
				if (!string.IsNullOrWhiteSpace(speedlimit))
				{
					if (!mat.ContainsYellow(RegionMappings.YellowArea(row)))
						additionalSpeed = speedlimit;
				}

				RowContent? content = parser.ResolveContent(additionalText, arrivalTime, departureTime);
				
				content = parser.CheckForDuplicateContent(content, hektometer, rows);
				
				if(content == null)
					continue;
					
				additionalContent.Add(content);
			}
			else
			{
				rows.AddRange(additionalContent.Select(x => new KeyValuePair<string, RowContent>(hektometer, x)));
				additionalContent.Clear();
				
				string speedLimit;

				// TODO: We could remove this, if we know the answer to the TODO above
				if (additionalSpeed != string.Empty)
				{
					speedLimit = additionalSpeed;
					additionalSpeed = string.Empty;
				}
				else
				{
					speedLimit = parser.SpeedLimit(mat, row);
				}
					
				if (!string.IsNullOrWhiteSpace(speedLimit))
				{
					// Skip if yellow (repeating)
					if (!mat.ContainsYellow(RegionMappings.YellowArea(row)))
					{
						// Skip if already contained
						if (speedChanges.Any())
						{
							if(speedChanges.TakeLast(3).All(x => x.Key != hektometer))
								speedChanges.Add(new KeyValuePair<string, string>(hektometer, speedLimit));
						}
						else
							speedChanges.Add(new KeyValuePair<string, string>(hektometer, speedLimit));
					}
				}

				RowContent? content = null;
				
				if (string.IsNullOrWhiteSpace(additionalText))
				{
					if (parser.TryParseIcon(mat, row, out RowContent? result))
						content = result;
				}
				else
				{
					content = parser.ResolveContent(additionalText, arrivalTime, departureTime);
					
					// No need for a null check, since the method does it
					// if(!string.IsNullOrWhiteSpace(arrivalTime))
					// 	content = parser.CheckForDuplicateStation(content, arrivalTime, additionalText, rows);
				}
				if(content != null)
					content = parser.CheckForDuplicateContent(content, hektometer, rows);

				if (content == null)
					continue;

				rows.Add(new KeyValuePair<string, RowContent>(hektometer, content));
			}
		}
		mat.Dispose();
	}
}