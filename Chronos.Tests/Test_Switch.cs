using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests a switch expression.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/switch.cs
    /// </summary>
    public class Test_Switch {
		private static int Test(int n) {
            return n switch
            {
                0 => 1,
                1 => 0,
                -1 => 2,
                _ => 0xff,
            };
		}

		private const string s_LongString = "{http://schemas.xmlsoap.org/ws/2003/03/business-process/}partnerLinks";

		private static int TestString(string s) {
            return s switch
            {
                "{http://schemas.xmlsoap.org/ws/2003/03/business-process/}partnerLinks" => 1,
                _ => 0,
            };
        }

		public static void Run() {
            Debug.Assert(Test(0) == 1);
            Debug.Assert(Test(1) == 0);
            Debug.Assert(Test(-1) == 2);
            Debug.Assert(Test(3) == 0xFF);

            Debug.Assert(TestString(s_LongString) == 1);
        }
    }
}
