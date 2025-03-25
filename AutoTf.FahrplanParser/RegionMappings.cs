using System.Drawing;

namespace AutoTf.FahrplanParser;

public static class RegionMappings
{
	public static readonly List<Rectangle> Rows = new List<Rectangle>()
	{
		new Rectangle(85, 109, 1165, 44),
		new Rectangle(85, 155, 1165, 44),
		new Rectangle(85, 201, 1165, 44),
		new Rectangle(85, 248, 1165, 44),
		new Rectangle(85, 294, 1165, 44),
		new Rectangle(85, 340, 1165, 44),
		new Rectangle(85, 385, 1165, 44),
		new Rectangle(85, 432, 1165, 44),
		new Rectangle(85, 477, 1165, 44),
		new Rectangle(85, 523, 1165, 44),
		new Rectangle(85, 568, 1165, 44),
		new Rectangle(85, 614, 1165, 44),
		new Rectangle(85, 659, 1165, 44),
		new Rectangle(85, 705, 1165, 44),
		new Rectangle(85, 750, 1165, 44),
	};
	
	public static readonly List<Rectangle> LocationPoints = new List<Rectangle>()
	{
		new Rectangle(190, 432, 37, 43),
		new Rectangle(190, 477, 37, 43),
		new Rectangle(190, 523, 37, 43),
		new Rectangle(190, 568, 37, 43),
		new Rectangle(190, 614, 37, 43),
		new Rectangle(190, 659, 37, 43),
		new Rectangle(190, 705, 37, 43)
	};
					
	public static readonly List<Rectangle> LocationPointsHektometer = new List<Rectangle>()
	{
		new Rectangle(259, 432, 127, 43),
		new Rectangle(259, 477, 127, 43),
		new Rectangle(259, 523, 127, 43),
		new Rectangle(259, 568, 127, 43),
		new Rectangle(259, 614, 127, 43),
		new Rectangle(259, 659, 127, 43),
		new Rectangle(259, 705, 127, 43)
	};
	
	public static readonly Rectangle PlanValidity = new Rectangle(348, 11, 259, 44);
	public static readonly Rectangle TrainNumber = new Rectangle(17, 11, 134, 44);
	public static readonly Rectangle Delay = new Rectangle(696, 812, 134, 30);
	public static readonly Rectangle Time = new Rectangle(1066, 11, 160, 44);
	public static readonly Rectangle Date = new Rectangle(805, 11, 190, 44);
}