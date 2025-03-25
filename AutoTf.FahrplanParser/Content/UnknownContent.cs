namespace AutoTf.FahrplanParser.Content;

public class UnknownContent : RowContent
{
	public UnknownContent(string content)
	{
		Content = content;
	}

	public string Content { get; set; }
}