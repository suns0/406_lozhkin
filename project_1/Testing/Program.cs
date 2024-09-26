using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Schedule_generation_library;

namespace Testing
{
    class test
    {
        static void Main()
        {
            Console.WriteLine("Choose parameters option:\n");
            Console.WriteLine("Стнадартные параметры - введите 0");
            Console.WriteLine("Ввод параметров - введите 1");
            char c = Console.ReadLine()[0];
            if (c == '0')
            {
                Console.WriteLine("Поколений - 300;");
                Console.WriteLine("Популяция одного поколения - 500");
                Schedule_generation testing = new Schedule_generation();
                testing.Generate(300, 500);
            }
            else 
            {
                Console.WriteLine("Параметры расписания (1 <= кол-во туров < кол-во команд <= кол-во игровых площадок)");
                Console.Write("Введите количество игровых площадок: ");
                string fields_str = Console.ReadLine();
                int fields = int.Parse(fields_str);
                Console.Write("Введите количество команд (четное число): ");
                string teams_str = Console.ReadLine();
                int teams = int.Parse(teams_str);
                Console.Write("Введите количество туров: ");
                string rounds_str = Console.ReadLine();
                int rounds = int.Parse(rounds_str);
                Console.WriteLine("\nПараметры генетического алгоритма");
                Console.Write("Введите количество поколений: ");
                string generations_str = Console.ReadLine();
                int generations = int.Parse(generations_str);
                Console.Write("Введите размер поколения: ");
                string population_str = Console.ReadLine();
                Console.WriteLine();
                int population = int.Parse(population_str);
                Schedule_generation testing = new Schedule_generation(fields, teams, rounds);
                testing.Generate(generations, population);
            }
        }
    }
}