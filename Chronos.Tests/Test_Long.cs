﻿using System.Diagnostics;

namespace Chronos.Tests {
	/// <summary>
	/// Tests the interaction with longs. 
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/long.cs
	/// </summary>
	public class Test_Long {
		private static readonly ulong[] s_Values = {
			1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048,
			4096, 8192, 16384, 32768, 65536, 131072,
			262144, 524288, 1048576, 2097152, 4194304, 8388608,
			16777216, 33554432, 67108864, 134217728, 268435456,
			536870912, 1073741824, 2147483648, 4294967296,
			8589934592, 17179869184, 34359738368, 68719476736,
			137438953472, 274877906944, 549755813888, 1099511627776,
			2199023255552, 4398046511104, 8796093022208,
			17592186044416, 35184372088832, 70368744177664,
			140737488355328, 281474976710656, 562949953421312,
			1125899906842624, 2251799813685248, 4503599627370496,
			9007199254740992, 18014398509481984, 36028797018963968,
			72057594037927936, 144115188075855872, 288230376151711744,
			576460752303423488, 1152921504606846976,
			2305843009213693952, 4611686018427387904
		};

		public static void Run() {
			int i;
			ulong val = 1;
			ulong val17 = 131072;
			ulong val33 = 8589934592;
			ulong val39 = 549755813888;
			ulong val47 = 140737488355328;
			ulong val55 = 36028797018963968;

			for (i = 0; i < s_Values.Length; ++i) {
				Debug.Assert(val == s_Values[i]);

				val *= 2;
			}

			Debug.Assert(val17 == s_Values[17]);
			Debug.Assert(val33 == s_Values[33]);
			Debug.Assert(val39 == s_Values[39]);
			Debug.Assert(val47 == s_Values[47]);
			Debug.Assert(val55 == s_Values[55]);
		}
    }
}
