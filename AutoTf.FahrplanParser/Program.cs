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
			await ProcessFileAsync(file, fileIndex++, rows, speedChanges);
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

	private static async Task ProcessFileAsync(string file, int fileIndex, List<KeyValuePair<string, RowContent>> rows, List<KeyValuePair<string, string>> speedChanges)
	{
		await Task.Run(() =>
		{
			using Tesseract engine = new Tesseract("tessdata/", "deu", OcrEngineMode.Default);
				
			Mat mat = CvInvoke.Imread(file);
				
			if (fileIndex == 0)
			{
				Console.WriteLine($"Date: {ExtractText(RegionMappings.Date, mat, engine).Replace("\n", "")} - {ExtractText(RegionMappings.Time, mat, engine).Replace("\n", "")}");
			
				Console.WriteLine($"Train Number: {ExtractText(RegionMappings.TrainNumber, mat, engine)}");
				Console.WriteLine($"Plan is{(ExtractText(RegionMappings.PlanValidity, mat, engine).Contains("gültig") ? "" : " not")} valid.\n");
					
				Console.WriteLine($"Current delay: {ExtractText(RegionMappings.Delay, mat, engine).Replace("\n", "")}.");
					
				// Does this maybe make a problem, if we are already on "page two" by location, so the point won't be on the first page?

				for (int i = 0; i < RegionMappings.LocationPoints.Count; i++)
				{
					Rectangle checkRoi = new Rectangle(RegionMappings.LocationPoints[i].X + 25, RegionMappings.LocationPoints[i].Y + 10, 6, 25);
					Mat checkMat = new Mat(mat, checkRoi);
						
					if(!checkMat.IsMoreBlackThanWhite())
						continue;

					string location = ExtractText(RegionMappings.LocationPointsHektometer[i], mat, engine).TrimEnd();
					Console.WriteLine($"Estimated location: {location}.\n\n");
					break;
				}
			}

			List<Rectangle> rowsRoi = new List<Rectangle>(RegionMappings.Rows);
			rowsRoi.Reverse();

			List<RowContent> additionalContent = new List<RowContent>();
			
			string additionalSpeed = string.Empty;

			for (int i = 0; i < rowsRoi.Count; i++)
			{
				Rectangle row = rowsRoi[i];
				
				string hektometer = ExtractTextClean(RegionMappings.Hektometer(row), mat, engine);

				string additionalText = ExtractTextClean(RegionMappings.AdditionalText(row), mat, engine);
				
				string arrivalTime = ExtractTextClean(RegionMappings.Arrival(row), mat, engine);
				string departureTime = ExtractTextClean(RegionMappings.Departure(row), mat, engine);
				
				if (string.IsNullOrWhiteSpace(hektometer))
				{
					// Add the current info to the next hektometer we see
						
					// Does the last information even matter, or can we safely skip this?
					if(i == rowsRoi.Count)
						continue;

					string speedlimit = ExtractTextClean(RegionMappings.SpeedLimit(row), mat, engine);
					
					if (!string.IsNullOrWhiteSpace(speedlimit))
					{
						if (!mat.ContainsYellow(RegionMappings.YellowArea(row)))
							additionalSpeed = speedlimit;
					}

					RowContent? content = ResolveContent(additionalText, arrivalTime, departureTime);
					
					// If it's a station, we are probably better off if we figure out its hektometer, and put it into the seperate list? Just like additionalSpeed?
					if (content is Station station)
					{
						if (rows.Count != 0)
						{
							List<KeyValuePair<string, RowContent>> stations = rows.Where(x => x.Value is Station).ToList();

							content = stations.Any(x => x.Value is Station value && 
							                            value.Arrival == arrivalTime && value.Name == additionalText) ? null : station;
						}
						else
							content = station;
					}
					
					if(content != null)
						additionalContent.Add(content);
				}
				else
				{
					RowContent? content;

					string speedLimit;

					// TODO: Rather make a "starting speed limit"?
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

					content = ResolveContent(additionalText, arrivalTime, departureTime);

					if (content is Station station)
					{
						if (rows.Count != 0)
						{
							List<KeyValuePair<string, RowContent>> stations = rows.Where(x => x.Value is Station).ToList();

							content = stations.Any(x => x.Value is Station value && 
							                            value.Arrival == arrivalTime && value.Name == additionalText) ? null : station;
						}
						else
							content = station;
					}

					if (content == null)
						continue;

					rows.Add(new KeyValuePair<string, RowContent>(hektometer, content));
					rows.AddRange(additionalContent.Select(x => new KeyValuePair<string, RowContent>(hektometer, x)));
					
					additionalContent.Clear();
				}
			}
		});
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