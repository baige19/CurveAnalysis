using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ScottPlot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using System.Linq;
using ScottPlot.Plottable;
using System.Text.RegularExpressions;

namespace CurveAnalysis
{
    public partial class Form1 : Form
    {
        private int K;//斜率参数
        private string currentFilePath;//当前文件路径
        private double VStart = 0;//起点电压
        private double VSet;//终点电压
        private double VMax = 10;//满量程电压
        private double X_Move;//偏移坐标
        private int V_T;//升至终点电压所需的时间
        public Form1()
        {
            InitializeComponent();
            K = 100;
            VSet = 10;
            X_Move = 0;

            V_T = (int)Math.Ceiling(K * (VSet - VStart) / VMax);
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

            // --------------------------
            // 获取 Plot 对象
            var plt = formsPlot1.Plot;
            plt.Clear();

            (var xsSmall, var ysSmall) = Downsample(xs, ys, 1000);

            // 绘制原始数据
            var scatter1 = plt.AddScatter(xsSmall, ysSmall, System.Drawing.Color.Blue);
            scatter1.MarkerShape = MarkerShape.filledCircle;
            scatter1.MarkerSize = 0f;
            scatter1.Label = "原始数据";

            // 绘制拟合曲线
            var scatter2 = plt.AddScatter(xs, yFit, System.Drawing.Color.Red);
            scatter2.MarkerShape = MarkerShape.none;
            scatter2.MarkerSize = 0f;
            scatter2.Label = $"拟合公式: y = {L_fit:F3}/(1 + exp(-{k_fit:F3}*(x - {x0_fit:F3})))";

            // 显示图例
            plt.Legend();
            formsPlot1.Plot.AxisAuto();
            formsPlot1.Refresh();
        }

        //梯形与次方拟合公式绘制
        private void DrawAuxiliaryCurves()
        {
            if (K <= 0) return; // 防止 K 为 0 或负数
            V_T = (int)Math.Ceiling(K * (VSet - VStart) / VMax);

            var plt = formsPlot1.Plot;

            // 清除之前的辅助曲线（保留原始数据和拟合曲线）
            foreach (var p in plt.GetPlottables())
            {
                if (p is ScatterPlot sp)
                {
                    if (sp.Label == "梯形数据" || sp.Label == "3次方数据" || sp.Label == "5次方数据" || sp.Label == "余弦数据")
                        plt.Remove(sp);
                }
            }

            // -------------------- 梯形数据 --------------------
            double[] X_trapezoid = new double[K + 1];
            double[] Y_trapezoid = new double[K + 1];

            for (int i = 0; i <= K; i++)
            {
                if (i == 0)
                    Y_trapezoid[i] = TrapezoidCurve(i, K);
                else
                    Y_trapezoid[i] = TrapezoidCurve(i, K) + Y_trapezoid[i - 1];
                X_trapezoid[i] = i / 1000.0 + X_Move;
            }

            var scatter3 = plt.AddScatter(X_trapezoid, Y_trapezoid, System.Drawing.Color.Yellow);
            scatter3.MarkerShape = MarkerShape.filledCircle;
            scatter3.MarkerSize = 5f;
            scatter3.Label = "梯形数据";

            // -------------------- 次方和余弦数据 --------------------
            double[] X_pow = new double[V_T + 1];
            double[] Y_pow3 = new double[V_T + 1];
            double[] Y_pow5 = new double[V_T + 1];
            double[] Y_cos = new double[V_T + 1];

            for (int i = 0; i <= V_T; i++)
            {
                X_pow[i] = i / 1000.0+ X_Move;
                Y_pow3[i] = CurvePow3(i, V_T, 0, VSet);
                Y_pow5[i] = CurvePow5(i, V_T , 0, VSet);
                Y_cos[i] = CurveCos(i, V_T, 0, VSet);
            }


            var scatter4 = plt.AddScatter(X_pow, Y_pow3, System.Drawing.Color.Green);
            scatter4.MarkerShape = MarkerShape.filledCircle;
            scatter4.MarkerSize = 5f;
            scatter4.Label = "3次方数据";

            var scatter5 = plt.AddScatter(X_pow, Y_pow5, System.Drawing.Color.Orange);
            scatter5.MarkerShape = MarkerShape.filledCircle;
            scatter5.MarkerSize = 5f;
            scatter5.Label = "5次方数据";

            var scatter6 = plt.AddScatter(X_pow, Y_cos, System.Drawing.Color.SaddleBrown);
            scatter6.MarkerShape = MarkerShape.filledCircle;
            scatter6.MarkerSize = 5f;
            scatter6.Label = "余弦数据";

            formsPlot1.Plot.AxisAuto();
            formsPlot1.Refresh();
        }

