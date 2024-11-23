using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Schedule_generation_library;

namespace Testing
{
    class test
    {
        static void Main()
        {
            int fields = 12;
            int teams = 10;
            int rounds = 8;
            int generations = 150;
            int mutations = 20;
            int population = 500;
            Console.WriteLine("Выберите вариант:\n");
            Console.WriteLine("Стнадартные параметры - введите 0");
            Console.WriteLine("Ввод параметров - введите 1");
            char c = Console.ReadLine()[0];
            Console.WriteLine();
            if (c == '0')
            {
                Params_output(fields, teams, rounds, generations, mutations, population);
                Schedule_generation testing = new Schedule_generation(fields, teams, rounds, mutations, population);
                testing.First_generation();
                Generation_with_output(testing, generations);
                Params_output(fields, teams, rounds, generations, mutations, population);
            }
            else if (c == '1')
            {
                Data_input(ref fields, ref teams, ref rounds, ref generations, ref mutations, ref population);
                Schedule_generation testing = new Schedule_generation(fields, teams, rounds, mutations, population);
                testing.First_generation();
                Generation_with_output(testing, generations);
                Params_output(fields, teams, rounds, generations, mutations, population);
            }
            else
            {
                Console.WriteLine("Некорректный выбор");
            }
        }
        static void Data_input(ref int fields, ref int teams, ref int rounds, ref int generations, ref int mutations, ref int population)
        {
            Console.WriteLine("Параметры расписания (1 <= кол-во туров < кол-во команд <= кол-во игровых площадок)");
            Console.Write("Введите количество игровых площадок: ");
            fields = int.Parse(Console.ReadLine());
            Console.Write("Введите количество команд (четное число): ");
            teams = int.Parse(Console.ReadLine());
            Console.Write("Введите количество туров: ");
            rounds = int.Parse(Console.ReadLine());
            Console.WriteLine("\nПараметры генетического алгоритма");
            Console.Write("Введите количество поколений: ");
            generations = int.Parse(Console.ReadLine());
            Console.Write("Введите количество мутаций: ");
            mutations = int.Parse(Console.ReadLine());
            Console.Write("Введите размер поколения: ");
            population = int.Parse(Console.ReadLine());
            Console.WriteLine();
        }
        static void Generation_with_output(Schedule_generation schedule, int generations)
        {
            Schedule iteration_res = null;
            for (int i = 0; i < generations; i++)
            {
                iteration_res = schedule.Iteration_parallel();
                Console.WriteLine($"Поколение {i + 1}:");
                Console.WriteLine("Лучшее расписание поколения");
                Console.WriteLine($"Минимальное кол-во соперников: {iteration_res.Min_rivals_count}");
                Console.WriteLine($"Минимальное кол-во посещённых площадок: {iteration_res.Min_fields_visited}\n");
            }
            Console.WriteLine($"Лучшее расписание:\n{iteration_res.ToString()}");
            string[,] alter_matr = iteration_res.Alternative_schedule_form();
            string tmp = "";
            string tmp2 = "";
            var json = JsonConvert.SerializeObject(iteration_res.Matrix);
            int[,] alter_matr2 = JsonConvert.DeserializeObject<int[,]>(json);
            for (int i = 0; i < alter_matr.GetLength(0); i++)
            {
                for (int j = 0; j < alter_matr.GetLength(1); j++)
                    tmp += alter_matr[i, j] + " ";
                tmp += "\n";
            }
            for (int i = 0; i < alter_matr2.GetLength(0); i++)
            {
                for (int j = 0; j < alter_matr2.GetLength(1); j++)
                    tmp2 += (alter_matr2[i, j] + 1).ToString() + " ";
                tmp2 += "\n";
            }
            Console.WriteLine(tmp);
            Console.WriteLine(tmp2);
        }
        static void Params_output(int fields, int teams, int rounds, int generations, int mutations, int population)
        {
            Console.WriteLine($"Площадок - {fields};");
            Console.WriteLine($"Команд - {teams}");
            Console.WriteLine($"Туров - {rounds}");
            Console.WriteLine($"Поколений - {generations};");
            Console.WriteLine($"Мутаций - {mutations};");
            Console.WriteLine($"Популяция одного поколения - {population}.\n");
        }
    }
}