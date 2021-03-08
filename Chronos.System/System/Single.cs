namespace System {
    public struct Single {
        public const float MaxValue = (float)3.402823E+38;
        public const float MinValue = (float)-3.402823E+38;
        public const float Epsilon = (float)1.401298E-45;
        public const float NegativeInfinity = (float)-1.0 / (float)0.0;
        public const float PositiveInfinity = (float)1.0 / (float)0.0;
        public const float NaN = (float)0.0 / (float)0.0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly float m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169
    }
}