        //采样
        private (double[], double[]) Downsample(double[] xs, double[] ys, int maxPoints = 1000)
        {
            int N = xs.Length;
            if (N <= maxPoints) return (xs, ys);

            double[] xs2 = new double[maxPoints];
            double[] ys2 = new double[maxPoints];

            double step = (double)N / maxPoints;
            for (int i = 0; i < maxPoints; i++)
            {
                int idx = (int)(i * step);
                xs2[i] = xs[idx];
                ys2[i] = ys[idx];
            }

            return (xs2, ys2);
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
        private double CurvePow3(double x, double T, double yStart, double ySet)
        {
            double y;       //返回值

            x = x / T;      //归一化，将时间限制在0到1之间

            y = yStart + (ySet - yStart) * (3 * Math.Pow(x, 2) - 2 * Math.Pow(x, 3));

            return y < ySet ? y : ySet;
        }

        //5次方曲线公式
        private double CurvePow5(double x, double T, double yStart, double ySet)
        {
            double y;       //返回值

            x = x / T;      //归一化，将时间限制在0到1之间

            y = yStart + (ySet - yStart) * (6 * Math.Pow(x, 5) - 15 * Math.Pow(x, 4) + 10 * Math.Pow(x, 3));

            return y < ySet ? y : ySet;
        }

        //余弦曲线公式
        private double CurveCos(double x, double T, double yStart, double ySet)
        {
            double y;       //返回值

            x = x / T;      //归一化，将时间限制在0到1之间

            y = yStart + (ySet - yStart) * ((1-Math.Cos(x*Math.PI))/2);

            return y < ySet ? y : ySet;
        }


        // K 值修改事件
        private void numericK_ValueChanged(object sender, EventArgs e)
        {
            K = (int)numericK.Value;

            RedrawCurve(); // 重新绘制曲线
        }

        private void btnLoadCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = ofd.FileName; // 保存完整路径
                    labelFileName.Text = Path.GetFileName(currentFilePath);

                    SetKFromFileName();
                    LoadAndPlotCsv(currentFilePath);
                    DrawAuxiliaryCurves();
                }
            }
        }

        // 使用 K 值重新绘制当前曲线
        private void RedrawCurve()
        {
            // 如果之前没有数据，则直接返回
            if (!formsPlot1.Plot.GetPlottables().Any()) return;

            // 重新绘制，K 值生效
            DrawAuxiliaryCurves();
        }

        //从文件获取K值
        private void SetKFromFileName()
        {
            if (string.IsNullOrEmpty(currentFilePath))
                return;

            string fileName = Path.GetFileNameWithoutExtension(currentFilePath); // dataXXX
            Match match = Regex.Match(fileName, @"\d+"); // 匹配数字
            if (match.Success)
            {
                K = int.Parse(match.Value); // 赋值给 K
                numericK.Value = K;         // 同步到界面控件
            }
        }

        private void numericX_ValueChanged(object sender, EventArgs e)
        {
            X_Move = (double)numericX.Value;

            RedrawCurve();
        }

        private void numericV_ValueChanged(object sender, EventArgs e)
        {
            VSet = (double)numericV.Value;

            RedrawCurve();
        }
    }
}