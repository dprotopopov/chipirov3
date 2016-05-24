using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravelingSalesmanProblem
{
    public partial class TravelingSalesmanProblemForm : Form
    {
        public TravelingSalesmanProblemForm()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            var totalPrice = 0.0;
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                if ((bool) dataGridView1[3, index].EditedFormattedValue)
                {
                    totalPrice += Convert.ToDouble(dataGridView1[2, index].EditedFormattedValue);
                }
            }
            numericUpDown3.Value = (decimal) totalPrice;
        }

        /// <summary>
        ///     Сохранение файла с параметрами задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveFile_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            using (var writer = File.CreateText(saveFileDialog1.FileName))
            {
                for (var index = 0; index < dataGridView1.Rows.Count; index++)
                {
                    writer.WriteLine(dataGridView1[0, index].EditedFormattedValue + ";" +
                                     dataGridView1[1, index].EditedFormattedValue + ";" +
                                     dataGridView1[2, index].EditedFormattedValue);
                }
            }
        }

        /// <summary>
        ///     Чтение файла с параметрами задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            using (var reader = File.OpenText(openFileDialog1.FileName))
            {
                dataGridView1.Rows.Clear();
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var array = line.Split(';').Cast<object>().ToArray();
                    dataGridView1.Rows.Add(array);
                }
            }
            UpdateTotal();
        }

        /// <summary>
        ///     Провека графа на простой цикл
        /// </summary>
        /// <returns></returns>
        private bool IsCyclic(IList<string> sources, IList<string> destinations)
        {
            Debug.Assert(sources.Count == destinations.Count);

            var map = new Dictionary<string, string>();
            for (var index = 0; index < sources.Count; index++)
            {
                if (map.ContainsKey(sources[index])) return false; // в цикле все точки отправления уникальные
                map.Add(sources[index], destinations[index]);
            }
            var item = map.Keys.First();
            var list = new List<string>();
            for (var j = 0; j < map.Count; j++)
            {
                list.Add(item = map[item]);
            }
            return list.Count == list.Distinct().Count();
        }

        /// <summary>
        ///     Нахождение решения полным перебором
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bruteForce_Click(object sender, EventArgs e)
        {
            var sources = new string[dataGridView1.Rows.Count];
            var destinations = new string[dataGridView1.Rows.Count];
            var prices = new double[dataGridView1.Rows.Count];
            var foundPrice = double.MaxValue;
            var foundIndex = 0L;
            var mutex = new Mutex();
            var cities = new List<string>();
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                sources[index] = Convert.ToString(dataGridView1[0, index].EditedFormattedValue);
                destinations[index] = Convert.ToString(dataGridView1[1, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[2, index].EditedFormattedValue);
                cities.Add(sources[index]);
                cities.Add(destinations[index]);
            }
            cities = cities.Distinct().ToList();

            // Вычисляем параллельно для всех комбинаций элементов
            Parallel.For(1L, 1L << dataGridView1.Rows.Count, bits =>
            {
                // Каждый бит отвечает за использование ориентированного ребра графа
                var price = 0.0;

                var count = 0;
                for (var j = bits; j > 0; j >>= 1)
                {
                    if ((j & 1) == 1) count++;
                }
                if (count != cities.Count) return;

                var index = 0;
                var s = new List<string>();
                var d = new List<string>();

                for (var j = bits; j > 0; j >>= 1, index++)
                {
                    if ((j & 1) == 0) continue;
                    s.Add(sources[index]);
                    d.Add(destinations[index]);
                }
                if (!IsCyclic(s, d)) return; // проверка что цикл

                index = 0;
                for (var j = bits; j > 0; j >>= 1, index++)
                {
                    if ((j & 1) == 1)
                    {
                        price += prices[index];
                    }
                }
                mutex.WaitOne();
                if (price < foundPrice)
                {
                    foundPrice = price;
                    foundIndex = bits;
                }
                mutex.ReleaseMutex();
            });

            for (var index = 0; index < dataGridView1.Rows.Count; index++, foundIndex >>= 1)
            {
                dataGridView1[3, index].Value = ((foundIndex & 1) == 1);
            }
            UpdateTotal();
            SystemSounds.Beep.Play();
        }

        /// <summary>
        ///     Добавление одного предмета указанного веса и цены
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addItem_Click(object sender, EventArgs e)
        {
            object[] array = {textBox1.Text, textBox2.Text, numericUpDown4.Value};
            dataGridView1.Rows.Add(array);
        }

        private void branchesAndBounds_Click(object sender, EventArgs e)
        {
            var count = dataGridView1.Rows.Count;
            var sources = new string[count];
            var destinations = new string[count];
            var prices = new double[count];
            var foundPrice = double.MaxValue;
            var cities = new List<string>();
            for (var index = 0; index < count; index++)
            {
                sources[index] = Convert.ToString(dataGridView1[0, index].EditedFormattedValue);
                destinations[index] = Convert.ToString(dataGridView1[1, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[2, index].EditedFormattedValue);
                cities.Add(sources[index]);
                cities.Add(destinations[index]);
            }
            cities = cities.Distinct().ToList();
            var list = new List<BranchesAndBoundsPlan>();
            var list2 = new List<BranchesAndBoundsPlan>();
            var zero = new BranchesAndBoundsPlan
            {
                MinCount = 0,
                MinPrice = 0,
                MaxCount = dataGridView1.Rows.Count,
                MaxPrice = prices.Sum()
            };
            list.Add(zero);
            for (var index = 0; index < count; index++)
            {
                var list1 = new List<BranchesAndBoundsPlan>();
                foreach (var item in list)
                {
                    var s = new List<string>();
                    var d = new List<string>();

                    for (var j = 0; j < count; j++)
                    {
                        if (item.bools.ContainsKey(j) && item.bools[j])
                        {
                            s.Add(sources[j]);
                            d.Add(destinations[j]);
                        }
                    }
                    if (s.Count == cities.Count && IsCyclic(s, d))
                    {
                        var price = 0.0;
                        for (var j = 0; j < dataGridView1.Rows.Count; j++)
                            if (item.bools.ContainsKey(j) && item.bools[j])
                                price += prices[j];
                        foundPrice = Math.Min(foundPrice, price);
                        list2.Add(item);
                    }
                    else
                    {
                        var a = new BranchesAndBoundsPlan();
                        foreach (var pair in item.bools)
                        {
                            a.bools.Add(pair.Key, pair.Value);
                        }
                        a.bools.Add(index, false);
                        a.MaxCount = item.MaxCount - 1;
                        a.MinCount = item.MinCount;
                        a.MaxPrice = item.MaxPrice - prices[index];
                        a.MinPrice = item.MinPrice;
                        if (a.MaxCount >= cities.Count)
                        {
                            list1.Add(a);
                        }

                        var b = new BranchesAndBoundsPlan();
                        foreach (var pair in item.bools)
                        {
                            b.bools.Add(pair.Key, pair.Value);
                        }
                        b.bools.Add(index, true);
                        b.MaxCount = item.MaxCount;
                        b.MinCount = item.MinCount + 1;
                        b.MaxPrice = item.MaxPrice;
                        b.MinPrice = item.MinPrice + prices[index];
                        if (b.MinCount <= cities.Count)
                        {
                            list1.Add(b);
                        }
                    }
                }
                list = list1.Where(i => i.MinPrice <= foundPrice).ToList();
            }
            if (!list2.Any()) return;
            var z = list2.First(i => Math.Abs(i.MinPrice - foundPrice) < 0.001);
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                dataGridView1[3, index].Value = z.bools.ContainsKey(index) && z.bools[index];
            }
            UpdateTotal();
            SystemSounds.Beep.Play();
        }

        /// <summary>
        ///     Метод поска с возвратом
        ///     При поиске не добавляются в стек плохие направления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void branchesAndBoundsAndReturn_Click(object sender, EventArgs e)
        {
            var count = dataGridView1.Rows.Count;
            var sources = new string[count];
            var destinations = new string[count];
            var prices = new double[count];
            var cities = new List<string>();
            for (var index = 0; index < count; index++)
            {
                sources[index] = Convert.ToString(dataGridView1[0, index].EditedFormattedValue);
                destinations[index] = Convert.ToString(dataGridView1[1, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[2, index].EditedFormattedValue);
                cities.Add(sources[index]);
                cities.Add(destinations[index]);
            }
            cities = cities.Distinct().ToList();

            var stack = new Stack<BranchesAndBoundsPlan>();
            var zero = new BranchesAndBoundsPlan();
            zero.MinCount = 0;
            zero.MaxCount = count;
            zero.MinPrice = 0;
            zero.MaxPrice = prices.Sum();
            stack.Push(zero);
            var foundPrice = double.MaxValue;
            var foundPlan = zero;
            while (stack.Any())
            {
                var item = stack.Pop();
                do
                {
                    if (item.bools.Count == count)
                    {
                        var s = new List<string>();
                        var d = new List<string>();

                        for (var j = 0; j < count; j++)
                        {
                            if (item.bools.ContainsKey(j) && item.bools[j])
                            {
                                s.Add(sources[j]);
                                d.Add(destinations[j]);
                            }
                        }
                        if (item.MaxPrice <= foundPrice && s.Count == cities.Count && IsCyclic(s, d))
                        {
                            foundPrice = item.MaxPrice;
                            foundPlan = item;
                        }
                        item = null;
                    }
                    else
                    {
                        var b = new BranchesAndBoundsPlan();
                        foreach (var pair in item.bools)
                        {
                            b.bools.Add(pair.Key, pair.Value);
                        }
                        var index = b.bools.Count;
                        b.bools.Add(index, true);
                        b.MaxCount = item.MaxCount;
                        b.MinCount = item.MinCount + 1;
                        b.MaxPrice = item.MaxPrice;
                        b.MinPrice = item.MinPrice + prices[index];
                        if (b.MaxCount >= cities.Count && b.MinPrice <= foundPrice) stack.Push(b); // отсечение границей

                        index = item.bools.Count;
                        item.bools.Add(index, false);
                        item.MaxCount -= 1;
                        item.MaxPrice -= prices[index];
                    }
                } while (item != null);
            }
            for (var index = 0; index < count; index++)
            {
                dataGridView1[3, index].Value = foundPlan.bools[index];
            }
            UpdateTotal();
            SystemSounds.Beep.Play();
        }

        private void dynaPro_Click(object sender, EventArgs e)
        {
            var count = dataGridView1.Rows.Count;
            var sources = new string[count];
            var destinations = new string[count];
            var prices = new double[count];
            var cities = new List<string>();
            for (var index = 0; index < count; index++)
            {
                sources[index] = Convert.ToString(dataGridView1[0, index].EditedFormattedValue);
                destinations[index] = Convert.ToString(dataGridView1[1, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[2, index].EditedFormattedValue);
                cities.Add(sources[index]);
                cities.Add(destinations[index]);
            }
            cities = cities.Distinct().ToList();

            var vertics = Enumerable.Range(0, cities.Count).ToArray();
            var matrix = new double[cities.Count, cities.Count];

            for (var i = 0; i < cities.Count; i++)
                for (var j = 0; j < cities.Count; j++)
                    if (i == j)
                        matrix[i, j] = 0;
                    else
                    {
                        matrix[i, j] = 1000000;
                    }

            for (var index = 0; index < count; index++)
            {
                matrix[cities.IndexOf(sources[index]), cities.IndexOf(destinations[index])] = prices[index];
            }

            // Held–Karp algorithm
            var DynamicProgramming = new TspDynamicProgramming(vertics, matrix);
            double cost;
            var route = DynamicProgramming.Solve(out cost);
            MessageBox.Show(string.Join("->", route.Select(i => cities[i])) + "\n" + cost);
        }
    }
}