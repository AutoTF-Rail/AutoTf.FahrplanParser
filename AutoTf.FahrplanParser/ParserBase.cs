using AutoTf.FahrplanParser.Content;
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