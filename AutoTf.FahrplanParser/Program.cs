using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

internal static class Program
{
	public static async Task Main(string[] args)
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
				string speedlimit = ExtractTextClean(RegionMappings.SpeedLimit(row), mat, engine);
				
				if (!string.IsNullOrWhiteSpace(speedlimit))
				{
					if (!mat.ContainsYellow(RegionMappings.YellowArea(row)))
						additionalSpeed = speedlimit;
				}

				RowContent? content = ResolveContent(additionalText, arrivalTime, departureTime);
				
				content = CheckForDuplicateStation(content, arrivalTime, additionalText, rows);
				
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
					speedLimit = ExtractText(RegionMappings.SpeedLimit(row), mat, engine).Trim();
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

				RowContent? content = ResolveContent(additionalText, arrivalTime, departureTime);

				content = CheckForDuplicateStation(content, arrivalTime, additionalText, rows);

				if (content == null)
					continue;

				rows.Add(new KeyValuePair<string, RowContent>(hektometer, content));
				
			}
		}
	}

	private static RowContent? CheckForDuplicateStation(RowContent content, string arrivalTime, string stationName, List<KeyValuePair<string, RowContent>> knownStations)
	{
		if (content is not Station station)
			return null;

		if (knownStations.Count == 0) 
			return station;
		
		List<KeyValuePair<string, RowContent>> stations = knownStations.Where(x => x.Value is Station).ToList();

		return stations.Any(x => x.Value is Station value && 
		                         value.Arrival == arrivalTime && value.Name == stationName) ? null : station;

	}

	private static RowContent ResolveContent(string additionalText, string arrivalTime, string departureTime)
	{
		if (!string.IsNullOrWhiteSpace(arrivalTime) && !string.IsNullOrWhiteSpace(departureTime))
		{
			return new Station()
			{
				Name = additionalText,
				Arrival = arrivalTime,
				Departure = departureTime
			};
		}

		if (additionalText.Contains("GSM-R"))
		{
			return new GSMRInfo(additionalText.Trim());
		}

		if (additionalText.Contains("Asig"))
		{
			return new Asig();
		}

		// TODO: Continue cases
		return new UnknownContent(additionalText);
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