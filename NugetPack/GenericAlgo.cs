using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace NugetPack
{
    public class GenericAlgo
    {

        private List<Solution> population = new List<Solution>();
        private Random random = new();
        private object Populatin_index_lock = new object();
        public event Action<int> ItUpdated;
        public event Action<int> SqUpdated;

        public void Initialize(int n_obs_1, int n_obs_2, int n_obs_3, int population_size)
        {
            int index = 0;
            Parallel.For(0, population_size, i =>
            {
                while (index < population_size) 
                {
                    var list = new List<Rectangle>();
                    int n_obs = n_obs_1 + n_obs_2 + n_obs_3;
                    int max_l = n_obs; 
                    for (int j = 0; j < n_obs_1; ++j)
                    {
                        list.Add(new Rectangle(random.Next(0, max_l), random.Next(0, max_l), 1, 1));
                        
                    }
                    for (int j = 0; j < n_obs_2; ++j)
                    {
                        list.Add(new Rectangle(random.Next(0, max_l), random.Next(0, max_l), 2, 2));

                    }
                    for (int j = 0; j < n_obs_3; ++j)
                    {
                        list.Add(new Rectangle(random.Next(0, max_l), random.Next(0, max_l), 3, 3));

                    }

                    var sol = new Solution(list);
                    if (sol.IsValid())
                    {
                        /// Критическая секция над популяцией и количеством элементов в ней
                        lock(Populatin_index_lock)
                        {
                            if (index < population_size) {
                                population.Add(sol);
                                index++;
                            }
                        }
                        
                    }
                }
            });
            
        }



        private Solution Crossover(Solution parent1, Solution parent2)
        {
            var sqs = new List<Rectangle>();

            ///Паралелизм невыгоден
            for (int i = 0; i < parent1.rectangles.Count; ++i) 
            {
                if (random.Next(0, 2) > 0)
                {
                    sqs.Add(parent1.rectangles[i]);
                }
                else
                {
                     sqs.Add(parent2.rectangles[i]);    
                }
            }

            return new Solution(sqs);

        }


        private void Mutate(Solution sol, int l, int r)
        {
            var pos = random.Next(l, Math.Min(r, sol.rectangles.Count));
            var rec = sol.rectangles[pos];

            ///foreach (var rec in  sol.rectangles) {
                // if (random.Next(0, 10) < 1)
                // {
                    int dx = random.Next(-4, 4);

                    sol.rectangles[pos].x_l += dx;
                    sol.rectangles[pos].x_r += dx;
                    
                // }
                // if (random.Next(0, 10) <= 1)
                // {
                    int dy = random.Next(-4, 4);
                    sol.rectangles[pos].y_t += dy;
                    sol.rectangles[pos].y_b += dy;
                // } 
                
            ///}

            sol.count_metric();
        }



        public void Evolute(int iteration = 0)
        {
            List<Solution> newPopulation = new List<Solution>();
            var SortedSolutions = population.OrderBy(s => s.count_metric()).ToList();
            List<Solution> SelectedPopulation = new List<Solution>();
            for (int i = 0; i < SortedSolutions.Count / 4;  ++i)
            {
                SelectedPopulation.Add((Solution) SortedSolutions[i].Clone());
                SelectedPopulation.Add((Solution)SortedSolutions[
                random.Next(SortedSolutions.Count / 4, SortedSolutions.Count / 4 * 3)].Clone());
            }

            if (iteration % 100 == 0) {
                for (int i = 0; i < SelectedPopulation.Count; ++i)
                {
                    var pair = random.Next(0, SelectedPopulation.Count);
                    if (pair != i)
                    {
                        Solution tmp = SelectedPopulation[pair];
                        SelectedPopulation[pair] = SelectedPopulation[i];
                        SelectedPopulation[i] = tmp;
                    }
                }
            }
            


            Parallel.For(0, population.Count, (i) => 
            {
                while (newPopulation.Count < population.Count)
                {
                    var parent1 = SelectedPopulation[random.Next(0, SelectedPopulation.Count)];
                    var parent2 = SelectedPopulation[random.Next(0, SelectedPopulation.Count)];

                    var child = Crossover(parent1, parent2);
                    int n_mutations = random.Next(1, 5);
                    n_mutations = 1;
                    for (int k = 0; n_mutations > k; ++k)
                    {
                        int part = child.rectangles.Count / n_mutations;
                        Mutate(child, part * k, part * (k + 1));
                    }
                    

                    // Критическая секция
                    if (child.IsValid())
                    {
                        lock(Populatin_index_lock) {
                            if (newPopulation.Count < population.Count) {
                                newPopulation.Add(child);
                            }
                        }
                        
                    }
                    
                }
            });

           

            var solutions = newPopulation.OrderBy(s => s.count_metric()).ToList();
            for (int i = 0; SelectedPopulation.Count < population.Count; ++i) 
            {
                SelectedPopulation.Add(solutions[i]);
            }

            
            this.population.Clear();
            for (int i = 0; i < SelectedPopulation.Count; ++i)
            {
                this.population.Add(SelectedPopulation[i]);
            }
            this.population = population.OrderBy(s => s.count_metric()).ToList();

        }

        ///Генерация решения будет последовательной => распаралелить нельзя.
        public Solution GetBestSol(int max_iters, CancellationToken token, Action<int, int, Solution> onProgressUpdate)
        {
            Solution best_sol = new Solution(new List<Rectangle> ());
            for (int i = 0; i < max_iters; ++i)
            {
                var sorted = population.OrderBy(s =>  s.count_metric()).ToList();
                population = sorted;
                best_sol = (Solution) sorted[0].Clone();

                onProgressUpdate(i, best_sol.count_metric(), best_sol);
                Console.WriteLine($"Поколение : {i}, \n Площадь: {best_sol.count_metric()} \n");
                foreach (var sq in best_sol.rectangles)
                {
                    ///Console.WriteLine($"x_l: {sq.x_l} y_b: {sq.y_b} w: {sq.weight} h: {sq.height}");
                }

                this.Evolute(i);


                if (token.IsCancellationRequested)
                {
                    return best_sol;
                }
                ///ItUpdated?.Invoke(i);
                ///SqUpdated?.Invoke(best_sol.Metric);
            }

            return best_sol;
        }
    } 

    
}


