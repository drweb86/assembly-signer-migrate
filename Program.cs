using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace AssemblySigner
{
	class Program
	{
		public static void Main(string[] args)
		{
			string configurationFile = Application.ExecutablePath + ".config";
			string[] configuration = File.ReadAllLines(configurationFile);
			string ilasm = configuration[0];
			string ildasm = configuration[1];

			if (!File.Exists(ilasm))
			{
				Console.WriteLine("Please edit configuration file and specify existent ilasm.exe file");
				return;
			}

			if (!File.Exists(ildasm))
			{
				Console.WriteLine("Please edit configuration file and specify existent ildasm.exe file");
				return;
			}

			Console.WriteLine("Drop here from file manager .snk file which will be used for signing assembly: ");
			string snkFile = Console.ReadLine();
			
			if (!File.Exists(snkFile))
			{
				Console.WriteLine("Hey, you didn't specified the existent .snk file!");
				return;
			}
			
			string quotedSnkFile = "\"" + snkFile + "\"";
			
			do
			{
				string output;
				string errorOutput;
				Process process;
			
				Console.WriteLine("Drop here from file manager source .dll library(unsigned assembly): ");
				string unsignedSourceAssembly = Console.ReadLine();
				
				if (string.IsNullOrEmpty(unsignedSourceAssembly))
				{
					Console.WriteLine("Hey, you didn't specified the source assembly!");
					return;
				}
				
				if (!File.Exists(unsignedSourceAssembly))
				{
					Console.WriteLine("Hey, the source assembly should exist!");
					return;
				}

				Console.WriteLine("Please enter target signed assembly path and name: ");
				string resDll = Console.ReadLine();
			
				string tempIl = Path.Combine(
					Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(unsignedSourceAssembly) + ".il");
				
				File.Delete(tempIl);
				Console.WriteLine("================================== ildasm ============================");
				process = new Process();
				process.StartInfo.FileName = ildasm;
				//ildasm /all /out=Bar.il Bar.dll
				process.StartInfo.Arguments =
					string.Format("/all /out={0} \"{1}\"",
					             tempIl,
					             unsignedSourceAssembly);
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.Start();
				output = process.StandardOutput.ReadToEnd();
				errorOutput = process.StandardError.ReadToEnd();
				process.WaitForExit();
				Console.WriteLine(output);
				Console.WriteLine(errorOutput);
				
				Console.WriteLine("================================== ilasm ============================");
				string tempDll = Path.Combine(
					Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(unsignedSourceAssembly) + ".dll");
				string tempRes = Path.Combine(
					Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(unsignedSourceAssembly) + ".res");
				
				File.Delete(tempRes);
				
				process = new Process();
				process.StartInfo.FileName = ilasm;
				
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				
				if (!File.Exists(tempRes))
				{
				//ilasm /dll /key=Foo.snk Bar.il
					process.StartInfo.Arguments = 
					string.Format("/dll /key={0} {1}",
					            quotedSnkFile,
					            tempIl);
				}
				else
				{
				//ilasm /dll /key=Foo.snk /RESOURCE={2} Bar.il
					process.StartInfo.Arguments = 
					string.Format("/dll /key={0} /RESOURCE={2} {1}",
					            quotedSnkFile,
					            tempIl,
					            tempRes);
				
				}
				
				process.Start();
				output = process.StandardOutput.ReadToEnd();
				errorOutput = process.StandardError.ReadToEnd();
				process.WaitForExit();
				Console.WriteLine(output);
				Console.WriteLine(errorOutput);
				
				Console.WriteLine("================================== copy ============================");
				File.Delete(resDll);
				File.Copy(tempDll, resDll);
			
				Console.WriteLine("Done\nTo quit hit enter, to sign another assembly please enter something else");
			}
			while (Console.ReadLine() != string.Empty);
		}
	}
}
