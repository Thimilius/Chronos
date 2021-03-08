using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Test delegates.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/delegate17.cs
	/// </summary>
	public class Test_Delegate {
		private delegate void SimpleDelegate();

		private static void Doo() { }

		public enum WasCalled {
			BaseWasCalled,
			DerivedWasCalled
		}

		public delegate WasCalled Del1(string s);
		public delegate WasCalled Del2(string s);

		public class Base {
			public virtual WasCalled Foo(string s) {
				return WasCalled.BaseWasCalled;
			}
		}

		public class Derived : Base {
			public override WasCalled Foo(string s) {
				return WasCalled.DerivedWasCalled;
			}
		}

		public static void Run() {
			Debug.Assert(new SimpleDelegate(Doo) == new SimpleDelegate(Doo));

			Derived d = new Derived();
			Del1 b = new Del1(d.Foo);
			Del2 f = new Del2(b.Invoke);
			var r = f("Derived.Foo");
			Debug.Assert(r == WasCalled.DerivedWasCalled);
		}
    }
}
