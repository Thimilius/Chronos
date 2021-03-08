using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests shifting operation.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/shift.cs
	/// </summary>
	public class Test_Shift {
		public static void Run() {
			int[] n = new int[1];
			int b = 16;

			n[0] = 100 + (1 << (16 - b));

			Debug.Assert(n[0] == 101);
		}
	}
}
