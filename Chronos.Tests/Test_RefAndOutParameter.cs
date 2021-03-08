using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Test ref and out parameters.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/outparm.cs
    /// </summary>
    public class Test_RefAndOutParameter {
        private static void OutParameter(out int n) {
            n = 1;
        }

        private static void RefParameter(ref int n) {
            n += 2;
        }

        public static void Run() {
            OutParameter(out int n);
            Debug.Assert(n == 1);

            RefParameter(ref n);
            Debug.Assert(n == 3);
        }
    }
}
