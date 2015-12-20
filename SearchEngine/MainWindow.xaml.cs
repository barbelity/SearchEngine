using SearchEngine.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        Indexer indexer;
        Parse parser;
        string filesPath, postingPath;
        public MainWindow()
        {




        }
        private void vModelChanged(int type, string value)
        {
            if (type == 1)
            {
                MessageBox.Show(value);
            }

        }

        private void btn_startParsing_Click(object sender, RoutedEventArgs e)
        {
          
            if (txtbx_filesPath.Text.Length != 0)
            {
                if (!Directory.Exists(txtbx_filesPath.Text))
                {
                    MessageBox.Show("Please enter a valid Data Files path");
                    return;
                }
                filesPath = txtbx_filesPath.Text;
            }
            else
            {
                MessageBox.Show("Please enter Data Files path");
                return;
            }
            if (txtbx_postingPath.Text.Length != 0)
            {
                if (!Directory.Exists(txtbx_postingPath.Text))
                {
                    MessageBox.Show("Please enter a valid Posting Files path");
                    return;
                }
                postingPath = txtbx_postingPath.Text;
            }
            else
            {
                MessageBox.Show("Please enter Posting Files path");
                return;
            }



            try
            {
                File.Create(postingPath + @"\abNumsPosting.txt").Dispose();
                File.Create(postingPath + @"\cfPosting.txt").Dispose();
                File.Create(postingPath + @"\gmPosting.txt").Dispose();
                File.Create(postingPath + @"\nrPosting.txt").Dispose();
                File.Create(postingPath + @"\szPosting.txt").Dispose();
            }
            catch (Exception exp)
            {

                return;
            }

            System.Console.WriteLine("started parsing at:" + DateTime.Now);
            indexer = new Indexer(postingPath);
            parser = new Parse(filesPath, indexer, cb_Stemmeing.IsChecked.Value);
            parser.ModelChanged += vModelChanged;
            Thread thread = new Thread(new ThreadStart(parser.startParsing));
            thread.Start();
            //System.Console.WriteLine("finished all at:" + DateTime.Now);


        }

        private void btn_clearPosting_Click(object sender, RoutedEventArgs e)
        {
            if (txtbx_postingPath.Text.Length != 0)
            {
                if (!Directory.Exists(txtbx_postingPath.Text))
                {
                    MessageBox.Show("Please enter a valid Posting Files path");
                    return;
                }
                postingPath = txtbx_postingPath.Text;
            }
            else
            {
                MessageBox.Show("Please enter Posting Files path");
                return;
            }

            try
            {
                File.Delete(postingPath + @"\abNumsPosting.txt");
                File.Delete(postingPath + @"\cfPosting.txt");
                File.Delete(postingPath + @"\gmPosting.txt");
                File.Delete(postingPath + @"\nrPosting.txt");
                File.Delete(postingPath + @"\szPosting.txt");
                File.Delete(postingPath + @"\list1.bin");
                File.Delete(postingPath + @"\list2.bin");
                File.Delete(postingPath + @"\list3.bin");
                File.Delete(postingPath + @"\list4.bin");
                File.Delete(postingPath + @"\list5.bin");
            }
            catch (Exception exp)
            {

                return;
            }
        }

        private void btn_displayPosting_Click(object sender, RoutedEventArgs e)
        {
 

            if (indexer != null)
            {
                txtbx_postingDisplay.Text = indexer.getPostingString();
            }


        }

        private void b_dataPath_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = openFile.ShowDialog();
            
            txtbx_filesPath.Text = openFile.SelectedPath.ToString();
        }

        private void b_postingPath_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = openFile.ShowDialog();
            txtbx_postingDisplay.Text = openFile.SelectedPath.ToString();
        }

        private void btn_loadPosting_Click(object sender, RoutedEventArgs e)
        {
            if (txtbx_postingPath.Text.Length != 0)
            {
                if (!Directory.Exists(txtbx_postingPath.Text))
                {
                    MessageBox.Show("Please enter a valid Posting Files path");
                    return;
                }

                postingPath = txtbx_postingPath.Text;
            }
            else
            {
                MessageBox.Show("Please enter Posting Files path");
                return;
            }

            indexer = new Indexer(postingPath, true);
        }
    }
}
