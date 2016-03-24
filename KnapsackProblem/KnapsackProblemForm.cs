using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KnapsackProblem
{
    public partial class KnapsackProblemForm : Form
    {
        public KnapsackProblemForm()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            var totalWeight = 0.0;
            var totalPrice = 0.0;
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                if ((bool) dataGridView1[2, index].EditedFormattedValue)
                {
                    totalWeight += Convert.ToDouble(dataGridView1[0, index].EditedFormattedValue);
                    totalPrice += Convert.ToDouble(dataGridView1[1, index].EditedFormattedValue);
                }
            }
            numericUpDown2.Value = (decimal) totalWeight;
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
                writer.WriteLine(numericUpDownCapacity.Value);
                for (var index = 0; index < dataGridView1.Rows.Count; index++)
                {
                    writer.WriteLine(dataGridView1[0, index].EditedFormattedValue + ";" +
                                     dataGridView1[1, index].EditedFormattedValue);
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
                numericUpDownCapacity.Value = Convert.ToDecimal(reader.ReadLine());
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var array = line.Split(';').Cast<object>().ToArray();
                    dataGridView1.Rows.Add(array);
                }
            }
            UpdateTotal();
        }

        /// <summary>
        ///     Нахождение решения полным перебором
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bruteForce_Click(object sender, EventArgs e)
        {
            var capacity = (double) numericUpDownCapacity.Value;
            var weights = new double[dataGridView1.Rows.Count];
            var prices = new double[dataGridView1.Rows.Count];
            var foundPrice = 0.0;
            var foundIndex = 0L;
            var mutex = new Mutex();

            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                weights[index] = Convert.ToDouble(dataGridView1[0, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[1, index].EditedFormattedValue);
            }

            if (dataGridView1.Rows.Count > 30)
            {
                MessageBox.Show("Очень долго");
                return;
            }

            // Вычисляем параллельно для всех комбинаций элементов
            Parallel.For(1L, 1L << dataGridView1.Rows.Count, bits =>
            {
                var weight = 0.0;
                var price = 0.0;
                var index = 0;
                for (var j = bits; j > 0; j >>= 1)
                {
                    if ((j & 1) == 1)
                    {
                        weight += weights[index];
                        price += prices[index];
                    }
                    index++;
                }
                if (weight > capacity) return;
                mutex.WaitOne();
                if (price >= foundPrice)
                {
                    foundPrice = price;
                    foundIndex = bits;
                }
                mutex.ReleaseMutex();
            });

            for (var index = 0; index < dataGridView1.Rows.Count; index++, foundIndex >>= 1)
            {
                dataGridView1[2, index].Value = ((foundIndex & 1) == 1);
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
            object[] array = {numericUpDown1.Value, numericUpDown4.Value};
            dataGridView1.Rows.Add(array);
        }

        /// <summary>
        ///     Метод ветвей и границ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void branchesAndBounds_Click(object sender, EventArgs e)
        {
            var capacity = (double) numericUpDownCapacity.Value;
            var weights = new double[dataGridView1.Rows.Count];
            var prices = new double[dataGridView1.Rows.Count];
            var foundPrice = 0.0;

            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                weights[index] = Convert.ToDouble(dataGridView1[0, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[1, index].EditedFormattedValue);
            }

            var list = new List<BranchesAndBoundsPlan>();
            var zero = new BranchesAndBoundsPlan();
            zero.MinWeight = 0;
            zero.MinPrice = 0;
            zero.MaxWeight = weights.Sum();
            zero.MaxPrice = prices.Sum();
            list.Add(zero);
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                foundPrice = list.Select(i => i.MinPrice).Max();
                var list1 = new List<BranchesAndBoundsPlan>();
                Parallel.ForEach(list, item =>
                {
                    var a = new BranchesAndBoundsPlan();
                    foreach (var pair in item.bools)
                    {
                        a.bools.Add(pair.Key, pair.Value);
                    }
                    a.bools.Add(index, false);
                    a.MaxWeight = item.MaxWeight - weights[index];
                    a.MaxPrice = item.MaxPrice - prices[index];
                    a.MinWeight = item.MinWeight;
                    a.MinPrice = item.MinPrice;
                    if (a.MaxPrice >= foundPrice) list1.Add(a);

                    var b = new BranchesAndBoundsPlan();
                    foreach (var pair in item.bools)
                    {
                        b.bools.Add(pair.Key, pair.Value);
                    }
                    b.bools.Add(index, true);
                    b.MaxWeight = item.MaxWeight;
                    b.MaxPrice = item.MaxPrice;
                    b.MinWeight = item.MinWeight + weights[index];
                    b.MinPrice = item.MinPrice + prices[index];
                    if (b.MinWeight <= capacity) list1.Add(b);
                });
                foundPrice = list1.Select(i => i.MinPrice).Max();
                list = list1.Where(i => i.MaxPrice >= foundPrice).ToList();
            }
            if (!list.Any()) return;
            foundPrice = list.Select(i => i.MinPrice).Max();
            var z = list.First(i => i.MinPrice == foundPrice);
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                dataGridView1[2, index].Value = z.bools[index];
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
            var capacity = (double) numericUpDownCapacity.Value;
            var weights = new double[dataGridView1.Rows.Count];
            var prices = new double[dataGridView1.Rows.Count];

            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                weights[index] = Convert.ToDouble(dataGridView1[0, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[1, index].EditedFormattedValue);
            }

            var stack = new Stack<BranchesAndBoundsPlan>();
            var zero = new BranchesAndBoundsPlan();
            zero.MinWeight = 0;
            zero.MinPrice = 0;
            zero.MaxWeight = weights.Sum();
            zero.MaxPrice = prices.Sum();
            stack.Push(zero);
            var foundPrice = 0.0;
            var foundPlan = zero;
            while (stack.Any())
            {
                var item = stack.Pop();
                do
                {
                    if (item.bools.Count == dataGridView1.Rows.Count)
                    {
                        if (item.MinPrice > foundPrice && item.MaxWeight < capacity)
                        {
                            foundPrice = item.MinPrice;
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
                        b.MaxWeight = item.MaxWeight;
                        b.MaxPrice = item.MaxPrice;
                        b.MinWeight = item.MinWeight + weights[index];
                        b.MinPrice = item.MinPrice + prices[index];
                        if (b.MaxPrice >= foundPrice && b.MinWeight < capacity) stack.Push(b); // отсечение границей

                        index = item.bools.Count;
                        item.bools.Add(index, false);
                        item.MaxWeight = item.MaxWeight - weights[index];
                        item.MaxPrice -= prices[index];
                    }
                } while (item != null);
            }
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                dataGridView1[2, index].Value = foundPlan.bools[index];
            }
            UpdateTotal();
            SystemSounds.Beep.Play();
        }

        private void dynaPro_Click(object sender, EventArgs e)
        {
            var count = dataGridView1.Rows.Count;
            var capacity = (int) numericUpDownCapacity.Value;
            var weights = new int[count];
            var prices = new double[count];

            for (var index = 0; index < count; index++)
            {
                weights[index] = Convert.ToInt32(dataGridView1[0, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[1, index].EditedFormattedValue);
            }


            // алгоритм Беллмана
            var m = new double[count + 1, capacity + 1];
            var x = new long[count + 1, capacity + 1];
            for (var j = 0; j <= capacity; j++)
            {
                m[0, j] = 0;
                x[0, j] = 0;
            }
            for (var i = 1; i <= count; i++)
                for (var j = 1; j <= capacity; j++)
                    if (weights[i - 1] > j)
                    {
                        m[i, j] = m[i - 1, j];
                        x[i, j] = x[i - 1, j];
                    }
                    else if (m[i - 1, j] > m[i - 1, j - weights[i - 1]] + prices[i - 1])
                    {
                        m[i, j] = m[i - 1, j];
                        x[i, j] = x[i - 1, j];
                    }
                    else
                    {
                        m[i, j] = m[i - 1, j - weights[i - 1]] + prices[i - 1];
                        x[i, j] = x[i - 1, j - weights[i - 1]] + (1L << (i - 1));
                    }

            for (var index = 0; index < count; index++)
            {
                dataGridView1[2, index].Value = (((x[count, capacity] >> index) & 1L) == 1L);
            }
            UpdateTotal();
            SystemSounds.Beep.Play();
        }
    }
}