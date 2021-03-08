using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests basic binary operations.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/test-ops.cs
    /// </summary>
    public class Test_Operations {
		private static sbyte SByteAdd(sbyte a, sbyte b) {
			return (sbyte)(a + b);
		}

		private static short ShortAdd(short a, short b) {
			return (short)(a + b);
		}

		private static double DoubleAdd(double a, double b) {
			return a + b;
		}

		private static int IntAdd(int a, int b) {
			return a + b;
		}

		private static int IntSub(int a, int b) {
			return a - b;
		}

		private static int IntMul(int a, int b) {
			return a * b;
		}

		private static int IntDiv(int a, int b) {
			return a / b;
		}

		public static void Run() {
			Debug.Assert(IntDiv(5, 2) == 2);
			Debug.Assert(IntAdd(1, 1) == 2);
			Debug.Assert(IntAdd(31, -1) == 30);
			Debug.Assert(IntSub(31, -1) == 32);
			Debug.Assert(IntMul(12, 12) == 144);
			Debug.Assert(SByteAdd(1, 1) == 2);
			Debug.Assert(SByteAdd(31, -1) == 30);
			Debug.Assert(ShortAdd(1, 1) == 2);
			Debug.Assert(ShortAdd(31, -1) == 30);
			Debug.Assert(DoubleAdd(1.5, 1.5) == 3);
		}
    }
}
