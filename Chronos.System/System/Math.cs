using System.Runtime.CompilerServices;

namespace System {
    public static class Math {
        public const double PI = 3.14159265358979;
        public const double E = 2.71828182845905;

        public static int Min(int a, int b) {
            return a < b ? a : b;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Sin(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Cos(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Tan(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Sinh(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Cosh(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Tanh(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Asin(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Acos(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Atan(double value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double Atan2(double y, double x);
    }
}
