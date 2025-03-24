using Tesseract;

internal static class Program
{
	public static void Main(string[] args)
	{
		while (true)
		{
			Console.WriteLine("AutoTF Fahrplan Parser");
			Console.Write("File path:");
			string? path = Console.ReadLine();
		
			if (path == null)
			{
				Console.WriteLine("Please enter a valid path.");
				continue;
			}

			if (!File.Exists(path))
			{
				Console.WriteLine("Please enter a valid path.");
				continue;
			}

			using TesseractEngine engine = new TesseractEngine("tessdata/", "deu", EngineMode.Default);
			using Pix? img = Pix.LoadFromFile(path);
			using Page? page = engine.Process(img);

			string text = page.GetText();
			Console.WriteLine(text);
			
		}
	}
}
