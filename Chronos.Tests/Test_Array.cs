using System;
using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests different array scenarios.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/array.cs
    /// </summary>
    public class Test_Array {
        private static void TestSimple() {
            int[] ia = new int[32];

            for (int i = 0; i < ia.Length; i++) {
                ia[i] = i * i;
            }

            for (int i = 0; i < ia.Length; i++) {
                Debug.Assert(ia[i] == i * i);
            }
        }

        private static void TestJagged() {
            int[][] j2 = new int[3][];

            j2[0] = new int[3];
            j2[1] = new int[6];
            j2[2] = new int[9];

            for (int i = 0; i < j2.Length; i++) {
                for (int j = 0; j < (i + 1) * 3; j++) {
                    j2[i][j] = j;
                }
            }

            for (int i = 0; i < j2.Length; i++) {
                for (int j = 0; j < (i + 1) * 3; j++) {
                    Debug.Assert(j2[i][j] == j);
                }
            }
        }

		public static void Run() {
            TestSimple();
            TestJagged();
        }
    }
}
