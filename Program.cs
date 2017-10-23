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
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            if (args.Length == 0)
			{
                //var lm = new Lumen(@"C:\s4explore\extract\data\ui\lumen\com_hdr\com_hdr.lm");
                Console.WriteLine("Please provide an input file to continue");
			}
			else
			{
				var lm = new Lumen(args[0]);
			}
        }
    }
}
