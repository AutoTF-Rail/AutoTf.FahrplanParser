using System.Drawing;
using AutoTf.FahrplanParser.Content.Content;
using AutoTf.FahrplanParser.Content.Content.Base;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

public abstract class ParserBase
{
	protected readonly Tesseract Engine;

	protected ParserBase(Tesseract engine)
	{
		Engine = engine;
	}

	protected bool TryParseTunnel(Mat mat, Rectangle row, string additionalText, out RowContent? content) =>
		ContentResolver.TryParseTunnel(mat, row, additionalText, out content);

	protected bool TryParseIcon(Mat mat, Rectangle row, out RowContent? content) =>
		ContentResolver.TryParseIcon(mat, row, out content);

	protected RowContent? ResolveContent(string additionalText, string arrivalTime, string departureTime)
	{
		// Next row says "*1) Kopf machen" but the *1) here doesn't matter afaik // TODO: Check the *1) things etc.
		if (arrivalTime.Contains("*1)"))
			return null;
		
		if (ContentResolver.TryParseSignal(additionalText, out RowContent? signalContent))
			return signalContent!;
		
		if (ContentResolver.TryParseMarker(additionalText, out RowContent? markerContent))
			return markerContent!;
		
		// Important to do this AFTER the markers, because Abzw and others could have a departure time too
		if (ContentResolver.TryParseStation(additionalText, arrivalTime, departureTime, out RowContent? stationContent))
			return stationContent!;
		
		return new UnknownContent(additionalText);
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