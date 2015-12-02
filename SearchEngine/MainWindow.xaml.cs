using SearchEngine.Model;
using System;
using System.Collections.Generic;
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
            string path = "";
            Parse parser = new Parse(path);
            string text = " 10.6 May percentage Binyamin Netanyahu Binyamin , United States, San Francisco JS JavaScript for example: Dollars 22 3/4 dsfsdfsda 300-45.6 million hyp6rion 3,000,000 fdg 6% between 18 10.6 percent and 24 6-7 10.77 percents iii.	Dollars price m (for example: Dollars 20.6m)	Dollars price bn (for example: Dollars 5.3bn) $price million (for example: $100 million) $price billion (for example: $100 billion)(for example: $450,000)  (for example: 16th May 1991)example: 14 MAY 1991)example: 14 MAY 91) 14 MAY) 1990 )May 1994)April 28, 1990)June 4) \"Weekly Information Newspaper of the Republic of Srpska\" (LES ECHOS, GMT";
            string dates = "  (for example: 16th May 1991)example: 14 MAY 1991)example: 14 MAY 91) 14 MAY) 1990 )May 1994)April 28, 1990)June 4)";
            parser.testReg(text);
        }
    }
}
