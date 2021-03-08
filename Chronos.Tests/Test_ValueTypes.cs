using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests the creation of value types.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/newobj-valuetype.cs
	/// </summary>
	public class Test_ValueTypes {
		private struct Struct {
			public int a;

			public Struct(int val) {
				a = val;
			}
		}

		public static void Run() {
			object o = new Struct(1);
			Struct s = new Struct(2);

			Debug.Assert(s.a == 2);
			Debug.Assert(((Struct)o).a == 1);
		}
    }
}
