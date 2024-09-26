using System.Collections.ObjectModel;
using System.Runtime.InteropServices.Marshalling;
using System.Xml.XPath;

namespace Schedule_generation_library
{
    // 1 <= rounds_num < teams_num <= playing_fields_num
    // matrix S_rn = playing_field
    // r - round; n - team
    public class Population : ObservableCollection<Schedule>
    {
        public Population(List<Schedule> schedules)
        {
            foreach (Schedule elem in schedules)
                this.Add(elem);
        }
        public Population() { }
    }

    public class Schedule_generation
    {

        int fields_count;
        int teams_count;
        int rounds_count;
        List<Schedule> best_schedules;
        Population current_population;

        public Schedule_generation(int fields = 12, int teams = 10, int rounds = 8)
        {
            this.fields_count = fields;
            this.teams_count = teams;
            this.rounds_count = rounds;
            best_schedules = new List<Schedule>();
        }
        public Schedule Generate(int generations = 5, int population_count = 100)
        {
            current_population = Initial_population(population_count);
            Random rand = new Random();
            Schedule best_from_generation = null;
            for (int i = 0; i < generations; i++)
            {
                Population next_population = new Population();
                best_from_generation = Best_schedule(current_population);
                best_schedules.Add(best_from_generation);
                next_population.Add(best_from_generation);
                for (int j = 0; j < population_count / 2 - 1; j++)
                {
                    List<Schedule> parents = Panmixia(current_population);
                    Schedule child_ver_1 = Crossing(parents[0], parents[1]);
                    Schedule child_ver_2 = Crossing(parents[1], parents[0]);
                    for (int k = 0; k < 5; k++)
                    {
                        child_ver_1.Mutate(rand);
                        child_ver_2.Mutate(rand);
                    }
                    next_population.Add(child_ver_1);
                    next_population.Add(child_ver_2);
                }
                current_population = next_population;
                Console.WriteLine($"Поколение {i + 1}:");
                Console.WriteLine("Лучшее расписание поколения");
                Console.WriteLine($"Минимальное кол-во соперников: {best_from_generation.Min_rivals_count}");
                Console.WriteLine($"Минимальное кол-во посещённых площадок: {best_from_generation.Min_fields_visited}\n");
            }
            //Population tmp = new Population(best_schedules);
            //Schedule result = Best_schedule(tmp);
            Console.WriteLine($"площадок - {fields_count}; команд - {teams_count}; туров - {rounds_count};\n");
            Console.WriteLine($"Лучшее расписание:\n{best_from_generation.ToString()}");
            return best_from_generation;
        }
        private List<Schedule> Panmixia(Population schedules)
        {
            Random rand = new Random();
            List<Schedule> result = new List<Schedule>();
            for (int k = 0; k < 2; k++)
            {
                Population tmp = new Population();
                int i = rand.Next(schedules.Count);
                int j = rand.Next(schedules.Count);
                tmp.Add(schedules[i]);
                tmp.Add(schedules[j]);
                result.Add(Best_schedule(tmp));
            }
            return result;
        }
        private Schedule Best_schedule(Population schedules)
        {
            int max_min_rivals = 0;
            int max_min_fields = 0;
            Schedule result = null;
            foreach (var schedule in schedules)
            {
                if (schedule.Min_rivals_count > max_min_rivals)
                {
                    max_min_rivals = schedule.Min_rivals_count;
                    max_min_fields = schedule.Min_fields_visited;
                    result = schedule;
                }
                else if (schedule.Min_rivals_count == max_min_rivals)
                {
                    if (schedule.Min_fields_visited > max_min_fields)
                    {
                        max_min_fields = schedule.Min_fields_visited;
                        max_min_rivals = schedule.Min_rivals_count;
                        result = schedule;
                    }
                }
            }
            return result;
        }
        public Schedule Crossing(Schedule parent_1, Schedule parent_2)
        {
            int[,] tmp_1 = parent_1.Get_schedule;
            int[,] tmp_2 = parent_2.Get_schedule;
            int[,] res_schedule = new int[tmp_1.GetLength(0), tmp_1.GetLength(1)];
            //for (int i = 0; i < res_schedule.GetLength(0) / 2; i++)
            //    for (int j = 0; j < res_schedule.GetLength(1); j++)
            //        res_schedule[i, j] = tmp_1[i, j];
            //for (int i = res_schedule.GetLength(0) / 2; i < res_schedule.GetLength(0); i++)
            //    for (int j = 0; j < res_schedule.GetLength(1); j++)
            //        res_schedule[i, j] = tmp_2[i, j];
            Random rand = new Random();
            int string_num = rand.Next(this.rounds_count);
            for (int i = 0; i < string_num; i++)
                for (int j = 0; j < res_schedule.GetLength(1); j++)
                    res_schedule[i, j] = tmp_1[i, j];
            for (int i = string_num; i < res_schedule.GetLength(0); i++)
                for (int j = 0; j < res_schedule.GetLength(1); j++)
                    res_schedule[i, j] = tmp_2[i, j];
            return new Schedule(res_schedule);
        }
        private Population Initial_population(int population_count)
        {
            List<Schedule> schedules = new List<Schedule>();
            for (int i = 0; i < population_count; i++)
            {
                Schedule tmp = new Schedule(fields_count, teams_count, rounds_count);
                schedules.Add(tmp);
            }
            return new Population(schedules);
        }
    }

