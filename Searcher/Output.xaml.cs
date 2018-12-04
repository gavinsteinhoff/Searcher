using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Searcher
{
    /// <summary>
    /// Interaction logic for Output.xaml
    /// </summary>
    public partial class Output : Window
    {
        public Output()
        {
            InitializeComponent();
        }

        public void AddData(Results r)
        {
            //creates a hyper link based on the result
            Hyperlink link = new Hyperlink();
            link.Inlines.Add(r.ToString());
            link.Tag = r.path;
            link.Click += new RoutedEventHandler(onClick);
            Text.Inlines.Add(link);
            Text.Inlines.Add("\n");
        }

        private void onClick(object sender, RoutedEventArgs args)
        {
            //gets what link was clicked and opens the file
            Hyperlink link = (Hyperlink)args.Source;
            System.Diagnostics.Process.Start(link.Tag.ToString());
        }
    }
}
