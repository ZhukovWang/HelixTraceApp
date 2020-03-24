//Copyright (c) 2018 Bruce Greene

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//of the Software, and to permit persons to whom the Software is furnished to do
//so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace HelixTraceDemoApp
{
    public partial class MainWindow : Window
    {
        private struct Point3DPlus
        {
            public Point3DPlus(Point3D point, Color color, double thickness)
            {
                this.point = point;
                this.color = color;
                this.thickness = thickness;
            }

            public Point3D point;
            public Color color;
            public double thickness;
        }

        private List<Point3DPlus> points = new List<Point3DPlus>();
        private Stopwatch stopwatch = Stopwatch.StartNew();

        private string filePath;

        public MainWindow()
        {
            InitializeComponent();

            OpenFile();

            var bw = new BackgroundWorker();
            bw.DoWork += GatherData;
            bw.RunWorkerAsync();
        }

        private void OpenFile()
        {
            var openDialog = new System.Windows.Forms.OpenFileDialog();
            openDialog.Multiselect = false;
            openDialog.CheckPathExists = true;

            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = openDialog.FileName;

                if (!File.Exists(filePath))
                {
                    System.Windows.MessageBox.Show("File is NOT exists!", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private void GatherData(object sender, DoWorkEventArgs e)
        {
            var readDataStream = new System.IO.StreamReader(filePath, Encoding.UTF8);

            while (true)
            {
                Thread.Sleep(5);  // 50ms data sampling period

                //Generate a test trace: an upward spiral with square corners

                #region old test code

                //double t = stopwatch.Elapsed.TotalSeconds * 0.25;
                //double sint = Math.Sin(t);
                //double cost = Math.Cos(t);
                //double x, y, z = t * 0.5;
                //Color color;

                //if (sint > 0.0 && cost > 0.0)
                //{
                //    if (sint > cost)
                //    {
                //        x = 100.0;
                //        y = 70.71 * cost + 50.0;
                //    }
                //    else
                //    {
                //        x = 70.71 * sint + 50.0;
                //        y = 100.0;
                //    }
                //    color = Colors.Red;
                //}
                //else if (sint < 0.0 && cost < 0.0)
                //{
                //    if (sint < cost)
                //    {
                //        x = 0.0;
                //        y = 70.71 * cost + 50.0;
                //    }
                //    else
                //    {
                //        x = 70.71 * sint + 50.0;
                //        y = 0.0;
                //    }
                //    color = Colors.Red;
                //}
                //else
                //{
                //    x = 50.0 * sint + 50.0;
                //    y = 50.0 * cost + 50.0;
                //    color = Colors.Blue;
                //}

                #endregion old test code

                Color color = Colors.Black;
                double x = 0, y = 0, z = 0;

                if (readDataStream.EndOfStream)
                {
                    break;
                }

                var readDataLine = readDataStream.ReadLine();

                var readDataNumbers = readDataLine.Split(' ');

                try
                {
                    x = Convert.ToDouble(readDataNumbers[0]);
                    y = Convert.ToDouble(readDataNumbers[1]);

                    if (readDataNumbers.Length >= 3)
                    {
                        z = Convert.ToDouble(readDataNumbers[2]);
                    }
                    else
                    {
                        z = 0;
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("Data format is error. The format is 'x y z'.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Environment.Exit(-1);
                }

                var point = new Point3DPlus(new Point3D(x, y, z), color, 1.5);
                bool invoke = false;
                lock (points)
                {
                    points.Add(point);
                    invoke = (points.Count == 1);
                }

                if (invoke)
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)PlotData);
            }
        }

        private void PlotData()
        {
            if (points.Count == 1)
            {
                Point3DPlus point;
                lock (points)
                {
                    point = points[0];
                    points.Clear();
                }

                plot.AddPoint(point.point, point.color, point.thickness);
            }
            else
            {
                Point3DPlus[] pointsArray;
                lock (points)
                {
                    pointsArray = points.ToArray();
                    points.Clear();
                }

                foreach (Point3DPlus point in pointsArray)
                    plot.AddPoint(point.point, point.color, point.thickness);
            }
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            plot.ZoomExtents(500);  // zoom to extents
                                    //plot.ResetCamera();  // orient and zoom
        }
    }
}
