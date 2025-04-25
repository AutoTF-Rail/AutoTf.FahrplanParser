using AutoTf.FahrplanParser.Content.Content.Base;

namespace AutoTf.FahrplanParser.Content.Content.Markers;

public class SwitchSide : RowContent
{
	public override string GetPrint()
	{
		return "Kopf machen";
	}

	public static bool TryParse(string additionalText, out IRowContent? content)
	{
		content = null;

		if (!additionalText.Contains("Kopf machen"))
			return false;
		
		content = new SwitchSide();
		return true;
	}
}