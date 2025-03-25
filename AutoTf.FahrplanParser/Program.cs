using System.Drawing;
using AutoTf.FahrplanParser;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;

internal static class Program
{
	private static Tesseract _engine = new Tesseract("tessdata/", "deu", OcrEngineMode.Default);
	public static void Main(string[] args)
	{
		Console.WriteLine("AutoTF Fahrplan Parser");
		Console.WriteLine($"Started at {DateTime.Now.ToString("mm:ss.fff")}");
		
		// We can't just go by hektometer keys, because hektometers might repeat
		List<KeyValuePair<string, string>> speedChanges = new List<KeyValuePair<string, string>>();

		Dictionary<string, RowContent> rows = new Dictionary<string, RowContent>();

		int fileIndex = 0;
		List<string> files = Directory.GetFiles("FahrplanData/").ToList();
		files.Sort();
		foreach (string file in files)
		{
			Mat mat = CvInvoke.Imread(file);
			
			if (fileIndex == 0)
			{
				Rectangle trainNumRoi = new Rectangle(17, 11, 134, 44);
				Rectangle planValidityRoi = new Rectangle(348, 11, 259, 44);
				Rectangle dateRoi = new Rectangle(805, 11, 190, 44);
				Rectangle timeRoi = new Rectangle(1066, 11, 160, 44);
				
				Rectangle delayRoi = new Rectangle(696, 812, 134, 30);
				
				Console.WriteLine($"Date: {ExtractText(dateRoi, mat).Replace("\n", "")} - {ExtractText(timeRoi, mat).Replace("\n", "")}");
		
				Console.WriteLine($"Train Number: {ExtractText(trainNumRoi, mat)}");
				Console.WriteLine($"Plan is{(ExtractText(planValidityRoi, mat).Contains("gültig") ? "" : " not")} valid.\n");
				
				Console.WriteLine($"Current delay: {ExtractText(delayRoi, mat).Replace("\n", "")}.");
				
				
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

					string location = ExtractText(locationPointHektometerRois[i], mat).TrimEnd();
					Console.WriteLine($"Estimated location: {location}.\n\n");
					break;
				}
			}

			fileIndex++;

			List<Rectangle> rowsRoi = new List<Rectangle>()
			{
				new Rectangle(85, 109, 1165, 44),
				new Rectangle(85, 155, 1165, 44),
				new Rectangle(85, 201, 1165, 44),
				new Rectangle(85, 248, 1165, 44),
				new Rectangle(85, 294, 1165, 44),
				new Rectangle(85, 340, 1165, 44),
				new Rectangle(85, 385, 1165, 44),
				new Rectangle(85, 432, 1165, 44),
				new Rectangle(85, 477, 1165, 44),
				new Rectangle(85, 523, 1165, 44),
				new Rectangle(85, 568, 1165, 44),
				new Rectangle(85, 614, 1165, 44),
				new Rectangle(85, 659, 1165, 44),
				new Rectangle(85, 705, 1165, 44),
				new Rectangle(85, 750, 1165, 44),
			};

			rowsRoi.Reverse();

			List<RowContent> additionalContent = new List<RowContent>();
			string additionalSpeed = string.Empty;

			for (int i = 0; i < rowsRoi.Count; i++)
			{
				Rectangle row = rowsRoi[i];
				Rectangle hektoRoi = new Rectangle(row.X + 173, row.Y, 126, 44);
				string hektoMeter = ExtractText(hektoRoi, mat).Replace("\n", "");
				
				Rectangle arrivalRoi = new Rectangle(row.X + 865, row.Y, 155, 44);
				Rectangle departureRoi = new Rectangle(row.X + 1026, row.Y, 140, 44);

				string arrivalTime = ExtractText(arrivalRoi, mat).Replace("\n", "").Trim();
				string departureTime = ExtractText(departureRoi, mat).Replace("\n", "").Trim();

				Rectangle additionalTextRoi = new Rectangle(row.X + 377, row.Y, 474, 44);
				string additionalText = ExtractText(additionalTextRoi, mat).Trim();
				
				if (string.IsNullOrWhiteSpace(hektoMeter))
				{
					// Add the current info to the next hektometer we see
					
					// Does the last information even matter, or can we safely skip this?
					if(i == rowsRoi.Count)
						continue;

					Rectangle speedLimitRoi = new Rectangle(row.X + 50, row.Y, 59, 44);

					string speedlimit = ExtractText(speedLimitRoi, mat);
					if (!string.IsNullOrWhiteSpace(speedlimit))
					{
						additionalSpeed = speedlimit.Trim();
					}

					
					if (!string.IsNullOrWhiteSpace(arrivalTime) && !string.IsNullOrWhiteSpace(departureTime))
					{
						additionalContent.Add(new Station()
						{
							Name = additionalText,
							Arrival = arrivalTime,
							Departure = departureTime
						});
						
						Console.WriteLine($"Added station {additionalText} at {hektoMeter}.");
					}
					else if (additionalText.Contains("GSM-R"))
					{
						additionalContent.Add(new GSMRInfo(additionalText));
					}
					else if (additionalText.Contains("Asig"))
					{
						additionalContent.Add(new Asig());
					}
					else
					{
						// TODO: Continue cases
						additionalContent.Add(new UnknownContent(additionalText));
					}
					continue;
				}
				else
				{
					RowContent? content = null;

					string speedLimit;

					// TODO: Rather make a "starting speed limit"?
					if (additionalSpeed != string.Empty)
					{
						speedLimit = additionalSpeed;
						additionalSpeed = string.Empty;
					}
					else
					{
						Rectangle speedLimitRoi = new Rectangle(row.X + 50, row.Y, 59, 44);
						speedLimit = ExtractText(speedLimitRoi, mat).Trim();
					}
					
					if (!string.IsNullOrWhiteSpace(speedLimit))
					{
						Rectangle yellowRoi = new Rectangle(row.X + 74, row.Y, 35, 9);
						
						if (!ContainsYellow(yellowRoi, mat))
						{
							// Skip if yellow (repeating)
							// Skip if already contained
							if (speedChanges.Any())
							{
								if(speedChanges.TakeLast(3).All(x => x.Key != hektoMeter))
									speedChanges.Add(new KeyValuePair<string, string>(hektoMeter, speedLimit));
							}
							else
								speedChanges.Add(new KeyValuePair<string, string>(hektoMeter, speedLimit));
						}

					}
					
					if (!string.IsNullOrWhiteSpace(arrivalTime) && !string.IsNullOrWhiteSpace(departureTime))
					{
						Station station = new Station()
						{
							Name = additionalText,
							Arrival = arrivalTime,
							Departure = departureTime,
							AdditionalContent = additionalContent
						};
						
						if (rows.Any())
						{
							List<KeyValuePair<string, RowContent>> stations = rows.Where(x => x.Value is Station).ToList();
							if(!stations.All(x => ((Station)x.Value).Arrival == arrivalTime && ((Station)x.Value).Name == additionalText))
								content = station;
						}
						else
							content = station;
						
						Console.WriteLine($"Added station {additionalText.Trim()} at {hektoMeter}. Arrival {arrivalTime} Depature: {departureTime}");
					}
					else if (additionalText.Contains("GSM-R"))
					{
						content = new GSMRInfo(additionalText.Trim())
						{
							AdditionalContent = additionalContent
						};
						
						Console.WriteLine($"Added GSM-R Info {additionalText.Trim()} at {hektoMeter}.");
					}
					else if (additionalText.Contains("Asig"))
					{
						content = new Asig()
						{
							AdditionalContent = additionalContent
						};
						
						Console.WriteLine($"Added Asig {additionalText.Trim()} at {hektoMeter}.");
					}
					else
					{
						// TODO: Continue cases
						content = new UnknownContent(additionalText)
						{
							AdditionalContent = additionalContent
						};
					}
					
					if (rows.ContainsKey(hektoMeter))
						rows[hektoMeter].AdditionalContent.Add(content);
					else
						rows.Add(hektoMeter, content);
					
					additionalContent.Clear();
				}
			}
		}
		