    public class Schedule
    {
        int playing_fields_num;
        int teams_num;
        int rounds_num;
        int[,] matrix;

        public Schedule(int fields = 12, int teams = 10, int rounds = 8)
        {
            this.playing_fields_num = fields;
            this.teams_num = teams;
            this.rounds_num = rounds;
            this.matrix = new int[rounds, teams];
            int[] fields_count = new int[playing_fields_num];
            int[] teams_count = new int[teams_num];
            var rand = new Random();
            bool flag = false;
            for (int i = 0; i < rounds_num; i++)
            {
                for (int j = 0; j < playing_fields_num; j++)
                {
                    fields_count[j] = 0;
                }
                for (int j = 0; j < teams_num; j++)
                {
                    teams_count[j] = 0;
                }
                for (int j = 0; j < teams_num / 2; j++)
                {
                    flag = false;
                    int random_field = 0;
                    int team_1 = 0;
                    int team_2 = 0;
                    while (!flag)
                    {
                        random_field = rand.Next(playing_fields_num);
                        if (fields_count[random_field] == 0)
                        {
                            team_1 = rand.Next(teams_num);
                            team_2 = rand.Next(teams_num);
                            if (team_1 != team_2 && teams_count[team_1] == 0 && teams_count[team_2] == 0)
                            {
                                fields_count[random_field]++;
                                teams_count[team_1]++;
                                teams_count[team_2]++;
                                flag = true;
                            }
                        }
                    }
                    this.matrix[i, team_1] = random_field;
                    this.matrix[i, team_2] = random_field;
                }
            }
        }
        public Schedule(int[,] data)
        {
            this.rounds_num = data.GetLength(0);
            this.teams_num = data.GetLength(1);
            this.matrix = new int[rounds_num, teams_num];
            int max = 0;
            for (int i = 0; i < this.matrix.GetLength(0); i++)
            {
                for (int j = 0; j < this.matrix.GetLength(1); j++)
                {
                    this.matrix[i, j] = data[i, j];
                    if (data[i, j] > max)
                        max = data[i, j];
                }
            }
            this.playing_fields_num = max + 1;
        }
        public int[,] Get_schedule
        {
            get { return this.matrix; }
        }
        public int Rivals_count(int team)
        {
            int count = 0;
            int[] lst = new int[teams_num];
            for (int i = 0; i < teams_num; i++)
                lst[i] = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                int current_field = matrix[i, team];
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (j != team && lst[j] == 0 && matrix[i, j] == current_field)
                    {
                        lst[j]++;
                        count++;
                    }
                }

            }
            return count;
        }
        public int Min_rivals_count
        {
            get
            {
                int min = teams_num;
                for (int i = 0; i < teams_num; i++)
                {
                    int tmp = Rivals_count(i);
                    if (tmp < min)
                        min = tmp;
                }
                return min;
            }
        }
        public int Fields_visited(int team)
        {
            int count = 0;
            int[] lst = new int[playing_fields_num];
            for (int i = 0; i < playing_fields_num; i++)
                lst[i] = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                int current_field = matrix[i, team];
                if (lst[current_field] == 0)
                {
                    lst[current_field]++;
                    count++;
                }
            }
            return count;
        }
        public int Min_fields_visited
        {
            get
            {
                int min = playing_fields_num;
                for (int i = 0; i < teams_num; i++)
                {
                    int tmp = Fields_visited(i);
                    if (tmp < min)
                        min = tmp;
                }
                return min;
            }
        }
        public void Fields_change(int round, int team_1, int team_2)
        {
            int field_1 = matrix[round, team_1];
            int field_2 = matrix[round, team_2];
            int index_1 = 0;
            int index_2 = 0;
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                if (i != team_1 && matrix[round, i] == field_1)
                {
                    index_1 = i;
                }
                if (i != team_2 && matrix[round, i] == field_2)
                {
                    index_2 = i;
                }
            }
            matrix[round, team_1] = field_2;
            matrix[round, index_1] = field_2;
            matrix[round, team_2] = field_1;
            matrix[round, index_2] = field_1;
        }
        public void Mutate(Random rand)
        {
            int r = rand.Next(rounds_num);
            int i = rand.Next(teams_num);
            int j = rand.Next(teams_num);
            Fields_change(r, i, j);
        }
        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < rounds_num; i++)
            {
                for (int j = 0; j < teams_num; j++)
                    res += String.Format("{0,3}", matrix[i, j] + 1);
                res += "\n";
            }
            return res;
        }
    }
}

