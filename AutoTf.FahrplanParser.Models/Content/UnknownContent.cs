using AutoTf.FahrplanParser.Content.Content.Base;

namespace AutoTf.FahrplanParser.Content.Content;

public class UnknownContent : RowContent
{
	public UnknownContent(string content)
	{
		Content = content;
	}

	public string Content { get; set; }
	
	public override string GetPrint()
	{
		return $"Unknown: {Content}";
	}
}