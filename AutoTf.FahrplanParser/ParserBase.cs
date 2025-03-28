using System.Text.RegularExpressions;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Signals;
using AutoTf.FahrplanParser.Content.Signals.Vorsignal;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

public abstract class ParserBase
{
	protected readonly Tesseract Engine;

	protected ParserBase(Tesseract engine)
	{
		Engine = engine;
	}

	public RowContent? ResolveContent(string additionalText, string arrivalTime, string departureTime)
	{
		// Next row says "*1) Kopf machen" but the *1) here doesn't matter afaik
		if (arrivalTime.Contains("*1)"))
			return null;
		
		if (!string.IsNullOrWhiteSpace(arrivalTime) && !string.IsNullOrWhiteSpace(departureTime))
		{
			if (arrivalTime.Trim() == "X")
			{
				// TODO: Handle this case?
			}
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
		if (additionalText.Contains("ZF"))
		{
			additionalText = additionalText.Replace("ZF", "").Replace("-", "").Trim();
			if (additionalText.Contains("ENDE"))
				return new ZugFunkEnde();
			
			return new ZugFunk(additionalText);
		}
		
		// TODO: Is the number here at any time different? Maybe führerstand ID?
		// TODO: This never has a hektometer, but we still need to assign it to one? 
		if (additionalText.Contains("Kopf machen"))
			return new SwitchSide();

		if (additionalText.Contains("Bü"))
		{
			// TODO: Is this "ET" of any importance?/Is there a different sign sometimes?
			string kilometer = additionalText.Replace("Bü", "").Replace("km", "").Replace("ET", "");
			return new Bahnübergang(kilometer);
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
			"Avsig" => new AusfahrVorsignal(remainingText),
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
			{ "Abzw" },
			{ "Üst" },
			{ "Übf" },
			{ "Awamst" },
			{ "Anst" },
		};

		string? markerType = markers.FirstOrDefault(additionalText.Contains);
		
		if (markerType == null) 
			return false;
		
		string remainingText = additionalText.Replace(markerType, "").Trim();
		
		content = markerType switch
		{
			"Abzw" => new Abzweigung(remainingText),
			"Üst" => new Überleitstelle(remainingText),
			"Übf" => new Überholbahnhof(remainingText),
			"Awamst" => new Ausweichanschlussstelle(remainingText),
			"Anst" => new Anschlussstelle(remainingText),
			_ => null
		};

		return content != null;
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