namespace System {
    public struct Double {
        public const double MaxValue = 1.7976931348623157e+308;
        public const double MinValue = -1.7976931348623157e+308;
        public const double Epsilon = 4.9406564584124654e-324;
        public const double NegativeInfinity = (double)-1.0 / (double)0.0;
        public const double PositiveInfinity = (double)1.0 / (double)0.0;
        public const double NaN = (double)0.0 / (double)0.0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly double m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169
    }
}
