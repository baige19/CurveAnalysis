using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ScottPlot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace CurveAnalysis
{
    public partial class Form1 : Form
    {
      
        public Form1()
        {
            InitializeComponent();

            LoadAndPlotCsv("data400.csv");
        }

        private void LoadAndPlotCsv(string filePath)
        {
            var xList = new List<double>();
            var yList = new List<double>();

            // 读取 CSV 文件
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 2 &&
                            double.TryParse(parts[0], out double x) &&
                            double.TryParse(parts[1], out double y))
                        {
                            xList.Add(x);
                            yList.Add(y);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取 CSV 出错: " + ex.Message);
                return;
            }

            double[] xs = xList.ToArray();
            double[] ys = yList.ToArray();

            // --------------------------
            // 自动生成初始猜测 [L, k, x0]
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            foreach (var y in ys)
            {
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            double L0 = maxY - minY;
            double k0 = 30; // 可根据数据陡度调整
            double x0_0 = xs[xs.Length / 2];

            var initialGuess = Vector<double>.Build.DenseOfArray(new double[] { L0, k0, x0_0 });

            // --------------------------
            // 定义平方误差目标函数
            Func<Vector<double>, double> objective = par =>
            {
                double L = par[0];
                double k = par[1];
                double x0 = par[2];
                double sum = 0;
                for (int i = 0; i < xs.Length; i++)
                {
                    double yPred = L / (1 + Math.Exp(-k * (xs[i] - x0)));
                    double error = ys[i] - yPred;
                    sum += error * error;
                }
                return sum;
            };

            // --------------------------
            // 使用 Nelder-Mead Simplex 优化器（不需要梯度）
            var optimizer = new NelderMeadSimplex(1e-6, 1000);
            var result = optimizer.FindMinimum(ObjectiveFunction.Value(objective), initialGuess);

            double L_fit = result.MinimizingPoint[0];
            double k_fit = result.MinimizingPoint[1];
            double x0_fit = result.MinimizingPoint[2];

            // 生成拟合曲线
            double[] yFit = new double[xs.Length];
            double[] yyFit = new double[400*1000];
            double[] xxFit = new double[yyFit.Length];
            for (int i = 0; i < xs.Length; i++)
                yFit[i] = L_fit / (1 + Math.Exp(-k_fit * (xs[i] - x0_fit)));

            for (int i = 0; i < yyFit.Length; i++)
            {
                if(i == 0)
                    yyFit[i] = MyCurve(i, yyFit.Length/1000);
                else
                    yyFit[i] = MyCurve(i, yyFit.Length/1000) *0.001 + yyFit[i-1] ;
                xxFit[i] = (i/1000f)/100/10-0.15;
            }




            // --------------------------
            // 获取 Plot 对象
            var plt = formsPlot1.Plot;
            plt.Clear();

            // 下采样显示（最多 10000 点）
            int maxPoints = 10000;
            int step = Math.Max(1, xs.Length / maxPoints);
            int sampleCount = xs.Length / step;
            var xsSample = new double[sampleCount];
            var ysSample = new double[sampleCount];
         
            for (int i = 0, j = 0; i < xs.Length; i += step, j++)
            {
                xsSample[j] = xs[i];
                ysSample[j] = ys[i];
            }

            // 绘制原始数据
            var scatter1 = plt.AddScatter(xsSample, ysSample, System.Drawing.Color.Blue);
            scatter1.MarkerShape = MarkerShape.filledCircle;
            scatter1.MarkerSize = 5f;
            scatter1.Label = "原始数据";



            // 绘制拟合曲线
            var scatter2 = plt.AddScatter(xs, yFit, System.Drawing.Color.Red);
            scatter2.MarkerShape = MarkerShape.none;
            scatter2.MarkerSize = 0f;
            scatter2.Label = $"拟合公式: y = {L_fit:F3}/(1 + exp(-{k_fit:F3}*(x - {x0_fit:F3})))";


            // 绘制梯形数据
            var scatter3 = plt.AddScatter(xxFit, yyFit, System.Drawing.Color.Yellow);
            scatter1.MarkerShape = MarkerShape.filledCircle;
            scatter1.MarkerSize = 0f;
            scatter1.Label = "梯形数据";

            // 显示图例
            plt.Legend();
            formsPlot1.Refresh();

            for(double i = 0;i<15;i++)
            {
                Console.WriteLine((6*Math.Pow(i/15,5)- 15 * Math.Pow(i/15, 4)+ 10 * Math.Pow(i/15, 3))*10.44);
            }
        }

        private double MyCurve(double t, double k)
        {
            double y,x0,x1,a,b,c;

            a = 90;
            b = 20;
            c = 27.5;

            t = t / 1000f;

            x0 = (b / k) / (a / (k * k));
            x1 = (b / k - c / k) / (-c / (k * k));

            //Console.WriteLine((((x1 - x0)+k)*b/k)/2);

            if (t < x0)
            {
                y = a * t / (k * k);
            }
            else if (t >= x0 && t < x1)
            {
                y = b / k;
            }
            else
            {
                y = -c * t / (k * k) + c / k;
            }
            return y;
        }

    }
}