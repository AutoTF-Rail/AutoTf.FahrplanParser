using AutoTf.FahrplanParser.Content.Base;

namespace AutoTf.FahrplanParser.Content;

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