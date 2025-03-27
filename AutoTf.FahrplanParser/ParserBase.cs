using System.Drawing;
using System.Text.RegularExpressions;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Signals;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

public abstract class ParserBase
{
	protected readonly Tesseract Engine;

	protected ParserBase(Tesseract engine)
	{
		Engine = engine;
	}

	public RowContent ResolveContent(string additionalText, string arrivalTime, string departureTime)
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

		if (TryParseSignal(additionalText, out RowContent? content))
			return content!;
		
		// Other
		if (additionalText.Contains("GSM-R"))
		{
			return new GSMRInfo(additionalText.Trim());
		}

		// TODO: Continue cases
		return new UnknownContent(additionalText);
	}

	// Used for Esig etc to get the speed, and station name (both are optional)
	private bool TryParseSignal(string additionalText, out RowContent? content)
	{
		content = null;
		string speed = "40";

		Dictionary<string, string> signalMap = new Dictionary<string, string>
		{
			{ "Esig", "E" },
			{ "Asig", "A" },
			{ "Bksig", "Bk" },
			{ "Zsig", "Z" }
		};

		string? sigType = signalMap.Keys.FirstOrDefault(additionalText.Contains);
		
		if (sigType == null) 
			return false;
		
		string speedId = signalMap[sigType];
		
		string remainingText = additionalText.Replace(sigType, "").Trim();
		
		Match match = Regex.Match(remainingText, @$"{speedId}\d+");
		
		if (match.Success)
		{
			speed = match.Value;
			remainingText = remainingText.Replace(speed, "").Trim();
		}
		
		string station = remainingText;
		
		content = sigType switch
		{
			"Esig" => new EinfahrSignal(station, speed),
			"Asig" => new AusfahrSignal(station, speed),
			"Bksig" => new BlockSignal(station, speed),
			"Zsig" => new ZwischenSignal(station, speed),
			_ => null
		};

		return content != null;
	}

	public RowContent? CheckForDuplicateStation(RowContent content, string arrivalTime, string stationName, List<KeyValuePair<string, RowContent>> knownStations)
	{
		if (content is not Station station)
			return null;

		if (knownStations.Count == 0) 
			return station;
		
		List<KeyValuePair<string, RowContent>> stations = knownStations.Where(x => x.Value is Station).ToList();

		return stations.Any(x => x.Value is Station value && 
		                         value.Arrival == arrivalTime && value.Name == stationName) ? null : station;
	}
}