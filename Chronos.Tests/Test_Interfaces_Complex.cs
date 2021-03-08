using System;
using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests a complex interface scenario.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/interface.cs
	/// </summary>
	public class Test_Interfaces_Complex {
		private interface IMeasurable {
			double Area();
		}

		private class Obj : IMeasurable {
			public double Area() {
				return 0.0;
			}
		}

		private class Rect : Obj {
            private readonly int x;
            private readonly int y;
            private readonly int w;
            private readonly int h;

            public Rect(int vx, int vy, int vw, int vh) {
				x = vx;
				y = vy;
				w = vw;
				h = vh;
			}

			public new double Area() {
				return (double)w * h;
			}
		}

		private class Circle : Obj {
            private readonly int x;
            private readonly int y;
            private readonly int r;

            public Circle(int vx, int vy, int vr) {
				x = vx;
				y = vy;
				r = vr;
			}
			public new double Area() {
				return r * r * Math.PI;
			}
		}

		public static void Run() {
			Obj rect, circle;
			double sum;

			rect = new Rect(0, 0, 10, 20);
			circle = new Circle(0, 0, 20);
			sum = rect.Area() + circle.Area();

			/* surprise! this calls Obj.Area... */
			Debug.Assert(sum == 0.0);
			
			/* now call the derived methods */
			sum = ((Rect)rect).Area() + ((Circle)circle).Area();
			Debug.Assert(sum == (200 + 400 * Math.PI));
			
			/* let's try to cast to the interface, instead */
			sum = ((IMeasurable)rect).Area() + ((IMeasurable)circle).Area();
			Debug.Assert(sum == 0.0);
		}
	}
}
