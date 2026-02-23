using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace pokemon_checkbox;

public class Program
{
	const int COLUMNS = 3;
	const int ROWS_PER_PAGE = 15;

	public static void Main(string[] args)
	{
		var list = GetNames();
		HtmlWriter("index.html", list);
	}

	public static List<Pokemon> GetNames()
	{
		string filePath = "pokemon_names.csv";

		using StreamReader reader = new(filePath);
		var header = reader.ReadLine() ?? throw new ArgumentException("Invalid line in csv");
		var deIndex = header.Split(',').IndexOf("de");

		string line;
		var maxPokemon = 386;

		var list = new List<Pokemon>();
		for (var i = 1; i <= maxPokemon; i++)
		{
			line = reader.ReadLine() ?? throw new ArgumentException("Invalid line in csv");
			var name = line.Split(',').ElementAt(deIndex);
			list.Add(new(i, name, $"sprites/{i}.png"));
		}
		return list;
	}

	public static void HtmlWriter(string path, List<Pokemon> list)
	{
		var settings = new XmlWriterSettings
		{
			Indent = true, // Pretty print
			IndentChars = "\t", // Tab
			Encoding = Encoding.UTF8,
			NewLineOnAttributes = false
		};

		var columns = list.Chunk(ROWS_PER_PAGE);
		var columns2 = list.Chunk(ROWS_PER_PAGE*COLUMNS);

		using var writer = XmlWriter.Create(path, settings);

		//what(writer, columns);
		what2(writer, columns2);
	}

	private static void what(XmlWriter writer, IEnumerable<Pokemon[]> columns)
	{
		writer.WriteStartElement("html");
		SetStyle(writer);
		writer.WriteStartElement("body");

		var even = true;
		for(var i = 0; i < columns.Count(); i+=COLUMNS)
		{
			writer.WriteStartElement("div");
			writer.WriteAttributeString("class", "tableWrapper");

			for (var a = 0; a < COLUMNS && i+a < columns.Count(); a++)
			{
				writer.WriteStartElement("div");
				AddTable(writer, [.. columns.ElementAt(i+a)], even);
				even = !even;
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private static void what2(XmlWriter writer, IEnumerable<Pokemon[]> pages)
	{
		writer.WriteStartElement("html");
		SetStyle(writer);
		writer.WriteStartElement("body");

		var even = true;
		foreach(var page in pages)
		{
			writer.WriteStartElement("div");
			writer.WriteAttributeString("class", "tableWrapper");

			var itemsPerColumn = (int)Math.Ceiling((decimal)page.Length/3);
			foreach(var column in page.Chunk(itemsPerColumn))
			{
				writer.WriteStartElement("div");
				AddTable(writer, [.. column], even);
				even = !even;
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		writer.WriteEndElement();
		writer.WriteEndElement();
	}


	private static void AddTable(XmlWriter writer, List<Pokemon> list, bool evenColumn)
	{
		writer.WriteStartElement("table");

		foreach (var pokemon in list)
		{
			AddRow(writer, pokemon, evenColumn ? "evenColumn" : "oddColumn");
		}
		writer.WriteEndElement();
	}

	private static void AddRow(XmlWriter writer, Pokemon pokemon, string? cssClass = null)
	{
		writer.WriteStartElement("tr");
		if(cssClass is not null)
			writer.WriteAttributeString("class", cssClass);
		AddCell(writer, $"#{pokemon.Index:D3}");
		AddCell(writer, pokemon.Name, "nameCell");
		writer.WriteStartElement("td");
		writer.WriteStartElement("img");
		writer.WriteAttributeString("src", GetBase64ImageString(pokemon.Path));
		writer.WriteEndElement();
		writer.WriteEndElement();
		AddCheckBoxCell(writer, pokemon.Index);

		writer.WriteEndElement();
	}

	private static void AddCheckBoxCell(XmlWriter writer, int index)
	{
		writer.WriteStartElement("td");
		writer.WriteStartElement("input");
		writer.WriteAttributeString("type", "checkbox");
		writer.WriteAttributeString("id", $"checkbox_{index}");
		writer.WriteAttributeString("class", "checkbox");
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private static void AddCell(XmlWriter writer, string content, string? cssClass = null)
	{
		writer.WriteStartElement("td");
		if (cssClass is not null)
		{
			writer.WriteAttributeString("class", cssClass);
		}
		writer.WriteString(content);
		writer.WriteEndElement();
	}

	private static void SetStyle(XmlWriter writer)
	{
		var fontName = "pokemon-font";

		writer.WriteStartElement("head");
		writer.WriteStartElement("style");
		writer.WriteString(GetFontStyle("pokemon_font.woff2", fontName));
		writer.WriteString($"body {{ font-family: '{fontName}'; -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; letter-spacing: 1px; }}");
		writer.WriteString("table { border-collapse: collapse }");
		writer.WriteString(".tableWrapper { display: flex; gap: 20px; }");
		writer.WriteString("td { text-align: center; }");
		writer.WriteString(".nameCell { width: 85px; }");

		writer.WriteString(".checkbox { transform: scale(1.5); width: 29px; }");
		writer.WriteString("tr:nth-child(even).evenColumn { background-color: #f2f2f2; }");
		writer.WriteString("tr:nth-child(odd).oddColumn { background-color: #f2f2f2; }");


		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	private static string GetBase64ImageString(string path)
	{
		var fileBytes = File.ReadAllBytes(path);
		return $"data:image/png;base64,{Convert.ToBase64String(fileBytes)}";
	}

	private static string GetFontStyle(string path, string name)
	{
		var stringBuilder = new StringBuilder();

		stringBuilder.Append("@font-face {\n");
		stringBuilder.Append($"font-family: '{name}';\n");

		var fileBytes = File.ReadAllBytes(path);
		var srcUrl = $"data:font/woff2;charset=utf-8;base64,{Convert.ToBase64String(fileBytes)}";
		stringBuilder.Append($"src: url({srcUrl}) format('woff2');\n");

		stringBuilder.Append("font-weight: normal;\n");
		stringBuilder.Append("font-style: normal;\n");
		stringBuilder.Append('}');

		return stringBuilder.ToString();
	}
}

public class Pokemon(int index, string name, string path)
{
	public int Index = index;
	public string Name = name;
	public string Path = path;
}