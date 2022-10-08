using System;
namespace biediConverter
{
	internal class markerObject
	{
		internal string Position { get; set; } = "";
		internal string Name { get; set; } = "";
		internal string Text { get; set; } = "";
		internal string Type { get; set; } = "";
		internal string MarkerType { get; set; } = "";
		internal string Color { get; set; } = "";
		internal string Fill { get; set; } = "";
		internal string A { get; set; } = "";
		internal string B { get; set; } = "";

		internal string makeString()
		{
			string str = "";
			str += string.Concat("_this = createMarker [", Name, ',', Position, "];\n");
			if (Text.Length > 0) str += string.Concat("_this setMarkerText ", Text, ";\n");
			if (Type.Length > 0) str += string.Concat("_this setMarkerType ", Type, ";\n");
			if (Color.Length > 0) str += string.Concat("_this setMarkerColor ", Color, ";\n");
			if (Fill.Length > 0) str += string.Concat("_this setMarkerBrush ", Fill, ";\n");
			if (MarkerType.Length > 0) str += string.Concat("_this setMarkerShape ", MarkerType, ";\n");
			if (A.Length > 0 || B.Length > 0) str += string.Concat("_this setMarkerSize [", A, ',', B, "];\n");
			Console.WriteLine(str);
			return str;
		}
	}
}
