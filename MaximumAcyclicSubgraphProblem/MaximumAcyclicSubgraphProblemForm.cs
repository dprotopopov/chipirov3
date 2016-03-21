﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaximumAcyclicSubgraphProblem
{
    public partial class MaximumAcyclicSubgraphProblemForm : Form
    {
        public MaximumAcyclicSubgraphProblemForm()
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
        ///     Провека графа на ацикличность
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        static public bool IsAcyclic(IList<string> sources, IList<string> destinations)
        {
            Debug.Assert(sources.Count==destinations.Count);
            for (var list = sources.Distinct().Except(destinations.Distinct());
                list.Any();
                list = sources.Distinct().Except(destinations.Distinct()))
            {
                for(var i=sources.Count;i-->0;)
                {
                    if (!list.Contains(sources[i])) continue;
                    // Удаляем ребра с вершинами в истоках
                    sources.RemoveAt(i);
                    destinations.RemoveAt(i);
                }
            }
            return sources.Count == 0;
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
            for (var index = 0; index < dataGridView1.Rows.Count; index++)
            {
                sources[index] = Convert.ToString(dataGridView1[0, index].EditedFormattedValue);
                destinations[index] = Convert.ToString(dataGridView1[1, index].EditedFormattedValue);
                prices[index] = Convert.ToDouble(dataGridView1[2, index].EditedFormattedValue);
            }

            if (dataGridView1.Rows.Count > 30)
            {
                MessageBox.Show("Очень долго");
                return;
            }

            // Вычисляем параллельно для всех комбинаций элементов
            Parallel.For(1L, 1L << dataGridView1.Rows.Count, bits =>
            {
                // Каждый бит отвечает за использование ориентированного ребра графа
                var price = 0.0;
                var s = new List<string>();
                var d = new List<string>();
                var index = 0;
                for (var j = bits; index < dataGridView1.Rows.Count; j >>= 1, index++)
                {
                    if ((j & 1) == 1) continue;
                    s.Add(sources[index]);
                    d.Add(destinations[index]);
                }
                if (!IsAcyclic(s,d)) return; // проверка графа на ацикличность

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
        }

        /// <summary>
        ///     Добавление одного предмета указанного веса и цены
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addItem_Click(object sender, EventArgs e)
        {
            object[] array = {numericUpDown1.Value, numericUpDown2.Value, numericUpDown4.Value};
            dataGridView1.Rows.Add(array);
        }
    }
}