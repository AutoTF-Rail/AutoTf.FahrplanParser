using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Base;
using AutoTf.FahrplanParser.Content.Icons;
using AutoTf.FahrplanParser.Content.Markers;
using AutoTf.FahrplanParser.Content.Signals;
using AutoTf.FahrplanParser.Content.Signals.Vorsignal;
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

	public bool TryParseIcon(Mat mat, Rectangle row, out RowContent? content)
	{
		content = null;
		
		try
		{
			Mat iconArea = GetIconArea(mat, row);

			if (LzbStart.TryParseIcon(iconArea))
				content = new LzbStart();
			else if (LzbEnd.TryParseIcon(iconArea))
				content = new LzbEnd();
			else if (YenMarker.TryParseIcon(iconArea))
				content = new YenMarker();
			else if (Stumpfgleis.TryParseIcon(iconArea))
				content = new Stumpfgleis();

			iconArea.Dispose();

			return content == null;
		}
		catch
		{
			// TODO: log?
			return false;
		}
	}

	public RowContent? ResolveContent(string additionalText, string arrivalTime, string departureTime)
	{
		// Next row says "*1) Kopf machen" but the *1) here doesn't matter afaik
		if (arrivalTime.Contains("*1)"))
			return null;
		
		if (TryParseSignal(additionalText, out RowContent? signalContent))
			return signalContent!;
		
		if (TryParseMarker(additionalText, out RowContent? markerContent))
			return markerContent!;
		
		// Important to do this AFTER the markers, because Abzw and others could have a departure time too
		if (TryParseStation(additionalText, arrivalTime, departureTime, out RowContent? stationContent))
			return stationContent!;
		
		return new UnknownContent(additionalText);
	}

	private bool TryParseStation(string additionalText, string arrival, string departure, out RowContent? content)
	{
		content = null;
		
		try
		{
			if (Station.TryParse(additionalText, arrival, departure, out content))
				return true;
			
			if (NoStopStation.TryParse(additionalText, departure, out content))
				return true;
			
			return false;
		}
		catch
		{
			return false;
		}
	}

	private bool TryParseSignal(string additionalText, out RowContent? content)
	{
		if (AusfahrVorsignal.TryParse(additionalText, out content))
			return true;
		
		if (BlockVorsignal.TryParse(additionalText, out content))
			return true;
		
		if (AusfahrSignal.TryParse(additionalText, out content))
			return true;
		
		if (BlockSignal.TryParse(additionalText, out content))
			return true;
		
		if (EinfahrSignal.TryParse(additionalText, out content))
			return true;
		
		if (SelbstBlockSignal.TryParse(additionalText, out content))
			return true;
		
		if (ZwischenSignal.TryParse(additionalText, out content))
			return true;

		return false;
	}
	
	private bool TryParseMarker(string additionalText, out RowContent? content)
	{
		content = null;

		try
		{
			if (Abzweigung.TryParse(additionalText, out content))
				return true;
			
			if (Anschlussstelle.TryParse(additionalText, out content))
				return true;
			
			if (Ausweichanschlussstelle.TryParse(additionalText, out content))
				return true;
			
			if (Bahnuebergang.TryParse(additionalText, out content))
				return true;
			
			if (SwitchSide.TryParse(additionalText, out content))
				return true;
			
			if (Ueberholbahnhof.TryParse(additionalText, out content))
				return true;
			
			if (Ueberleitstelle.TryParse(additionalText, out content))
				return true;
			
			if (ZugFunk.TryParse(additionalText, out content))
				return true;

			return false;
		}
		catch
		{
			return false;
		}
	}

	private Mat GetIconArea(Mat mat, Rectangle row)
	{
		Rectangle roi = new Rectangle(row.X + 386, row.Y, 60, 44);
		Mat area = new Mat(mat, roi);
		CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);

		if (area.NumberOfChannels != 1)
			CvInvoke.CvtColor(area, area, ColorConversion.Bgr2Gray);

		return area;
	}

	protected static string ExtractTextClean(Rectangle roi, Mat mat, Tesseract engine) => ExtractText(roi, mat, engine).Replace("\n", "");

	protected static string ExtractText(Rectangle roi, Mat mat, Tesseract engine)
	{
		using Mat roiMat = new Mat(mat, roi);
		using Pix pix = new Pix(roiMat);
		
		engine.SetImage(pix);
		
		return engine.GetUTF8Text().Trim();
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