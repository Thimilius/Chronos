using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests a simple interface scenario.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/interface1.cs
	/// </summary>
	public class Test_Interfaces_Simple {
		private interface IB {
			int Method();
		}

		private class A {
			public virtual int Method() {
				return 1;
			}
		}

		private class C : A, IB {
			
		}

		public static void Run() {
			C c = new C();

			Debug.Assert(c.Method() == 1);
		}
	}
}
