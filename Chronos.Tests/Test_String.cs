using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests strings.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/string.cs
	/// </summary>
	public class Test_String {
        public static void Run() {
			string a = "ddd";
			string b = "ddd";
			string c = "ddda";

			Debug.Assert(a == b);
			Debug.Assert(ReferenceEquals(a, b));
			Debug.Assert(!ReferenceEquals(c, string.Concat(b, "a")));
			Debug.Assert(c == string.Concat(b, "a"));
			Debug.Assert(ReferenceEquals(string.Empty, ""));
		}
    }
}
