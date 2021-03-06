﻿using SearchEngine.Model;
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
        Searcher searcher;
        Ranker ranker;
        string filesPath, postingPath;

        /// <summary>
        /// mail constructor
        /// </summary>
        public MainWindow()
        {

            ranker = new Ranker();
            searcher = new Searcher(ranker);
            searcher.SearcherChanged += vSearcherChanged;

        }

        private void vSearcherChanged(int type, string value)
        {
            if (type == 1)
            {
                MessageBox.Show(value);
            }
            else if (type == 2)
            {

                this.Dispatcher.Invoke((Action)(() =>
                {
                    List<string> ans = searcher.getResult();
                    if (ans.Count > 1)
                        txtbx_postingDisplay.Text = ans.Aggregate((i, j) => i + '\n' + j);
                    else if (ans.Count == 1) txtbx_postingDisplay.Text = ans.ElementAt(0);
                }));
            }

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
                if (Directory.Exists(postingPath + @"\docs"))
                    Directory.Delete(postingPath + @"\docs");
                Directory.CreateDirectory(postingPath + @"\docs");
            }
            catch (Exception exp)
            {

                return;
            }

            btn_startParsing.IsEnabled = false;
            btn_loadPosting.IsEnabled = false;
            indexer = new Indexer(postingPath);
			ranker.postingPath = postingPath;
            parser = new Parse(filesPath, postingPath, indexer, cb_Stemmeing.IsChecked.Value);
            parser.ModelChanged += vModelChanged;
            Thread thread = new Thread(new ThreadStart(parser.startParsing));
            thread.Start();
            btn_runQuery.IsEnabled = true;
            btn_runQueryFile.IsEnabled = true;
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
                btn_clearPosting.IsEnabled = false;
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
                File.Delete(postingPath + @"\Doc.bin");
           


                MessageBox.Show("Posting Files were cleared");
                btn_startParsing.IsEnabled = true;
                btn_clearPosting.IsEnabled = true;
                btn_runQuery.IsEnabled = false;
                btn_runQueryFile.IsEnabled = false;
            }
            catch (Exception exp)
            {
                btn_clearPosting.IsEnabled = true;
                btn_runQueryFile.IsEnabled = true;
                MessageBox.Show("clearing Posting Files failed");
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
            txtbx_postingPath.Text = openFile.SelectedPath.ToString();
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            killAll();
            Application.Current.Shutdown();
        }

        private void killAll()
        {
            if (parser != null)
            {
                parser.kill();
                searcher.kill();
                if (t_query != null && t_query.IsAlive)
                    t_query.Join();
            }

        }
        Thread t_query;
        private void btn_runQuery_Click(object sender, RoutedEventArgs e)
        {

            if (txtbx_query.Text != "")
            {
				ranker.mainIndexList1 = indexer.mainIndexList1;
				ranker.mainIndexList2 = indexer.mainIndexList2;
				ranker.mainIndexList3 = indexer.mainIndexList3;
				ranker.mainIndexList4 = indexer.mainIndexList4;
				ranker.mainIndexList5 = indexer.mainIndexList5;
                txtbx_postingDisplay.Text = "";
                searcher.toMonth = toMonth;
                searcher.fromMonth = fromMonth;
                searcher._indexer = indexer;
                searcher.queryText = txtbx_query.Text;
                Parse.use_stem = cb_Stemmeing.IsChecked.Value;
                t_query = new Thread(() => searcher.searchDocs());
                t_query.Start();
            }
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
			ranker.postingPath = postingPath;
            btn_startParsing.IsEnabled = false;
            btn_loadPosting.IsEnabled = false;
            btn_runQuery.IsEnabled = true;
            btn_runQueryFile.IsEnabled = true;
            MessageBox.Show("Posting was loaded from file");
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            data.Add("");
            for (int i = 1; i < 13; i++)
            {
                data.Add(i.ToString());
            }


            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        int fromMonth = 0, toMonth = 0;
        Thread t_multiQuery;
        private bool multiThread;
        string[] queries;
        List<string> result;

        private void btn_runQueryFile_Click(object sender, RoutedEventArgs e)
        {
            if (txtbx_postingPath.Text.Length != 0)
            {
                if (!Directory.Exists(txtbx_postingPath.Text))
                {
                    MessageBox.Show("Please enter a valid Posting Files path");
                    //hi there
                    return;
                }

                searcher.toMonth = toMonth;
                searcher.fromMonth = fromMonth;
                searcher._indexer = indexer;
                Parse.use_stem = cb_Stemmeing.IsChecked.Value;

                postingPath = txtbx_postingPath.Text;
                try
                {
                    queries = File.ReadAllLines(postingPath + "\\queries.txt");
                }
                catch (Exception)
                {

                    MessageBox.Show("Please add a queries.txt file at posting path");
                    return;
                }
				ranker.mainIndexList1 = indexer.mainIndexList1;
				ranker.mainIndexList2 = indexer.mainIndexList2;
				ranker.mainIndexList3 = indexer.mainIndexList3;
				ranker.mainIndexList4 = indexer.mainIndexList4;
				ranker.mainIndexList5 = indexer.mainIndexList5;
                t_multiQuery = new Thread(delegate ()
                {
                    if (File.Exists(postingPath + "\\result.txt"))
                    {
                        File.Delete(postingPath + "\\result.txt");
                    }

                    foreach (var line in queries)
                    {
                        int num = int.Parse(line.Substring(0, 4));
                        searcher.queryText = line.Substring(4);
                        searcher.searchDocs();
                        result = searcher.getResult();
                        using (StreamWriter sw = File.AppendText(postingPath + "\\result.txt"))
                        {
                            foreach (var item in result)
                            {
                                sw.WriteLine(num.ToString() + " 0 " + item + " 0 42.38 mt");
                            }
                        }
                    }

                });
                t_multiQuery.Start();
                

            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;
            if (comboBox.Name == "combx_fromMonth")
            {
                fromMonth = comboBox.SelectedIndex;
            }
            else
            {
                toMonth = comboBox.SelectedIndex;
            }


            /*
            // ... Set SelectedItem as Window Title.
            string value = comboBox.SelectedItem as string;
            this.Title = "Selected: " + value;
            */
        }


    }
}
