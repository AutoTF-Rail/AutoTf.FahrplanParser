namespace AutoTf.FahrplanParser.Content;

public class ZugFunk : RowContent
{
	public ZugFunk(string info)
	{
		Info = info;
	}
	
	public string Info { get; set; }
	
	public override string GetPrint()
	{
		return $"- ZF {Info} -";
	}
}