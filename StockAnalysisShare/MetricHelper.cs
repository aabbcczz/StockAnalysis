using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsDefinition
{
    public static class MetricHelper
    {
        public static string ConvertMetricToCsvCompatibleHead(string metric)
        {
            return metric.Replace(',', '|');
        }

        public static string ConvertCsvHeadToMetric(string head)
        {
            return head.Replace('|', ',');
        }

        public static double[] OperateNew(double[] op1, double[] op2, Func<double, double, double> f)
        {
            if (op1.Length != op2.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            double[] result = new double[op1.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = f(op1[i], op2[i]);
            }

            return result;
        }

        public static double[] OperateNew(double[] op1, double[] op2, double[] op3, Func<double, double, double, double> f)
        {
            if (op1.Length != op2.Length || op1.Length != op3.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            double[] result = new double[op1.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = f(op1[i], op2[i], op3[i]);
            }

            return result;
        }

        public static double[] OperateNew(double[] op1, double[] op2, double[] op3, double[] op4, Func<double, double, double, double, double> f)
        {
            if (op1.Length != op2.Length || op1.Length != op3.Length || op1.Length != op4.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            double[] result = new double[op1.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = f(op1[i], op2[i], op3[i], op4[i]);
            }

            return result;
        }

        public static double[] OperateNew(double[] op1, double[] op2, double[] op3, double[] op4, double[] op5, Func<double, double, double, double, double, double> f)
        {
            if (op1.Length != op2.Length || op1.Length != op3.Length || op1.Length != op4.Length || op1.Length != op5.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            double[] result = new double[op1.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = f(op1[i], op2[i], op3[i], op4[i], op5[i]);
            }

            return result;
        }

        public static double[] OperateNew(double[] op1, double op2, Func<double, double, double> f)
        {
            double[] result = new double[op1.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = f(op1[i], op2);
            }

            return result;
        }

        public static double[] OperateThis(this double[] op1, double[] op2, Func<double, double, double> f)
        {
            if (op1.Length != op2.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            for (int i = 0; i < op1.Length; ++i)
            {
                op1[i] = f(op1[i], op2[i]);
            }

            return op1;
        }

        public static double[] OperateThis(this double[] op1, double[] op2, double[] op3, Func<double, double, double, double> f)
        {
            if (op1.Length != op2.Length || op1.Length != op3.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            for (int i = 0; i < op1.Length; ++i)
            {
                op1[i] = f(op1[i], op2[i], op3[i]);
            }

            return op1;
        }

        public static double[] OperateThis(this double[] op1, double[] op2, double[] op3, double[] op4, Func<double, double, double, double, double> f)
        {
            if (op1.Length != op2.Length || op1.Length != op3.Length || op1.Length != op4.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            for (int i = 0; i < op1.Length; ++i)
            {
                op1[i] = f(op1[i], op2[i], op3[i], op4[i]);
            }

            return op1;
        }

        public static double[] OperateThis(this double[] op1, double[] op2, double[] op3, double[] op4, double[] op5, Func<double, double, double, double, double, double> f)
        {
            if (op1.Length != op2.Length || op1.Length != op3.Length || op1.Length != op4.Length || op1.Length != op5.Length)
            {
                throw new ArgumentException("input arrays have different length");
            }

            for (int i = 0; i < op1.Length; ++i)
            {
                op1[i] = f(op1[i], op2[i], op3[i], op4[i], op5[i]);
            }

            return op1;
        }

        public static double[] OperateThis(this double[] op1, double op2, Func<double, double, double> f)
        {
            for (int i = 0; i < op1.Length; ++i)
            {
                op1[i] = f(op1[i], op2);
            }

            return op1;
        }
    }
}
