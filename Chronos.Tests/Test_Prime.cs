using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests prime number calculation.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/test-prime.cs
	/// </summary>
	public class Test_Prime {
		private static bool TestPrime(int x) {
			if ((x & 1) != 0) {
				for (int n = 3; n < x; n += 2) {
					if ((x % n) == 0)
						return false;
				}
				return true;
			}
			return (x == 2);
		}

		public static void Run() {
			Debug.Assert(TestPrime(17));
		}
	}
}
