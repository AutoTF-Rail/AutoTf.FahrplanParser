using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content.Markers;

public class SwitchSide : RowContent
{
	// TODO: Is the number here at any time different? Maybe führerstand ID?
	// TODO: This never has a hektometer, but we still need to assign it to one? 
	
	public override string GetPrint()
	{
		return "*1) Kopf machen";
	}

	public static bool TryParse(string additionalText, out RowContent? content)
	{
		content = null;

		if (!additionalText.Contains("Kopf machen"))
			return false;
		
		content = new SwitchSide();
		return true;
	}
}