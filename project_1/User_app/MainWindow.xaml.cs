using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Schedule_generation_library;

namespace User_app
{
    public partial class MainWindow : Window
    {
        IntermediaryData _data;
        public bool Stop_flag { get; set; }
        public MainWindow()
        {
            _data = new IntermediaryData();
            this.DataContext = _data;
            InitializeComponent();
        }
        private void Start_click(object sender, RoutedEventArgs e)
        {
            Start_button.IsEnabled = false;
            _data.Change_parameters();
            this.DataContext = null;
            this.DataContext = _data;

            int min_rivals_count = 0;
            int min_fields_visited = 0;
            Schedule iteration_res = null;
            Schedule_generation generator = new Schedule_generation(_data.Fields_count, _data.Teams_count, _data.Rounds_count);
            generator.First_generation();
            int i = 0;
            Stop_flag = false;
            while (i < _data.Generations_count && !Stop_flag)
            {
                iteration_res = generator.Iteration_parallel();
                TextBlock1.Text = (i + 1).ToString();
                if (min_rivals_count != iteration_res.Min_rivals_count || min_fields_visited != iteration_res.Min_fields_visited)
                { 
                    TextBlock2.Text = iteration_res.Min_rivals_count.ToString();
                    TextBlock3.Text = iteration_res.Min_fields_visited.ToString();
                    //string[,] alter_matr = iteration_res.Alternative_schedule_form();
                    //string tmp = "";
                    //for (i = 0; i < alter_matr.GetLength(0); i++)
                    //{
                    //    for (int j = 0; j < alter_matr.GetLength(1); j++)
                    //        tmp += alter_matr[i, j] + " ";
                    //    tmp += "\n";
                    //}
                    // TextBlock4.Text = tmp;
                }
                i += 1;
            }
            string[,] alter_matr = iteration_res.Alternative_schedule_form();
            string tmp = "";
            for (i = 0; i < alter_matr.GetLength(0); i++)
            {
                for (int j = 0; j < alter_matr.GetLength(1); j++)
                    tmp += alter_matr[i, j] + " ";
                tmp += "\n";
            }
            TextBlock4.Text = tmp;
            Start_button.IsEnabled = true;
        }
        private void Stop_click(object sender, RoutedEventArgs e)
        {
            Stop_flag = true;
        }
    }
    public class IntermediaryData: INotifyPropertyChanged
    {
        public int Fields_count { get; set; }
        public int Teams_count { get; set; }
        public int Rounds_count { get; set; }
        public int Mutations_count { get; set; }
        public int Population_size { get; set; }
        public int Generations_count { get; set; }
        public IntermediaryData()
        { 
            Fields_count = 12;
            Teams_count = 10;
            Rounds_count = 8;
            Mutations_count = 100;
            Population_size = 100;
            Generations_count = 10;
        }
        public void Change_parameters()
        {
            PropChanged("Fields_count");
            PropChanged("Teams_count");
            PropChanged("Rounds_count");
            PropChanged("Mutations_count");
            PropChanged("Population_size");
            PropChanged("Generations_count");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void PropChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public static class Command
    {
        public static RoutedCommand RunCommand = new RoutedCommand("RunCommand", typeof(User_app.Command));
    }
}