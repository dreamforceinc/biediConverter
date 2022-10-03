﻿using System;
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
		const string prefix = "_obj = [";
		const string postfix = "];";
		const string loop = "{\n\t_this = (_x select 0) createVehicle ((_x select 1) select 0);\n\t_this setDir ((_x select 1) select 1);\n\t_this setPos ((_x select 1) select 0);\n\tif (_x select 2) then {_this setVectorUp [0,0,1];};\n} forEach _obj;";

		private static string parseString(string str)
		{
			int pos = str.IndexOf('=');
			str = str.Substring(pos + 1);
			str = str.Trim('"', ';');
			return str;
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

			//Console.WriteLine(appName + '\n');
			List<vehObject> objects = new List<vehObject>();
			string bufLine;

			try
			{
				using (StreamReader sr = new StreamReader(args[0]))
				{
					while (!sr.EndOfStream)
					{
						bufLine = sr.ReadLine();
						if (!bufLine.ToLower().StartsWith("class _vehicle_")) continue;

						vehObject item = new vehObject();
						while (bufLine != "\t};")
						{
							//Console.WriteLine(bufLine);
							bufLine = sr.ReadLine();

							if (bufLine.Contains("POSITION="))
							{
								item.Position = parseString(bufLine);
								continue;
							}
							if (bufLine.Contains("TYPE="))
							{
								item.Type = parseString(bufLine);
								continue;
							}
							if (bufLine.Contains("AZIMUT="))
							{
								item.Azimut = parseString(bufLine);
								continue;
							}
							if (bufLine.ToUpper().Contains("INIT="))
							{
								item.Init = true;
								continue;
							}
						}
						objects.Add(item);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}



			try
			{
				using (StreamWriter sw = new StreamWriter(sqfFile))
				{
					sw.WriteLine(prefix);
					for (int i = 0; i < objects.Count(); ++i)
					{
						var obj = objects[i];
						sw.WriteLine(obj.makeString(i == objects.Count() - 1 ? false : true));
					}
					sw.WriteLine(postfix);
					sw.WriteLine(loop);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			Console.WriteLine("Done.");
			Console.WriteLine("Press any key to quit.");
			Console.ReadKey();
			return 0;
		}
	}
}