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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Text.Json;
using Newtonsoft.Json;
using Schedule_generation_library;

namespace User_app
{
    public partial class MainWindow : Window
    {
        IntermediaryData _data;
        public int Stop_index { get; set; }
        public Schedule_generation generator = null;
        public bool Stop_flag { get; set; }
        public bool Load_flag { get; set; }
        public bool Save_flag { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            _data = new IntermediaryData();
            this.DataContext = _data;
            Start_button.IsEnabled = true;
            Save_button.IsEnabled = false;
            Load_button.IsEnabled = true;
            Stop_button.IsEnabled = false;
        }
        private async void Start_click(object sender, RoutedEventArgs e)
        {
            Start_button.IsEnabled = false;
            Stop_button.IsEnabled = true;
            Save_button.IsEnabled = false;
            Load_button.IsEnabled = false;
            _data.Change_parameters();
            this.DataContext = null;
            this.DataContext = _data;

            Schedule iteration_res = null;
            generator = new Schedule_generation(_data.Fields_count, _data.Teams_count, _data.Rounds_count, _data.Mutations_count, _data.Population_size);

            generator.First_generation();
            int i = 0;
            Stop_flag = false;
            while (i < _data.Generations_count && !Stop_flag)
            {
                iteration_res = generator.Iteration_parallel();
                await Task.Delay(0).ContinueWith(skip =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if ((i + 1) % 20 == 0)
                        {
                            print(iteration_res);
                            TextBlock1.Text = (i + 1).ToString();
                        }
                    });
                });
                i += 1;
            }

            TextBlock1.Text = i.ToString();
            print(iteration_res);
            MessageBox.Show("Выполнение остановлено");

            Stop_index = i;
            Start_button.IsEnabled = true;
            Stop_button.IsEnabled = false;
            Save_button.IsEnabled = true;
            Load_button.IsEnabled = true;
        }
        private void Stop_click(object sender, RoutedEventArgs e)
        {
            Stop_flag = true;
        }
        private void print(Schedule iteration_res)
        {
            string[,] alter_matr = iteration_res.Alternative_schedule_form();
            string tmp = "";

            for (int i = 0; i < alter_matr.GetLength(0); i++)
            {
                for (int j = 0; j < alter_matr.GetLength(1); j++)
                    tmp += alter_matr[i, j] + " ";
                tmp += "\n";
            }

            TextBlock2.Text = iteration_res.Min_rivals_count.ToString();
            TextBlock3.Text = iteration_res.Min_fields_visited.ToString();
            TextBlock4.Text = tmp;
        }

        private void Save_click(object sender, RoutedEventArgs e)
        {
            Save_flag = true;
            while (Save_flag)
            {
                SavingNameWindow savingNameWindow = new SavingNameWindow();
                savingNameWindow.Owner = this;
                savingNameWindow.ShowDialog();
                if (savingNameWindow.Exit_flag)
                {
                    if (savingNameWindow.Generator_name.Length >= 4)
                    {
                        using (var db = new ApplicationContext())
                        {
                            Generator_attrs tmp_generator = new Generator_attrs();

                            tmp_generator.Name = savingNameWindow.Generator_name;
                            tmp_generator.Stop_idx = Stop_index;
                            tmp_generator.Teams_count = _data.Teams_count;
                            tmp_generator.Rounds_count = _data.Rounds_count;
                            tmp_generator.Fields_count = _data.Fields_count;
                            tmp_generator.Mutations_count = _data.Mutations_count;
                            tmp_generator.Generations_count = _data.Generations_count;
                            tmp_generator.Population_size = _data.Population_size;
                            db.Generators_attrs.Add(tmp_generator);

                            for (int i = 0; i < generator.Current_population.Count; i++)
                            {
                                string serializable_schedule = JsonConvert.SerializeObject(generator.Current_population[i].Schedule_matrix);
                                Schedule_population tmp = new Schedule_population();
                                tmp.generator_Attrs = tmp_generator;
                                tmp.Schedule_view = serializable_schedule;

                                db.Schedule_Populations.Add(tmp);
                            }

                            db.SaveChanges();
                            MessageBox.Show("Генератор сохранен");
                            Save_flag = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Название должно состоять минимум из 4-х символов");
                    }
                }
                else
                {
                    Save_flag = false;
                    MessageBox.Show("Генератор не сохранен");
                }
            }
        }

        private async void Load_click(object sender, RoutedEventArgs e)
        {
            using (var db = new ApplicationContext())
            {
                Load_flag = true;
                List<string> generators_names = db.Generators_attrs.Select(x => x.Name).ToList();
                if (generators_names.Count == 0)
                {
                    MessageBox.Show("Нет доступных генераторов");
                    Load_flag = false;
                }

                while (Load_flag)
                {
                    LoadGeneratorWindow loadGeneratorWindow = new LoadGeneratorWindow(generators_names);
                    loadGeneratorWindow.Owner = this;
                    loadGeneratorWindow.ShowDialog();
                    if (loadGeneratorWindow.Exit_flag)
                    {
                        Start_button.IsEnabled = false;
                        Stop_button.IsEnabled = true;
                        Save_button.IsEnabled = false;
                        Load_button.IsEnabled = false;
                        Generator_attrs g = db.Generators_attrs.Include(x => x.Population).ToList().Single(x => x.Name == loadGeneratorWindow.Generator_name);
                        _data.Fields_count = g.Fields_count;
                        _data.Teams_count = g.Teams_count;
                        _data.Rounds_count = g.Rounds_count;
                        _data.Mutations_count = g.Mutations_count;
                        _data.Population_size = g.Population_size;
                        _data.Generations_count = g.Generations_count;
                        _data.Change_parameters();

                        this.DataContext = null;
                        this.DataContext = _data;

                        List<Schedule> tmp_lst = new List<Schedule>();
                        for (int j = 0; j < g.Population.Count; j++)
                        {
                            Schedule tmp = new Schedule(JsonConvert.DeserializeObject<int[,]>(g.Population[j].Schedule_view));
                            tmp_lst.Add(tmp);
                        }

                        generator = new Schedule_generation(g.Fields_count, g.Teams_count, g.Rounds_count, g.Mutations_count, g.Population_size);
                        generator.First_generation(tmp_lst);
                        Stop_index = g.Stop_idx;

                        Schedule iteration_res = null;
                        int i = Stop_index;
                        Stop_flag = false;
                        while (i < _data.Generations_count && !Stop_flag)
                        {
                            iteration_res = generator.Iteration_parallel();
                            await Task.Delay(0).ContinueWith(skip =>
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    if ((i + 1) % 20 == 0)
                                    {
                                        print(iteration_res);
                                        TextBlock1.Text = (i + 1).ToString();
                                    }
                                });
                            });
                            i += 1;
                        }

                        TextBlock1.Text = i.ToString();
                        if (iteration_res != null)
                        {
                            print(iteration_res);
                        }
                        else
                        {
                            MessageBox.Show("Данный генератор уже выполнен");
                        }
                        Load_flag = false;
                        MessageBox.Show("Выполнение остановлено");

                        Stop_index = i;
                        Start_button.IsEnabled = true;
                        Stop_button.IsEnabled = false;
                        Save_button.IsEnabled = true;
                        Load_button.IsEnabled = true;
                    }
                    else
                    {
                        Load_flag = false;
                        MessageBox.Show("Генератор не загружен");
                    }
                }
            }
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
            Mutations_count = 50;
            Population_size = 250;
            Generations_count = 100;
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
    public class Generator_attrs
    {
        public string? Name { get; set; }
        public int Stop_idx { get; set; }
        public List<Schedule_population> Population { get; set; } = new();
        public int Fields_count { get; set; }
        public int Teams_count { get; set; }
        public int Rounds_count { get; set; }
        public int Mutations_count { get; set; }
        public int Population_size { get; set; }
        public int Generations_count { get; set; }
    }
    public class Schedule_population
    { 
        public int Id { get; set; }
        public string? Schedule_view { get; set; }
        public Generator_attrs generator_Attrs { get; set; }
    }
    public class ApplicationContext : DbContext
    {
        public DbSet<Generator_attrs> Generators_attrs { get; set; }
        public DbSet<Schedule_population> Schedule_Populations { get; set; }
        public ApplicationContext()
        {
           // Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=myappdb2;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Generator_attrs>().HasKey(tmp => tmp.Name);
        }
    }
}