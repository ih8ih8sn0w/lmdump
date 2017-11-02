using System;
using System.Diagnostics;
using System.IO;

namespace lmdump
{
	static class Program
	{
		public static Lumen lm;

		[STAThread]

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				lm = new Lumen(@"C:\Users\ih8ih8sn0w\Desktop\SSB4\Tools\Wii U\sm4shexplorer 0.07.1\extract\data\ui\lumen\other\other.lm");
			}
			else
			{
				lm = new Lumen(args[0]);
			}
		}
	}
}
