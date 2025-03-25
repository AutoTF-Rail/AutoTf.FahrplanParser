namespace AutoTf.FahrplanParser;

public class GSMRInfo : RowContent
{
	public GSMRInfo(string info)
	{
		Info = info;
	}
	
	public string Info { get; set; }
}