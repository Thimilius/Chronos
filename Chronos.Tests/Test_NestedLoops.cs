using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests nested loops.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/nested-loops.cs
    /// </summary>
    public class Test_NestedLoops {
		public static int Run() {
			int n = 2;
			int x = 0;
			int a = n;
			while (a-- != 0) {
				int b = n;
				while (b-- != 0) {
					int c = n;
					while (c-- != 0) {
						int d = n;
						while (d-- != 0) {
							int e = n;
							while (e-- != 0) {
								int f = n;
								while (f-- != 0) {
									x++;
								}
							}
						}
					}
				}
			}
			
			Debug.Assert(x == 64);

			return 0;
		}
	}
}
