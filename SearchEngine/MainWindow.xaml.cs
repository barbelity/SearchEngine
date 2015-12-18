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
            File.Create("postingFile.txt");
            string path = @"E:\Users\Ziv\Documents\שנה שלישית\אחזור\corpus\corpus";
            //Indexer indexer = new Indexer();
            Parse parser = new Parse(path);
            parser.startParsing();
            //SortedDictionary<string, Term> dict = parser.d_allTerms;
            //System.Console.WriteLine("Finished parsing at:" + DateTime.Now);
            System.Console.WriteLine("finished all at:" + DateTime.Now);
            System.Console.Read();
            /*
            System.Console.WriteLine("started indexing at:" + DateTime.Now);
            indexer.saveTerms(dict);
            System.Console.WriteLine("finished indexing at:" + DateTime.Now);
            System.Console.Read();
            */
        }
    }
}
