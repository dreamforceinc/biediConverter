using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace biediConverter
{
	internal class Program
	{
		static Assembly app = typeof(Program).Assembly;
		static string appName = app.GetName().Name;
		static string appVer = app.GetName().Version.Major.ToString() + '.' + app.GetName().Version.Minor.ToString();
		static string appFullVer = app.GetName().Version.ToString();
		static object[] attrCopyright = app.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
		static object[] attrCompany = app.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
		static string appCopyright = (attrCopyright[0] as AssemblyCopyrightAttribute).Copyright;
		static string appCompany = (attrCompany[0] as AssemblyCompanyAttribute).Company;


		static string parseString(string str) => str.Substring(str.IndexOf('=') + 1);

		static void parseVehicle(StreamReader sr, string str, vehObject veh)
		{
			string buf;
			while (str != "};")
			{
				str = sr.ReadLine();
				buf = str.Trim().ToLower();
				if (buf.StartsWith("position=")) veh.Position = parseString(str).Trim('"', ';');
				if (buf.StartsWith("type=")) veh.Type = parseString(str).Trim(';');
				if (buf.StartsWith("azimut=")) veh.Azimut = parseString(str).Trim('"', ';');
				if (buf.StartsWith("init=\"_this setvectorup")) veh.Init = true;
			}
		}

		static void parseMarker(StreamReader sr, string str, markerObject mrk)
		{
			string buf;
			while (str != "};")
			{
				str = sr.ReadLine();
				buf = str.Trim().ToLower();
				if (buf.StartsWith("position=")) mrk.Position = parseString(str).Trim('"', ';');
				if (buf.StartsWith("name=")) mrk.Name = parseString(str).TrimEnd(';');
				if (buf.StartsWith("text=")) mrk.Text = parseString(str).TrimEnd(';');
				if (buf.StartsWith("type=")) mrk.Type = parseString(str).TrimEnd(';');
				if (buf.StartsWith("marker_type=")) mrk.MarkerType = parseString(str).TrimEnd(';');
				if (buf.StartsWith("color=")) mrk.Color = parseString(str).TrimEnd(';');
				if (buf.StartsWith("fill=")) mrk.Fill = parseString(str).TrimEnd(';');
				if (buf.StartsWith("a=")) mrk.A = parseString(str).Trim('"', ';');
				if (buf.StartsWith("b=")) mrk.B = parseString(str).Trim('"', ';');
			}
		}

		static int Main(string[] args)
		{
			Console.WriteLine($"{appName} v{appVer} ({appFullVer})\n{appCopyright} by {appCompany}");

			if (args.Count() != 1)
			{
				Console.WriteLine($"usage: {appName}.exe \"path\\to\\file.biedi\"");
				return 1;
			}

			string sourcePath = args[0];
			if (!File.Exists(sourcePath))
			{
				Console.WriteLine($"ERROR: file '{sourcePath}' does not exist!");
				return 2;
			}

			string fileext = Path.GetExtension(sourcePath);
			if (fileext.ToLower() != ".biedi")
			{
				Console.WriteLine($"ERROR: file '{sourcePath}' is not .biedi file!");
				return 3;
			}

			string sqfFile = Path.Combine(Path.GetDirectoryName(sourcePath), "output.sqf");

			List<vehObject> vehicles = new List<vehObject>();
			List<markerObject> markers = new List<markerObject>();
			string bufLine;

			try
			{
				using (StreamReader sr = new StreamReader(args[0]))
				{
					while (!sr.EndOfStream)
					{
						bufLine = sr.ReadLine().Trim().ToLower();
						if (bufLine.StartsWith("objecttype=\"vehicle\";"))
						{
							vehObject item = new vehObject();
							parseVehicle(sr, bufLine, item);
							vehicles.Add(item);
						}
						if (bufLine.StartsWith("objecttype=\"marker\";"))
						{
							markerObject marker = new markerObject();
							parseMarker(sr, bufLine, marker);
							markers.Add(marker);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			if (vehicles.Count > 0 || markers.Count > 0)
			{
				try
				{
					using (StreamWriter sw = new StreamWriter(sqfFile))
					{
						sw.WriteLine($"// Created by {appName} v{appVer} ({appFullVer})\n// {appCopyright} by {appCompany}");
						if (vehicles.Count > 0)
						{
							sw.WriteLine();
							Console.WriteLine();
							sw.WriteLine("_obj = [");
							for (int i = 0; i < vehicles.Count(); ++i)
							{
								var obj = vehicles[i];
								sw.WriteLine(obj.makeString(i == vehicles.Count() - 1 ? false : true));
							}
							sw.WriteLine("];");
							sw.WriteLine("{\n\t_this = (_x select 0) createVehicle (_x select 1);\n\t_this setPos (_x select 1);\n\t_this setDir (_x select 2);\n\tif (_x select 3) then {_this setVectorUp [0,0,1];};\n} forEach _obj;");
						}
						if (markers.Count > 0)
						{
							sw.WriteLine();
							Console.WriteLine();
							for (int i = 0; i < markers.Count(); ++i)
							{
								sw.WriteLine(markers[i].makeString());
							}
						}
					}
					Console.WriteLine("\nDone.");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}

			return 0;
		}
	}
}
