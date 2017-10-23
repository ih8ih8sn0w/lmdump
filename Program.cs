using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lmdump
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());
			if (args.Length == 0)
			{
				var lm = new Lumen(@"C:/users/ih8ih8sn0w/desktop/chara.lm");
				Console.WriteLine("Default chara.lm loaded");
			}
			else
			{
				var lm = new Lumen(args[0]);
				Console.WriteLine("This file was loaded: " + args[0]);
			}
            //var x = 43;
        }
    }
}
