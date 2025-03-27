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

		if (TryParseSignal(additionalText, out RowContent? signalContent))
			return signalContent!;
		
		if (TryParseMarker(additionalText, out RowContent? markerContent))
			return markerContent!;
		
		// Important to do this AFTER the markers, because Abzw could have a departure time too
		
		// TODO: Gibt es stationen die notiert sind, aber auch keine abfahrts zeit haben?
		if (string.IsNullOrWhiteSpace(arrivalTime) && !string.IsNullOrWhiteSpace(departureTime))
			return new NoStopStation(additionalText);
		
		// Other
		if (additionalText.Contains("GSM-R"))
		{
			return new GSMRInfo(additionalText.Trim());
		}

		// TODO: Continue cases
		return new UnknownContent(additionalText);
	}

	private bool TryParseSignal(string additionalText, out RowContent? content)
	{
		content = null;
		string speed = "40";

		Dictionary<string, string> signalMap = new Dictionary<string, string>
		{
			// U for unknown
			{ "Esig", "E" },
			{ "Asig", "A" },
			{ "Bksig", "Bk" },
			{ "Zsig", "Z" },
			{ "Bkvsig", "U" },
			{ "Sbk", "U" },
		};
		// TODO: Does a Bksig have the option for speed? Because it can carry the signal ID too (Same for Sbk)

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
		
		content = sigType switch
		{
			"Esig" => new EinfahrSignal(remainingText, speed),
			"Asig" => new AusfahrSignal(remainingText, speed),
			"Bksig" => new BlockSignal(remainingText, speed),
			"Zsig" => new ZwischenSignal(remainingText, speed),
			"Bkvsig" => new BlockVorsignal(remainingText),
			"Sbk" => new SelbstBlockSignal(remainingText),
			_ => null
		};

		return content != null;
	}
	
	private bool TryParseMarker(string additionalText, out RowContent? content)
	{
		content = null;

		List<string> markers = new List<string>
		{
			{ "Abzw" }
		};

		string? markerType = markers.FirstOrDefault(additionalText.Contains);
		
		if (markerType == null) 
			return false;
		
		string remainingText = additionalText.Replace(markerType, "").Trim();
		
		content = markerType switch
		{
			"Abzw" => new Abzweigung(remainingText),
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

	public RowContent? CheckForDuplicateContent(RowContent content, string hektometer, List<KeyValuePair<string, RowContent>> rows)
	{
		if (rows.Count == 0) 
			return content;
		
		// TODO: Is this enough of a comparison? 
		// TODO: Can we just use this for the station check too?
		// TODO: Limit the check to the last 5? Since the last 5 are always only repeated?
		return rows.Any(x => x.Key == hektometer && x.Value.GetType() == content.GetType()) ? null : content;
	}
}