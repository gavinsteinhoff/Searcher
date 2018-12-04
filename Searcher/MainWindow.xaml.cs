using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace Searcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Default extensions to search for
            txtBoxExtension.AppendText("*.txt");
            txtBoxExtension.AppendText(Environment.NewLine);
            txtBoxExtension.AppendText("*.json");
            txtBoxExtension.AppendText(Environment.NewLine);
            txtBoxExtension.AppendText("*.xml");
        }

        private bool HasSearchFolder = false;
        private bool HasOutputFile = false;
        private bool HasWordFile = false;
        private string SearchPath;
        private string OutputFile;
        private string WordFile;

        private void FindMatches(string folder, Data data)
        {
            //First loops through all extensions
            foreach (string ext in data.ExtensionList)
            {
                //Loops through all files in the folder and subfolders
                foreach (string file in Directory.EnumerateFiles(folder, ext, SearchOption.AllDirectories))
                {
                    int lineNumber = 1;
                    foreach (string line in File.ReadAllLines(file))
                    {
                        //takes one line and loops through all the words to find
                        data.WordList.ForEach(word =>
                        {
                            //Creates a regex statemnt from the word and allows words to be found inside words with \\S*
                            Regex regex = new Regex("\\S*" + Regex.Escape(word) + "\\S*", RegexOptions.IgnoreCase);
                            MatchCollection matches = regex.Matches(line);
                            foreach (Match match in matches)
                            {
                                foreach (Capture capture in match.Captures)
                                {
                                    //Adds the word to the results list
                                    Results result = new Results(lineNumber, file, capture.Value, capture.Index);
                                    data.Results.Add(result);
                                }
                            }
                        }
                        );
                        lineNumber++;
                    }
                }
            }
        }

        Stopwatch stopwatch = new Stopwatch();

        private void worker_FindMatches(object sender, DoWorkEventArgs e)
        {
            stopwatch.Reset();
            stopwatch.Start();

            //Gets the data from the parameter
            Data data = (Data)e.Argument;
            FindMatches(SearchPath, data);
            //sends results to worker_done
            e.Result = data;
        }

        private void worker_Done(object sender, RunWorkerCompletedEventArgs e)
        {

            stopwatch.Stop();
            //Grabs the data
            Data data = (Data)e.Result;
            //hides the progress bar
            pbStatus.Visibility = Visibility.Hidden;
            btnSearch.IsEnabled = true;
            MessageBoxResult mb = MessageBox.Show(string.Format("Found {0} matches in {1} seconds", data.Results.Count, stopwatch.ElapsedMilliseconds/1000));
            //adds all the data to the output window
            foreach (Results result in data.Results)
            {
                data.OutputWindow.AddData(result);
            }
            //Shows the outputwindow
            data.OutputWindow.ShowDialog();
            if (HasOutputFile)
            {
                //Turns Results into string array to dump into file
                List<string> outputText = new List<string>();
                data.Results.ForEach(r => outputText.Add(r.ToString()));
                File.WriteAllLines(OutputFile, outputText);
                //Shows Text Report
                Process.Start(OutputFile);
            }
        }

  

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //checks if the user has all the required items
            if (HasSearchFolder && (HasWordFile || ckManualWordList.IsChecked.Value))
            {
                //Creates the output file but doesn't load it yet
                Output outputWindow = new Output();
                //Gets all the data to send to the worker
                Data data = new Data(GetWordList(), GetExtensionList(), outputWindow);
                //Changed to UI
                pbStatus.Visibility = Visibility.Visible;
                btnSearch.IsEnabled = false;
                //Gets a new worker, gives them their methods, and startes them
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += worker_FindMatches;
                worker.RunWorkerCompleted += worker_Done;
                worker.RunWorkerAsync(data);
            }
            else
            {
                MessageBoxResult mb = MessageBox.Show("Select a Search Folder and Input File or Use a Manual word List");
            }
        }

        private void btnSearchFrom_Click(object sender, RoutedEventArgs e)
        {
            //Uses old windows form way to get a folder - there is no new way to get just a folder
            //Propts the user to select a folder to search
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) { }
                {
                    SearchPath = dialog.SelectedPath;
                    txtSearchFrom.Text = SearchPath;
                    HasSearchFolder = true;
                }
            }
        }

        private void btnOutputTo_Click(object sender, RoutedEventArgs e)
        {
            //Propts the user to provide a place to save the output.txt file
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "Ouput";
            sfd.DefaultExt = ".txt";
            sfd.Filter = "Text documents (.txt)|*.txt";
            var result = sfd.ShowDialog();
            if (result == true)
            {
                OutputFile = sfd.FileName;
                txtOutputTo.Text = sfd.FileName;
                HasOutputFile = true;
            }
        }

        private void btnGetWordList_Click(object sender, RoutedEventArgs e)
        {
            //Propts the user to open a text document
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text documents (.txt)|*.txt";
            var result = ofd.ShowDialog();
            if (result == true)
            {
                txtWordList.Text = ofd.FileName.ToString();
                WordFile = ofd.FileName.ToString();
                ckManualWordList.IsChecked = false;
                HasWordFile = true;
            }
        }

        private List<string> GetWordList()
        {
            List<string> words = new List<string>();
            //Way to get manual words
            if (ckManualWordList.IsChecked.Value)
            {
                if (txtBoxWordList.Text != string.Empty)
                {
                    //loops through each line and grabs the word and trims it
                    int lineCount = txtBoxWordList.LineCount;
                    for (int line = 0; line < lineCount; line++)
                    {
                        string word = txtBoxWordList.GetLineText(line);
                        if (word != string.Empty)
                        {
                            word = word.Trim();
                            words.Add(word);
                        }
                    }
                }
                else
                {
                    MessageBoxResult mb = MessageBox.Show("Fill in the word list or import a word file");
                }
            }
            else
            {
                //opens up the word file and loops through the lines and grabs and trims the word
                string line;
                StreamReader file = new StreamReader(WordFile);
                while ((line = file.ReadLine()) != null)
                {
                    words.Add(line.Trim());
                }
            }
            return words;
        }

        private List<string> GetExtensionList()
        {
            List<string> extensions = new List<string>();
            //Same procedure as the manual word list
            if (txtBoxExtension.Text != string.Empty)
            {
                int lineCount = txtBoxExtension.LineCount;
                for (int line = 0; line < lineCount; line++)
                {
                    string extension = txtBoxExtension.GetLineText(line);
                    if (extension != string.Empty)
                    {
                        extension = extension.Trim();
                        extensions.Add(extension);
                    }
                }
            }
            else
            {
                MessageBoxResult mb = MessageBox.Show("Fill in the extension list");
            }
            return extensions;
        }

        //Changes word list textbox avalibilty depending on the word list checkbox
        private void ckManualWordList_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBoxWordList.IsEnabled = false;
        }
        private void ckManualWordList_Checked(object sender, RoutedEventArgs e)
        {
            txtBoxWordList.IsEnabled = true;
        }
    }

    public class Data
    {
        public List<string> WordList { get; set; }
        public List<string> ExtensionList { get; set; }
        public List<Results> Results { get; set; }
        public Output OutputWindow { get; set; }

        public Data(List<string> words, List<string> extensions, Output output)
        {
            WordList = words;
            ExtensionList = extensions;
            OutputWindow = output;
            Results = new List<Results>();
        }

    }

}
