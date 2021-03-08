using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests object creation and interaction.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/obj.cs
    /// </summary>
    public class Test_Object {
		private class TestObj {
			public static int s_Foo = 5;

			public int Foo = 1;
			public int Bar;

			public TestObj() {
				Bar = 2;
			}

			public int Method() {
				return Bar;
			}

			public object Clone() {
				return MemberwiseClone();
            }
		}

		public static void Run() {
			TestObj obj = new TestObj();
			Debug.Assert(TestObj.s_Foo + obj.Foo + obj.Method() == 8);

			TestObj clone = (TestObj)obj.Clone();
			Debug.Assert(clone.Bar == 2);
		}
	}
}
