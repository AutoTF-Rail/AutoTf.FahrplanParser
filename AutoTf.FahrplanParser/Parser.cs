using System.Drawing;
using AutoTf.FahrplanParser.Content;
using AutoTf.FahrplanParser.Content.Content.Base;
using AutoTf.FahrplanParser.Extensions;
using Emgu.CV;
using Emgu.CV.OCR;

namespace AutoTf.FahrplanParser;

public class Parser : InfoParser
{
	public Parser(Tesseract engine) : base(engine) { }

	public void ReadPage(Parser parser, Mat mat, ref List<KeyValuePair<string, RowContent>> rows, ref List<KeyValuePair<string, string>> speedChanges)
	{
		List<Rectangle> rowsRoi = [..RegionMappings.Rows];
		rowsRoi.Reverse();

		List<RowContent> additionalContent = new List<RowContent>();
		
		foreach (Rectangle row in rowsRoi)
		{
			string hektometer = parser.Hektometer(mat, row);
			string additionalText = parser.AdditionalText(mat, row);
			string arrivalTime = parser.Arrival(mat, row);
			string departureTime = parser.Departure(mat, row);
			
			// If we don't have a hektometer, we will add it's info to the next one
			if (string.IsNullOrWhiteSpace(hektometer))
			{
				RowContent? content = parser.ResolveContent(additionalText, arrivalTime, departureTime);
				
				if(content == null)
					continue;
				
				content = parser.CheckForDuplicateContent(content, hektometer, rows);
				
				additionalContent.Add(content!);
			}
			else
			{
				rows.AddRange(additionalContent.Select(x => new KeyValuePair<string, RowContent>(hektometer, x)));
				additionalContent.Clear();
				
				string speedLimit = parser.SpeedLimit(mat, row);
					
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
				
				// We need to save this, because tunnelContent could return to being null, if it's duplicate, but in the check after the tunnel parsing, we need to know if we had a tunnel. (And which type)
				TunnelType tunnelType = TunnelType.None;
				
				if (parser.TryParseTunnel(mat, row, additionalText, out RowContent? tunnelContent))
				{
					if(tunnelContent is TunnelContent tunnel)
						tunnelType = tunnel.GetTunnelType();
					
					// TODO: Different list?
					tunnelContent = parser.CheckForDuplicateContent(tunnelContent!, hektometer, rows);
					
					if (tunnelContent != null)
						rows.Add(new KeyValuePair<string, RowContent>(hektometer, tunnelContent));
				}

				if (tunnelType != TunnelType.End)
				{
					if (string.IsNullOrWhiteSpace(additionalText))
					{
						if (parser.TryParseIcon(mat, row, out RowContent? result))
							content = result;
					}
					else
					{
						if (tunnelType != TunnelType.Start)
							content = parser.ResolveContent(additionalText, arrivalTime, departureTime);
						
						// No need for a null check, since the method does it
						// if(!string.IsNullOrWhiteSpace(arrivalTime))
						// 	content = parser.CheckForDuplicateStation(content, arrivalTime, additionalText, rows);
					}
					
				}
				if(content != null)
					content = parser.CheckForDuplicateContent(content, hektometer, rows);

				if (content == null)
					continue;

				rows.Add(new KeyValuePair<string, RowContent>(hektometer, content));
			}
		}
	}
}