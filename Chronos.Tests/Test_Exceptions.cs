using System;
using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests different kinds of exceptions.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/exception.cs
    /// </summary>
    public class Test_Exceptions {
		private class Ex {
			public int p;
        }

		private static int TestNullReference() {
			Ex x = null;

			try {
				x.p = 1;
			} catch (NullReferenceException) {
				return 0;
			}
			return 1;
		}

		private static int TestDivideByZero(int a) {
			int res;
			int fin = 0;
			try {
				res = 10 / a;
			} catch (DivideByZeroException) {
				if (fin != 1)
					res = 34;
				else
					res = 33;
			} catch {
				if (fin != 1)
					res = 24;
				else
					res = 22;
			} finally {
				fin = 1;
			}
			return res;
		}

		public static void Run() {
			Debug.Assert(TestNullReference() == 0);
			Debug.Assert(TestDivideByZero(1) == 10);
			Debug.Assert(TestDivideByZero(0) == 34);
		}
    }
}
