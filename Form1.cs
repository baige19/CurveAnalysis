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

            for (int i = 0; i < xs.Length; i++)
                yFit[i] = L_fit / (1 + Math.Exp(-k_fit * (xs[i] - x0_fit)));

            int K = 200;            //斜率参数

            //梯形数据
            double x_trapezoid_fit = 0; //曲线向左偏移参数
            double[] X_trapezoid = new double[K];
            double[] Y_trapezoid = new double[K];

            for (int i = 0; i < X_trapezoid.Length; i++)
            {
                if (i == 0)
                    Y_trapezoid[i] = TrapezoidCurve(i, K);
                else
                    Y_trapezoid[i] = TrapezoidCurve(i, K)  + Y_trapezoid[i - 1];
                X_trapezoid[i] = i / 1000f + x_trapezoid_fit;
            }

            //次方数据
            double[] X_pow = new double[K];
            double[] Y_pow3 = new double[K];
            double[] Y_pow5 = new double[K];

            for (int i = 0; i < K; i++)
            {
                X_pow[i] = i / 1000f;
                //3次方数据
                Y_pow3[i] = CurvePow3(i, K, 0, 10);
                //5次方数据
                Y_pow5[i] = CurvePow5(i, K, 0, 10);
            }

            // --------------------------
            // 获取 Plot 对象
            var plt = formsPlot1.Plot;
            plt.Clear();

            // 绘制原始数据
            var scatter1 = plt.AddScatter(xs, ys, System.Drawing.Color.Blue);
            scatter1.MarkerShape = MarkerShape.filledCircle;
            scatter1.MarkerSize = 5f;
            scatter1.Label = "原始数据";

            // 绘制拟合曲线
            var scatter2 = plt.AddScatter(xs, yFit, System.Drawing.Color.Red);
            scatter2.MarkerShape = MarkerShape.none;
            scatter2.MarkerSize = 0f;
            scatter2.Label = $"拟合公式: y = {L_fit:F3}/(1 + exp(-{k_fit:F3}*(x - {x0_fit:F3})))";

            // 绘制梯形数据
            var scatter3 = plt.AddScatter(X_trapezoid, Y_trapezoid, System.Drawing.Color.Yellow);
            scatter3.MarkerShape = MarkerShape.filledCircle;
            scatter3.MarkerSize = 0f;
            scatter3.Label = "梯形数据";

            //绘制3次方数据
            var scatter4 = plt.AddScatter(X_pow, Y_pow3, System.Drawing.Color.Green);
            scatter4.MarkerShape = MarkerShape.filledCircle;
            scatter4.MarkerSize = 0f;
            scatter4.Label = "3次方数据";

            //绘制5次方数据
            var scatter5 = plt.AddScatter(X_pow, Y_pow5, System.Drawing.Color.Orange);
            scatter5.MarkerShape = MarkerShape.filledCircle;
            scatter5.MarkerSize = 0f;
            scatter5.Label = "5次方数据";

            // 显示图例
            plt.Legend();
            formsPlot1.Refresh();
        }

        //梯形曲线公式
        private double TrapezoidCurve(double x, double k)
        {
            double y;       //返回值
            double x0, x1;  //阶梯分界点
            double a, b, c; //阶梯函数系数

            a = 90;
            b = 20;
            c = 27.5;

            x0 = (b / k) / (a / (k * k));
            x1 = (b / k - c / k) / (-c / (k * k));

            if (x < x0)
            {
                y = x * a / (k * k);
            }
            else if (x >= x0 && x < x1)
            {
                y = b / k;
            }
            else
            {
                y = x * (-c) / (k * k) + c / k;
            }
            return y;
        }

        //3次方曲线公式
        private double CurvePow3(double x, double k, double yStart, double ySet)
        {
            double y;       //返回值

            x = x / k;      //归一化，将时间限制在0到1之间

            y = yStart + (ySet - yStart) * (3 * Math.Pow(x, 2) - 2 * Math.Pow(x, 3));

            return y < ySet ? y : ySet;
        }

        //5次方曲线公式
        private double CurvePow5(double x, double k, double yStart, double ySet)
        {
            double y;       //返回值

            x = x / k;      //归一化，将时间限制在0到1之间

            y = yStart + (ySet - yStart) * (6 * Math.Pow(x, 5) - 15 * Math.Pow(x, 4) + 10 * Math.Pow(x, 3));

            return y < ySet ? y : ySet;
        }

    }
}