using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests the static initilization of classes.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/classinit.cs
    /// </summary>
    public class Test_ClassInit {
        private class Foo {
            public static int I = 0;
        }

        private class Bar {
            public static int J;

            static Bar() {
                J = Foo.I;
            }
        }

        public static void Run() {
            Foo.I = 5;

            Debug.Assert(Bar.J == 5);
        }
    }
}