		Console.WriteLine($"Finished at {DateTime.Now.ToString("mm:ss.fff")}");
		
		Console.WriteLine(Environment.NewLine + Environment.NewLine);

		foreach (KeyValuePair<string,string> speedChange in speedChanges)
		{
			Console.WriteLine($"Speed change to {speedChange.Value} at {speedChange.Key}");
		}
		
		
		
		_engine.Dispose();
	}
	
	public static bool ContainsYellow(Rectangle roi, Mat mat)
	{
		Mat roiMat = new Mat(mat, roi);

		Mat hsv = new Mat();
		CvInvoke.CvtColor(roiMat, hsv, ColorConversion.Bgr2Hsv);

		ScalarArray lowerYellow = new ScalarArray(new MCvScalar(25, 100, 100));
		ScalarArray upperYellow = new ScalarArray(new MCvScalar(35, 255, 255));

		Mat mask = new Mat();
		CvInvoke.InRange(hsv, lowerYellow, upperYellow, mask);

		int nonZeroCount = CvInvoke.CountNonZero(mask);

		int threshold = roi.Width * roi.Height / 20;

		return nonZeroCount > threshold;
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
	
	// Rectangle nextStopRoi = new Rectangle(726, 67, 524, 33);
	// Rectangle currSpeedLimitRoi = new Rectangle(124, 750, 76, 36);
	//
	// string nextStop = ExtractText(nextStopRoi, mat);
	// string newNextStop = string.Empty;
	//
	// if(!string.IsNullOrEmpty(nextStop))
	// 	newNextStop = nextStop.Split("alt: ")[1];
	//
	// Console.WriteLine($"Next stop: {newNextStop}");
	// Console.WriteLine($"Current speed limit: {ExtractText(currSpeedLimitRoi, mat)}");
	//
	// Console.WriteLine($"Finished at {DateTime.Now.ToString("mm:ss.fff")}");
	//
	// mat.Dispose();
}
