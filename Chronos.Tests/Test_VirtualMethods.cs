using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests virtual methods.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/virtual-method.cs
    /// </summary>
    public class Test_VirtualMethods {
		private interface IFoo {
			int H();
		}

		private class A : IFoo {
			public int F() { return 1; }
			public virtual int G() { return 2; }
			public int H() { return 10; }
		}

		private class B : A {
			public new int F() { return 3; }
			public override int G() { return 4; }
			public new int H() { return 11; }
		}

		public static void Run() {
			B b = new B();
			A a = b;

			Debug.Assert(a.F() == 1);
			Debug.Assert(b.F() == 3);
			Debug.Assert(b.G() == 4);
			Debug.Assert(a.G() == 4);
			Debug.Assert(a.H() == 10);
			Debug.Assert(b.H() == 11);
			Debug.Assert(((A)b).H() == 10);
			Debug.Assert(((B)a).H() == 11);
		}
    }
}
