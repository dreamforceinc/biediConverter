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
		static uint vehiclesCount = 0, markersCount = 0;


		static string parseString(string str) => str.Substring(str.IndexOf('=') + 1);

		static void parseVehicle(StreamReader sr, string str, vehObject veh)
		{
			string buf;
			while (str != "};")
			{
				str = sr.ReadLine();
				buf = str.Trim().ToLower();
				if (buf.StartsWith("position="))
				{
					veh.Position = str.Substring(str.IndexOf('=') + 1).Trim('"', ';');
					//veh.Position = veh.Position.Trim('"', ';');
				}
				if (buf.StartsWith("type="))
				{
					veh.Type = str.Substring(str.IndexOf('=') + 1).Trim(';');
					//veh.Type = veh.Type.Trim(';');
				}
				if (buf.StartsWith("azimut="))
				{
					veh.Azimut = str.Substring(str.IndexOf('=') + 1).Trim('"', ';');
					//veh.Azimut = veh.Azimut.Trim('"', ';');
				}
				if (buf.StartsWith("init=\"_this setvectorup"))
				{
					veh.Init = true;
				}
			}
		}

		static void parseMarker(StreamReader sr, string str, markerObject mrk)
		{
			string buf;
			while (str != "};")
			{
				str = sr.ReadLine();
				buf = str.Trim().ToLower();
				if (buf.StartsWith("position="))
				{
					mrk.Position = str.Substring(str.IndexOf('=') + 1).Trim('"', ';');
				}
				if (buf.StartsWith("name="))
				{
					mrk.Name = str.Substring(str.IndexOf('=') + 1).TrimEnd(';');
				}
				if (buf.StartsWith("text="))
				{
					mrk.Text = str.Substring(str.IndexOf('=') + 1).TrimEnd(';');
				}
				if (buf.StartsWith("type="))
				{
					mrk.Type = str.Substring(str.IndexOf('=') + 1).TrimEnd(';');
				}
				if (buf.StartsWith("marker_type="))
				{
					mrk.MarkerType = str.Substring(str.IndexOf('=') + 1).TrimEnd(';');
				}
				if (buf.StartsWith("color="))
				{
					mrk.Color = str.Substring(str.IndexOf('=') + 1).TrimEnd(';');
				}
				if (buf.StartsWith("fill="))
				{
					mrk.Fill = str.Substring(str.IndexOf('=') + 1).TrimEnd(';');
				}
				if (buf.StartsWith("a="))
				{
					mrk.A = str.Substring(str.IndexOf('=') + 1).Trim('"', ';');
				}
				if (buf.StartsWith("b="))
				{
					mrk.B = str.Substring(str.IndexOf('=') + 1).Trim('"', ';');
				}
			}
		}

		static int Main(string[] args)
		{
			Console.WriteLine($"{appName} v{appVer}\n{appCopyright} by {appCompany}\n");

			if (args.Count() != 1)
			{
				Console.WriteLine($"usage: {appName}.exe \"path\\to\\file.biedi\"");
				Console.WriteLine("Press any key to quit.");
				Console.ReadKey();
				return 1;
			}
			if (!File.Exists(args[0]))
			{
				Console.WriteLine($"{appName}: file '{args[0]}' does not exist!");
				Console.WriteLine("Press any key to quit.");
				Console.ReadKey();
				return 2;
			}
			string path = Path.GetDirectoryName(args[0]);
			string filename = Path.GetFileNameWithoutExtension(args[0]);
			string fileext = Path.GetExtension(args[0]);
			if (fileext.ToLower() != ".biedi")
			{
				Console.WriteLine($"{appName}: file '{args[0]}' is not .biedi file!");
				Console.WriteLine("Press any key to quit.");
				Console.ReadKey();
				return 3;
			}
			string sqfFile = Path.Combine(path, filename + ".sqf");
#if DEBUG
			sqfFile = "C:\\Users\\W0LF\\Documents\\ArmA 2\\missions\\test.Chernarus\\out.sqf";
#endif
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
							vehiclesCount++;
							parseVehicle(sr, bufLine, item);
							vehicles.Add(item);
						}
						if (bufLine.StartsWith("objecttype=\"marker\";"))
						{
							markerObject marker = new markerObject();
							markersCount++;
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

			if (vehiclesCount > 0 || markersCount > 0)
			{
				try
				{
					using (StreamWriter sw = new StreamWriter(sqfFile))
					{
						sw.WriteLine($"// Created by {appName} v{appVer}\n// {appCopyright} by {appCompany}\n");
						if (vehiclesCount > 0)
						{
							sw.WriteLine("_obj = [");
							for (int i = 0; i < vehicles.Count(); ++i)
							{
								var obj = vehicles[i];
								sw.WriteLine(obj.makeString(i == vehicles.Count() - 1 ? false : true));
							}
							sw.WriteLine("];");
							sw.WriteLine("{\n\t_this = (_x select 0) createVehicle (_x select 1);\n\t_this setPos (_x select 1);\n\t_this setDir (_x select 2);\n\tif (_x select 3) then {_this setVectorUp [0,0,1];};\n} forEach _obj;");
						}
						if (markersCount > 0)
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
			else if (vehiclesCount == 0)
			{
				Console.WriteLine("\nNo vehicles found.");
			}
			else if (markersCount == 0)
			{
				Console.WriteLine("\nNo markers found.");
			}

			Console.WriteLine("Press any key to quit.");
			Console.ReadKey();
			return 0;
		}
	}
}
