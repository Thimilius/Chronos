using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests arrays.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/array.cs
    /// </summary>
    public class Test_ArrayBounds {
		public static void Run() {
            try {
                byte[] b = new byte[0];
                b[0] = 128;
                Debug.Assert(false);
            } catch {
                Debug.Assert(true);
            }
            Debug.Assert(true);
        }
    }
}
