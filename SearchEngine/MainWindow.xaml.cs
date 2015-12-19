using SearchEngine.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SearchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            System.Console.WriteLine("started parsing at:" + DateTime.Now);
            File.Create("abNumsPosting.txt");
			File.Create("cfPosting.txt");
			File.Create("gmPosting.txt");
			File.Create("nrPosting.txt");
			File.Create("szPosting.txt");
			string path = @"C:\Users\Bar\Desktop\engineFiles\corpus";

            Parse parser = new Parse(path);
            parser.startParsing();

            System.Console.WriteLine("finished all at:" + DateTime.Now);
            System.Console.Read();
        }
    }
}
