using System;
using System.Threading.Tasks;

//v18

namespace mrzvis2C4
{
    internal class Program
    {
        private static int addTimes = 0;
        private static int subTimes = 0;
        private static int multiplyTimes = 0;
        private static int compareTimes = 0;

        private const int p = 5;
        private const int m = 4;
        private const int q = 6;

        private const int processorElements = 1;

        private static double[][] matrixA, matrixB, matrixE, matrixG, matrixC;

        private static void Main(string[] args)
        {
            initMatrix();

            computeMatrixC();

            Console.WriteLine("Matrix A");  printMatrix(matrixA);
            Console.WriteLine("Matrix B");  printMatrix(matrixB);
            Console.WriteLine("Matrix E");  printMatrix(matrixE);
            Console.WriteLine("Matrix G");  printMatrix(matrixG);
            Console.WriteLine("Matrix C");  printMatrix(matrixC);
        }

        private static double[][] randomMatrix(int x, int y, int seed)
        {
            double[][] matrix = new double[x][];
            for (int i = 0; i < x; i++)
            {
                matrix[i] = new double[y];
            }

            Random r = new Random(seed);

            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    matrix[i][j] = r.NextDouble() - r.NextDouble() * 2f;
                }
            }

            return matrix;
        }

        private static void nullifyMatrixC(int x, int y)
        {
            double[][] matrix = new double[x][];
            for (int i = 0; i < x; i++)
            {
                matrix[i] = new double[y];
            }

            matrixC = matrix;

            for (int i = 0; i < matrixG.Length; i++)
            {
                for (int j = 0; j < matrixG[i].Length; j++)
                {
                    matrixC[i][j] = 0;
                }
            }
        }

        private static void initMatrix()
        {
            matrixA = randomMatrix(p, m, 0);
            matrixB = randomMatrix(m, q, 1);
            matrixE = randomMatrix(1, m, 2);
            matrixG = randomMatrix(p, q, 3);
            nullifyMatrixC(p, q);
        }

        #region functions
        private static double aDeltaB(double aIK, double bKJ)
        {
            compareTimes++; subTimes++; addTimes++;
            return Math.Max(aIK + bKJ - 1, 0);
        }

        private static double deltaD(int currentI, int currentJ)
        {
            double tempResult = (1 - aDeltaB(matrixA[currentI][0], matrixB[0][currentJ]));
            subTimes++;

            for (int f = 1; f < m; f++)
            {
                multiplyTimes++; subTimes++;
                tempResult *= (1 - aDeltaB(matrixA[currentI][f], matrixB[f][currentJ]));
            }

            subTimes++;
            return 1 - tempResult;
        }

        private static double aToB(double aIK, double bKJ)
        {
            subTimes++; addTimes++; compareTimes++;
            double temp = Math.Min(1 - aIK + bKJ,0);
            return (temp > 1) ? 1 : temp;
        }

        private static double bToA(double aIK, double bKJ)
        {
            subTimes++; addTimes++; compareTimes++;
            double temp = Math.Min(1 + bKJ - aIK,0);
            return (temp > 1) ? 1 : temp;
        }

        private static double functionCalculate(int currentI, int currentJ, int currentK)
        {
            double result = 0;
            addTimes += 2;            multiplyTimes += 7;            subTimes += 3;

            result += aToB(matrixA[currentI][currentK],
                matrixB[currentK][currentJ]) * (2 * matrixE[0][currentK] - 1) * matrixE[0][currentK] +
                bToA(matrixA[currentI][currentK],
                    matrixB[currentK][currentJ]) * (1 + (4 * aToB(matrixA[currentI][currentK],
                        matrixB[currentK][currentJ]) - 2)
                        * matrixE[0][currentK]) * (1 - matrixE[0][currentK]);

            return result;
        }

        private static double deltaF(int currentI, int currentJ)
        {
            double result = 1;
            for (int k = 0; k < m; k++)
            {
                multiplyTimes++;
                result *= functionCalculate(currentI, currentJ, k);
            }
            return result;
        }

        private static double FdeltaD(int currentI, int currentJ)
        {
            double tempResult = (deltaF(currentI, currentJ) + deltaD(currentI, currentJ) - 1);
            addTimes++; subTimes++; compareTimes++;
            if (tempResult > 0) return tempResult; else return 0;
        }

        #endregion

        private static void computeMatrixC()
        {
            Parallel.For(0, matrixC.Length,
                new ParallelOptions() { MaxDegreeOfParallelism = processorElements },
                i =>
            {
                for (int j = 0; j < matrixC[i].Length; j++)
                {
                    multiplyTimes += 6; subTimes += 3; addTimes += 2;

                    matrixC[i][j] = deltaF(i, j) *
                        (3 * matrixG[i][j] - 2) *
                        matrixG[i][j] +
                        (deltaD(i, j) +
                            (4 * FdeltaD(i, j) -
                                3 * deltaD(i, j)) *
                            matrixG[i][j]
                        ) *
                        (1 - matrixG[i][j]);
                }
            });
        }

        private static void printMatrix(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write(matrix[i][j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}