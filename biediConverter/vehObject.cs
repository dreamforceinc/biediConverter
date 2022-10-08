using System;
namespace biediConverter
{
	internal class vehObject
	{
		internal string Position { get; set; } = "";
		internal string Azimut { get; set; } = "0";
		internal string Type { get; set; } = "";
		internal bool Init { get; set; } = false;

		internal string makeString(bool comma)
		{
			string str = string.Concat('\t', '[', Type, ',', Position, ',', Azimut, ',', Init.ToString().ToLower(), ']');
			if (comma) str += ',';
			Console.WriteLine(str);
			return str;
		}
	}
}
