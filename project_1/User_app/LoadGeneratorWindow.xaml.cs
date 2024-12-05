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
using System.Windows.Shapes;

namespace User_app
{
    public partial class LoadGeneratorWindow : Window
    {
        public bool Exit_flag { get; set; }
        public string Generator_name
        {
            get
            {
                return (string)GeneratorsNamesListBox.SelectedItem;
            }
        }
        public LoadGeneratorWindow(List<string> generators_names)
        {
            InitializeComponent();
            GeneratorsNamesListBox.ItemsSource = generators_names;
        }
        private void Choose_click(object sender, RoutedEventArgs e)
        {
            Exit_flag = true;
            this.Close();
        }
        private void Cancel_click(object sender, RoutedEventArgs e)
        {
            Exit_flag = false;
            this.Close();
        }
    }
}
