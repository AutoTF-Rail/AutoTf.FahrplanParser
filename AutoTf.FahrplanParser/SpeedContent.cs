namespace AutoTf.FahrplanParser;

public class SpeedContent : RowContent
{
	public SpeedContent(string speedLimit)
	{
		SpeedLimit = speedLimit;
	}

	public string SpeedLimit { get; set; }
}