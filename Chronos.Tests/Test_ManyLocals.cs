﻿namespace Chronos.Tests {
	/// <summary>
	/// Tests many local variables inside a function.
	/// 
	/// Test from: https://github.com/mono/mono/blob/master/mono/tests/many-locals.cs
	/// </summary>
	public class Test_ManyLocals {
		private struct T {
			public uint A { get; }
			public uint B { get; }

			public T(uint A, uint B) {
                this.A = A;
                this.B = B;
			}
        }

		public static void Run() {
			T loc0 = new T(0xdeadbeef, 0xcafebabe);
			T loc1 = new T(0xdeadbeef, 0xcafebabe);
			T loc2 = new T(0xdeadbeef, 0xcafebabe);
			T loc3 = new T(0xdeadbeef, 0xcafebabe);
			T loc4 = new T(0xdeadbeef, 0xcafebabe);
			T loc5 = new T(0xdeadbeef, 0xcafebabe);
			T loc6 = new T(0xdeadbeef, 0xcafebabe);
			T loc7 = new T(0xdeadbeef, 0xcafebabe);
			T loc8 = new T(0xdeadbeef, 0xcafebabe);
			T loc9 = new T(0xdeadbeef, 0xcafebabe);
			T loc10 = new T(0xdeadbeef, 0xcafebabe);
			T loc11 = new T(0xdeadbeef, 0xcafebabe);
			T loc12 = new T(0xdeadbeef, 0xcafebabe);
			T loc13 = new T(0xdeadbeef, 0xcafebabe);
			T loc14 = new T(0xdeadbeef, 0xcafebabe);
			T loc15 = new T(0xdeadbeef, 0xcafebabe);
			T loc16 = new T(0xdeadbeef, 0xcafebabe);
			T loc17 = new T(0xdeadbeef, 0xcafebabe);
			T loc18 = new T(0xdeadbeef, 0xcafebabe);
			T loc19 = new T(0xdeadbeef, 0xcafebabe);
			T loc20 = new T(0xdeadbeef, 0xcafebabe);
			T loc21 = new T(0xdeadbeef, 0xcafebabe);
			T loc22 = new T(0xdeadbeef, 0xcafebabe);
			T loc23 = new T(0xdeadbeef, 0xcafebabe);
			T loc24 = new T(0xdeadbeef, 0xcafebabe);
			T loc25 = new T(0xdeadbeef, 0xcafebabe);
			T loc26 = new T(0xdeadbeef, 0xcafebabe);
			T loc27 = new T(0xdeadbeef, 0xcafebabe);
			T loc28 = new T(0xdeadbeef, 0xcafebabe);
			T loc29 = new T(0xdeadbeef, 0xcafebabe);
			T loc30 = new T(0xdeadbeef, 0xcafebabe);
			T loc31 = new T(0xdeadbeef, 0xcafebabe);
			T loc32 = new T(0xdeadbeef, 0xcafebabe);
			T loc33 = new T(0xdeadbeef, 0xcafebabe);
			T loc34 = new T(0xdeadbeef, 0xcafebabe);
			T loc35 = new T(0xdeadbeef, 0xcafebabe);
			T loc36 = new T(0xdeadbeef, 0xcafebabe);
			T loc37 = new T(0xdeadbeef, 0xcafebabe);
			T loc38 = new T(0xdeadbeef, 0xcafebabe);
			T loc39 = new T(0xdeadbeef, 0xcafebabe);
			T loc40 = new T(0xdeadbeef, 0xcafebabe);
			T loc41 = new T(0xdeadbeef, 0xcafebabe);
			T loc42 = new T(0xdeadbeef, 0xcafebabe);
			T loc43 = new T(0xdeadbeef, 0xcafebabe);
			T loc44 = new T(0xdeadbeef, 0xcafebabe);
			T loc45 = new T(0xdeadbeef, 0xcafebabe);
			T loc46 = new T(0xdeadbeef, 0xcafebabe);
			T loc47 = new T(0xdeadbeef, 0xcafebabe);
			T loc48 = new T(0xdeadbeef, 0xcafebabe);
			T loc49 = new T(0xdeadbeef, 0xcafebabe);
			T loc50 = new T(0xdeadbeef, 0xcafebabe);
			T loc51 = new T(0xdeadbeef, 0xcafebabe);
			T loc52 = new T(0xdeadbeef, 0xcafebabe);
			T loc53 = new T(0xdeadbeef, 0xcafebabe);
			T loc54 = new T(0xdeadbeef, 0xcafebabe);
			T loc55 = new T(0xdeadbeef, 0xcafebabe);
			T loc56 = new T(0xdeadbeef, 0xcafebabe);
			T loc57 = new T(0xdeadbeef, 0xcafebabe);
			T loc58 = new T(0xdeadbeef, 0xcafebabe);
			T loc59 = new T(0xdeadbeef, 0xcafebabe);
			T loc60 = new T(0xdeadbeef, 0xcafebabe);
			T loc61 = new T(0xdeadbeef, 0xcafebabe);
			T loc62 = new T(0xdeadbeef, 0xcafebabe);
			T loc63 = new T(0xdeadbeef, 0xcafebabe);
			T loc64 = new T(0xdeadbeef, 0xcafebabe);
			T loc65 = new T(0xdeadbeef, 0xcafebabe);
			T loc66 = new T(0xdeadbeef, 0xcafebabe);
			T loc67 = new T(0xdeadbeef, 0xcafebabe);
			T loc68 = new T(0xdeadbeef, 0xcafebabe);
			T loc69 = new T(0xdeadbeef, 0xcafebabe);
			T loc70 = new T(0xdeadbeef, 0xcafebabe);
			T loc71 = new T(0xdeadbeef, 0xcafebabe);
			T loc72 = new T(0xdeadbeef, 0xcafebabe);
			T loc73 = new T(0xdeadbeef, 0xcafebabe);
			T loc74 = new T(0xdeadbeef, 0xcafebabe);
			T loc75 = new T(0xdeadbeef, 0xcafebabe);
			T loc76 = new T(0xdeadbeef, 0xcafebabe);
			T loc77 = new T(0xdeadbeef, 0xcafebabe);
			T loc78 = new T(0xdeadbeef, 0xcafebabe);
			T loc79 = new T(0xdeadbeef, 0xcafebabe);
			T loc80 = new T(0xdeadbeef, 0xcafebabe);
			T loc81 = new T(0xdeadbeef, 0xcafebabe);
			T loc82 = new T(0xdeadbeef, 0xcafebabe);
			T loc83 = new T(0xdeadbeef, 0xcafebabe);
			T loc84 = new T(0xdeadbeef, 0xcafebabe);
			T loc85 = new T(0xdeadbeef, 0xcafebabe);
			T loc86 = new T(0xdeadbeef, 0xcafebabe);
			T loc87 = new T(0xdeadbeef, 0xcafebabe);
			T loc88 = new T(0xdeadbeef, 0xcafebabe);
			T loc89 = new T(0xdeadbeef, 0xcafebabe);
			T loc90 = new T(0xdeadbeef, 0xcafebabe);
			T loc91 = new T(0xdeadbeef, 0xcafebabe);
			T loc92 = new T(0xdeadbeef, 0xcafebabe);
			T loc93 = new T(0xdeadbeef, 0xcafebabe);
			T loc94 = new T(0xdeadbeef, 0xcafebabe);
			T loc95 = new T(0xdeadbeef, 0xcafebabe);
			T loc96 = new T(0xdeadbeef, 0xcafebabe);
			T loc97 = new T(0xdeadbeef, 0xcafebabe);
			T loc98 = new T(0xdeadbeef, 0xcafebabe);
			T loc99 = new T(0xdeadbeef, 0xcafebabe);
			T loc100 = new T(0xdeadbeef, 0xcafebabe);
			T loc101 = new T(0xdeadbeef, 0xcafebabe);
			T loc102 = new T(0xdeadbeef, 0xcafebabe);
			T loc103 = new T(0xdeadbeef, 0xcafebabe);
			T loc104 = new T(0xdeadbeef, 0xcafebabe);
			T loc105 = new T(0xdeadbeef, 0xcafebabe);
			T loc106 = new T(0xdeadbeef, 0xcafebabe);
			T loc107 = new T(0xdeadbeef, 0xcafebabe);
			T loc108 = new T(0xdeadbeef, 0xcafebabe);
			T loc109 = new T(0xdeadbeef, 0xcafebabe);
			T loc110 = new T(0xdeadbeef, 0xcafebabe);
			T loc111 = new T(0xdeadbeef, 0xcafebabe);
			T loc112 = new T(0xdeadbeef, 0xcafebabe);
			T loc113 = new T(0xdeadbeef, 0xcafebabe);
			T loc114 = new T(0xdeadbeef, 0xcafebabe);
			T loc115 = new T(0xdeadbeef, 0xcafebabe);
			T loc116 = new T(0xdeadbeef, 0xcafebabe);
			T loc117 = new T(0xdeadbeef, 0xcafebabe);
			T loc118 = new T(0xdeadbeef, 0xcafebabe);
			T loc119 = new T(0xdeadbeef, 0xcafebabe);
			T loc120 = new T(0xdeadbeef, 0xcafebabe);
			T loc121 = new T(0xdeadbeef, 0xcafebabe);
			T loc122 = new T(0xdeadbeef, 0xcafebabe);
			T loc123 = new T(0xdeadbeef, 0xcafebabe);
			T loc124 = new T(0xdeadbeef, 0xcafebabe);
			T loc125 = new T(0xdeadbeef, 0xcafebabe);
			T loc126 = new T(0xdeadbeef, 0xcafebabe);
			T loc127 = new T(0xdeadbeef, 0xcafebabe);
			T loc128 = new T(0xdeadbeef, 0xcafebabe);
			T loc129 = new T(0xdeadbeef, 0xcafebabe);
			T loc130 = new T(0xdeadbeef, 0xcafebabe);
			T loc131 = new T(0xdeadbeef, 0xcafebabe);
			T loc132 = new T(0xdeadbeef, 0xcafebabe);
			T loc133 = new T(0xdeadbeef, 0xcafebabe);
			T loc134 = new T(0xdeadbeef, 0xcafebabe);
			T loc135 = new T(0xdeadbeef, 0xcafebabe);
			T loc136 = new T(0xdeadbeef, 0xcafebabe);
			T loc137 = new T(0xdeadbeef, 0xcafebabe);
			T loc138 = new T(0xdeadbeef, 0xcafebabe);
			T loc139 = new T(0xdeadbeef, 0xcafebabe);
			T loc140 = new T(0xdeadbeef, 0xcafebabe);
			T loc141 = new T(0xdeadbeef, 0xcafebabe);
			T loc142 = new T(0xdeadbeef, 0xcafebabe);
			T loc143 = new T(0xdeadbeef, 0xcafebabe);
			T loc144 = new T(0xdeadbeef, 0xcafebabe);
			T loc145 = new T(0xdeadbeef, 0xcafebabe);
			T loc146 = new T(0xdeadbeef, 0xcafebabe);
			T loc147 = new T(0xdeadbeef, 0xcafebabe);
			T loc148 = new T(0xdeadbeef, 0xcafebabe);
			T loc149 = new T(0xdeadbeef, 0xcafebabe);
			T loc150 = new T(0xdeadbeef, 0xcafebabe);
			T loc151 = new T(0xdeadbeef, 0xcafebabe);
			T loc152 = new T(0xdeadbeef, 0xcafebabe);
			T loc153 = new T(0xdeadbeef, 0xcafebabe);
			T loc154 = new T(0xdeadbeef, 0xcafebabe);
			T loc155 = new T(0xdeadbeef, 0xcafebabe);
			T loc156 = new T(0xdeadbeef, 0xcafebabe);
			T loc157 = new T(0xdeadbeef, 0xcafebabe);
			T loc158 = new T(0xdeadbeef, 0xcafebabe);
			T loc159 = new T(0xdeadbeef, 0xcafebabe);
			T loc160 = new T(0xdeadbeef, 0xcafebabe);
			T loc161 = new T(0xdeadbeef, 0xcafebabe);
			T loc162 = new T(0xdeadbeef, 0xcafebabe);
			T loc163 = new T(0xdeadbeef, 0xcafebabe);
			T loc164 = new T(0xdeadbeef, 0xcafebabe);
			T loc165 = new T(0xdeadbeef, 0xcafebabe);
			T loc166 = new T(0xdeadbeef, 0xcafebabe);
			T loc167 = new T(0xdeadbeef, 0xcafebabe);
			T loc168 = new T(0xdeadbeef, 0xcafebabe);
			T loc169 = new T(0xdeadbeef, 0xcafebabe);
			T loc170 = new T(0xdeadbeef, 0xcafebabe);
			T loc171 = new T(0xdeadbeef, 0xcafebabe);
			T loc172 = new T(0xdeadbeef, 0xcafebabe);
			T loc173 = new T(0xdeadbeef, 0xcafebabe);
			T loc174 = new T(0xdeadbeef, 0xcafebabe);
			T loc175 = new T(0xdeadbeef, 0xcafebabe);
			T loc176 = new T(0xdeadbeef, 0xcafebabe);
			T loc177 = new T(0xdeadbeef, 0xcafebabe);
			T loc178 = new T(0xdeadbeef, 0xcafebabe);
			T loc179 = new T(0xdeadbeef, 0xcafebabe);
			T loc180 = new T(0xdeadbeef, 0xcafebabe);
			T loc181 = new T(0xdeadbeef, 0xcafebabe);
			T loc182 = new T(0xdeadbeef, 0xcafebabe);
			T loc183 = new T(0xdeadbeef, 0xcafebabe);
			T loc184 = new T(0xdeadbeef, 0xcafebabe);
			T loc185 = new T(0xdeadbeef, 0xcafebabe);
			T loc186 = new T(0xdeadbeef, 0xcafebabe);
			T loc187 = new T(0xdeadbeef, 0xcafebabe);
			T loc188 = new T(0xdeadbeef, 0xcafebabe);
			T loc189 = new T(0xdeadbeef, 0xcafebabe);
			T loc190 = new T(0xdeadbeef, 0xcafebabe);
			T loc191 = new T(0xdeadbeef, 0xcafebabe);
			T loc192 = new T(0xdeadbeef, 0xcafebabe);
			T loc193 = new T(0xdeadbeef, 0xcafebabe);
			T loc194 = new T(0xdeadbeef, 0xcafebabe);
			T loc195 = new T(0xdeadbeef, 0xcafebabe);
			T loc196 = new T(0xdeadbeef, 0xcafebabe);
			T loc197 = new T(0xdeadbeef, 0xcafebabe);
			T loc198 = new T(0xdeadbeef, 0xcafebabe);
			T loc199 = new T(0xdeadbeef, 0xcafebabe);
			int loc_i = 11;
			long loc_j = 12;
			
			if (loc0.A != 0xdeadbeef || loc0.B != 0xcafebabe) throw new System.Exception();
			if (loc1.A != 0xdeadbeef || loc1.B != 0xcafebabe) throw new System.Exception();
			if (loc2.A != 0xdeadbeef || loc2.B != 0xcafebabe) throw new System.Exception();
			if (loc3.A != 0xdeadbeef || loc3.B != 0xcafebabe) throw new System.Exception();
			if (loc4.A != 0xdeadbeef || loc4.B != 0xcafebabe) throw new System.Exception();
			if (loc5.A != 0xdeadbeef || loc5.B != 0xcafebabe) throw new System.Exception();
			if (loc6.A != 0xdeadbeef || loc6.B != 0xcafebabe) throw new System.Exception();
			if (loc7.A != 0xdeadbeef || loc7.B != 0xcafebabe) throw new System.Exception();
			if (loc8.A != 0xdeadbeef || loc8.B != 0xcafebabe) throw new System.Exception();
			if (loc9.A != 0xdeadbeef || loc9.B != 0xcafebabe) throw new System.Exception();
			if (loc10.A != 0xdeadbeef || loc10.B != 0xcafebabe) throw new System.Exception();
			if (loc11.A != 0xdeadbeef || loc11.B != 0xcafebabe) throw new System.Exception();
			if (loc12.A != 0xdeadbeef || loc12.B != 0xcafebabe) throw new System.Exception();
			if (loc13.A != 0xdeadbeef || loc13.B != 0xcafebabe) throw new System.Exception();
			if (loc14.A != 0xdeadbeef || loc14.B != 0xcafebabe) throw new System.Exception();
			if (loc15.A != 0xdeadbeef || loc15.B != 0xcafebabe) throw new System.Exception();
			if (loc16.A != 0xdeadbeef || loc16.B != 0xcafebabe) throw new System.Exception();
			if (loc17.A != 0xdeadbeef || loc17.B != 0xcafebabe) throw new System.Exception();
			if (loc18.A != 0xdeadbeef || loc18.B != 0xcafebabe) throw new System.Exception();
			if (loc19.A != 0xdeadbeef || loc19.B != 0xcafebabe) throw new System.Exception();
			if (loc20.A != 0xdeadbeef || loc20.B != 0xcafebabe) throw new System.Exception();
			if (loc21.A != 0xdeadbeef || loc21.B != 0xcafebabe) throw new System.Exception();
			if (loc22.A != 0xdeadbeef || loc22.B != 0xcafebabe) throw new System.Exception();
			if (loc23.A != 0xdeadbeef || loc23.B != 0xcafebabe) throw new System.Exception();
			if (loc24.A != 0xdeadbeef || loc24.B != 0xcafebabe) throw new System.Exception();
			if (loc25.A != 0xdeadbeef || loc25.B != 0xcafebabe) throw new System.Exception();
			if (loc26.A != 0xdeadbeef || loc26.B != 0xcafebabe) throw new System.Exception();
			if (loc27.A != 0xdeadbeef || loc27.B != 0xcafebabe) throw new System.Exception();
			if (loc28.A != 0xdeadbeef || loc28.B != 0xcafebabe) throw new System.Exception();
			if (loc29.A != 0xdeadbeef || loc29.B != 0xcafebabe) throw new System.Exception();
			if (loc30.A != 0xdeadbeef || loc30.B != 0xcafebabe) throw new System.Exception();
			if (loc31.A != 0xdeadbeef || loc31.B != 0xcafebabe) throw new System.Exception();
			if (loc32.A != 0xdeadbeef || loc32.B != 0xcafebabe) throw new System.Exception();
			if (loc33.A != 0xdeadbeef || loc33.B != 0xcafebabe) throw new System.Exception();
			if (loc34.A != 0xdeadbeef || loc34.B != 0xcafebabe) throw new System.Exception();
			if (loc35.A != 0xdeadbeef || loc35.B != 0xcafebabe) throw new System.Exception();
			if (loc36.A != 0xdeadbeef || loc36.B != 0xcafebabe) throw new System.Exception();
			if (loc37.A != 0xdeadbeef || loc37.B != 0xcafebabe) throw new System.Exception();
			if (loc38.A != 0xdeadbeef || loc38.B != 0xcafebabe) throw new System.Exception();
			if (loc39.A != 0xdeadbeef || loc39.B != 0xcafebabe) throw new System.Exception();
			if (loc40.A != 0xdeadbeef || loc40.B != 0xcafebabe) throw new System.Exception();
			if (loc41.A != 0xdeadbeef || loc41.B != 0xcafebabe) throw new System.Exception();
			if (loc42.A != 0xdeadbeef || loc42.B != 0xcafebabe) throw new System.Exception();
			if (loc43.A != 0xdeadbeef || loc43.B != 0xcafebabe) throw new System.Exception();
			if (loc44.A != 0xdeadbeef || loc44.B != 0xcafebabe) throw new System.Exception();
			if (loc45.A != 0xdeadbeef || loc45.B != 0xcafebabe) throw new System.Exception();
			if (loc46.A != 0xdeadbeef || loc46.B != 0xcafebabe) throw new System.Exception();
			if (loc47.A != 0xdeadbeef || loc47.B != 0xcafebabe) throw new System.Exception();
			if (loc48.A != 0xdeadbeef || loc48.B != 0xcafebabe) throw new System.Exception();
			if (loc49.A != 0xdeadbeef || loc49.B != 0xcafebabe) throw new System.Exception();
			if (loc50.A != 0xdeadbeef || loc50.B != 0xcafebabe) throw new System.Exception();
			if (loc51.A != 0xdeadbeef || loc51.B != 0xcafebabe) throw new System.Exception();
			if (loc52.A != 0xdeadbeef || loc52.B != 0xcafebabe) throw new System.Exception();
			if (loc53.A != 0xdeadbeef || loc53.B != 0xcafebabe) throw new System.Exception();
			if (loc54.A != 0xdeadbeef || loc54.B != 0xcafebabe) throw new System.Exception();
			if (loc55.A != 0xdeadbeef || loc55.B != 0xcafebabe) throw new System.Exception();
			if (loc56.A != 0xdeadbeef || loc56.B != 0xcafebabe) throw new System.Exception();
			if (loc57.A != 0xdeadbeef || loc57.B != 0xcafebabe) throw new System.Exception();
			if (loc58.A != 0xdeadbeef || loc58.B != 0xcafebabe) throw new System.Exception();
			if (loc59.A != 0xdeadbeef || loc59.B != 0xcafebabe) throw new System.Exception();
			if (loc60.A != 0xdeadbeef || loc60.B != 0xcafebabe) throw new System.Exception();
			if (loc61.A != 0xdeadbeef || loc61.B != 0xcafebabe) throw new System.Exception();
			if (loc62.A != 0xdeadbeef || loc62.B != 0xcafebabe) throw new System.Exception();
			if (loc63.A != 0xdeadbeef || loc63.B != 0xcafebabe) throw new System.Exception();
			if (loc64.A != 0xdeadbeef || loc64.B != 0xcafebabe) throw new System.Exception();
			if (loc65.A != 0xdeadbeef || loc65.B != 0xcafebabe) throw new System.Exception();
			if (loc66.A != 0xdeadbeef || loc66.B != 0xcafebabe) throw new System.Exception();
			if (loc67.A != 0xdeadbeef || loc67.B != 0xcafebabe) throw new System.Exception();
			if (loc68.A != 0xdeadbeef || loc68.B != 0xcafebabe) throw new System.Exception();
			if (loc69.A != 0xdeadbeef || loc69.B != 0xcafebabe) throw new System.Exception();
			if (loc70.A != 0xdeadbeef || loc70.B != 0xcafebabe) throw new System.Exception();
			if (loc71.A != 0xdeadbeef || loc71.B != 0xcafebabe) throw new System.Exception();
			if (loc72.A != 0xdeadbeef || loc72.B != 0xcafebabe) throw new System.Exception();
			if (loc73.A != 0xdeadbeef || loc73.B != 0xcafebabe) throw new System.Exception();
			if (loc74.A != 0xdeadbeef || loc74.B != 0xcafebabe) throw new System.Exception();
			if (loc75.A != 0xdeadbeef || loc75.B != 0xcafebabe) throw new System.Exception();
			if (loc76.A != 0xdeadbeef || loc76.B != 0xcafebabe) throw new System.Exception();
			if (loc77.A != 0xdeadbeef || loc77.B != 0xcafebabe) throw new System.Exception();
			if (loc78.A != 0xdeadbeef || loc78.B != 0xcafebabe) throw new System.Exception();
			if (loc79.A != 0xdeadbeef || loc79.B != 0xcafebabe) throw new System.Exception();
			if (loc80.A != 0xdeadbeef || loc80.B != 0xcafebabe) throw new System.Exception();
			if (loc81.A != 0xdeadbeef || loc81.B != 0xcafebabe) throw new System.Exception();
			if (loc82.A != 0xdeadbeef || loc82.B != 0xcafebabe) throw new System.Exception();
			if (loc83.A != 0xdeadbeef || loc83.B != 0xcafebabe) throw new System.Exception();
			if (loc84.A != 0xdeadbeef || loc84.B != 0xcafebabe) throw new System.Exception();
			if (loc85.A != 0xdeadbeef || loc85.B != 0xcafebabe) throw new System.Exception();
			if (loc86.A != 0xdeadbeef || loc86.B != 0xcafebabe) throw new System.Exception();
			if (loc87.A != 0xdeadbeef || loc87.B != 0xcafebabe) throw new System.Exception();
			if (loc88.A != 0xdeadbeef || loc88.B != 0xcafebabe) throw new System.Exception();
			if (loc89.A != 0xdeadbeef || loc89.B != 0xcafebabe) throw new System.Exception();
			if (loc90.A != 0xdeadbeef || loc90.B != 0xcafebabe) throw new System.Exception();
			if (loc91.A != 0xdeadbeef || loc91.B != 0xcafebabe) throw new System.Exception();
			if (loc92.A != 0xdeadbeef || loc92.B != 0xcafebabe) throw new System.Exception();
			if (loc93.A != 0xdeadbeef || loc93.B != 0xcafebabe) throw new System.Exception();
			if (loc94.A != 0xdeadbeef || loc94.B != 0xcafebabe) throw new System.Exception();
			if (loc95.A != 0xdeadbeef || loc95.B != 0xcafebabe) throw new System.Exception();
			if (loc96.A != 0xdeadbeef || loc96.B != 0xcafebabe) throw new System.Exception();
			if (loc97.A != 0xdeadbeef || loc97.B != 0xcafebabe) throw new System.Exception();
			if (loc98.A != 0xdeadbeef || loc98.B != 0xcafebabe) throw new System.Exception();
			if (loc99.A != 0xdeadbeef || loc99.B != 0xcafebabe) throw new System.Exception();
			if (loc100.A != 0xdeadbeef || loc100.B != 0xcafebabe) throw new System.Exception();
			if (loc101.A != 0xdeadbeef || loc101.B != 0xcafebabe) throw new System.Exception();
			if (loc102.A != 0xdeadbeef || loc102.B != 0xcafebabe) throw new System.Exception();
			if (loc103.A != 0xdeadbeef || loc103.B != 0xcafebabe) throw new System.Exception();
			if (loc104.A != 0xdeadbeef || loc104.B != 0xcafebabe) throw new System.Exception();
			if (loc105.A != 0xdeadbeef || loc105.B != 0xcafebabe) throw new System.Exception();
			if (loc106.A != 0xdeadbeef || loc106.B != 0xcafebabe) throw new System.Exception();
			if (loc107.A != 0xdeadbeef || loc107.B != 0xcafebabe) throw new System.Exception();
			if (loc108.A != 0xdeadbeef || loc108.B != 0xcafebabe) throw new System.Exception();
			if (loc109.A != 0xdeadbeef || loc109.B != 0xcafebabe) throw new System.Exception();
			if (loc110.A != 0xdeadbeef || loc110.B != 0xcafebabe) throw new System.Exception();
			if (loc111.A != 0xdeadbeef || loc111.B != 0xcafebabe) throw new System.Exception();
			if (loc112.A != 0xdeadbeef || loc112.B != 0xcafebabe) throw new System.Exception();
			if (loc113.A != 0xdeadbeef || loc113.B != 0xcafebabe) throw new System.Exception();
			if (loc114.A != 0xdeadbeef || loc114.B != 0xcafebabe) throw new System.Exception();
			if (loc115.A != 0xdeadbeef || loc115.B != 0xcafebabe) throw new System.Exception();
			if (loc116.A != 0xdeadbeef || loc116.B != 0xcafebabe) throw new System.Exception();
			if (loc117.A != 0xdeadbeef || loc117.B != 0xcafebabe) throw new System.Exception();
			if (loc118.A != 0xdeadbeef || loc118.B != 0xcafebabe) throw new System.Exception();
			if (loc119.A != 0xdeadbeef || loc119.B != 0xcafebabe) throw new System.Exception();
			if (loc120.A != 0xdeadbeef || loc120.B != 0xcafebabe) throw new System.Exception();
			if (loc121.A != 0xdeadbeef || loc121.B != 0xcafebabe) throw new System.Exception();
			if (loc122.A != 0xdeadbeef || loc122.B != 0xcafebabe) throw new System.Exception();
			if (loc123.A != 0xdeadbeef || loc123.B != 0xcafebabe) throw new System.Exception();
			if (loc124.A != 0xdeadbeef || loc124.B != 0xcafebabe) throw new System.Exception();
			if (loc125.A != 0xdeadbeef || loc125.B != 0xcafebabe) throw new System.Exception();
			if (loc126.A != 0xdeadbeef || loc126.B != 0xcafebabe) throw new System.Exception();
			if (loc127.A != 0xdeadbeef || loc127.B != 0xcafebabe) throw new System.Exception();
			if (loc128.A != 0xdeadbeef || loc128.B != 0xcafebabe) throw new System.Exception();
			if (loc129.A != 0xdeadbeef || loc129.B != 0xcafebabe) throw new System.Exception();
			if (loc130.A != 0xdeadbeef || loc130.B != 0xcafebabe) throw new System.Exception();
			if (loc131.A != 0xdeadbeef || loc131.B != 0xcafebabe) throw new System.Exception();
			if (loc132.A != 0xdeadbeef || loc132.B != 0xcafebabe) throw new System.Exception();
			if (loc133.A != 0xdeadbeef || loc133.B != 0xcafebabe) throw new System.Exception();
			if (loc134.A != 0xdeadbeef || loc134.B != 0xcafebabe) throw new System.Exception();
			if (loc135.A != 0xdeadbeef || loc135.B != 0xcafebabe) throw new System.Exception();
			if (loc136.A != 0xdeadbeef || loc136.B != 0xcafebabe) throw new System.Exception();
			if (loc137.A != 0xdeadbeef || loc137.B != 0xcafebabe) throw new System.Exception();
			if (loc138.A != 0xdeadbeef || loc138.B != 0xcafebabe) throw new System.Exception();
			if (loc139.A != 0xdeadbeef || loc139.B != 0xcafebabe) throw new System.Exception();
			if (loc140.A != 0xdeadbeef || loc140.B != 0xcafebabe) throw new System.Exception();
			if (loc141.A != 0xdeadbeef || loc141.B != 0xcafebabe) throw new System.Exception();
			if (loc142.A != 0xdeadbeef || loc142.B != 0xcafebabe) throw new System.Exception();
			if (loc143.A != 0xdeadbeef || loc143.B != 0xcafebabe) throw new System.Exception();
			if (loc144.A != 0xdeadbeef || loc144.B != 0xcafebabe) throw new System.Exception();
			if (loc145.A != 0xdeadbeef || loc145.B != 0xcafebabe) throw new System.Exception();
			if (loc146.A != 0xdeadbeef || loc146.B != 0xcafebabe) throw new System.Exception();
			if (loc147.A != 0xdeadbeef || loc147.B != 0xcafebabe) throw new System.Exception();
			if (loc148.A != 0xdeadbeef || loc148.B != 0xcafebabe) throw new System.Exception();
			if (loc149.A != 0xdeadbeef || loc149.B != 0xcafebabe) throw new System.Exception();
			if (loc150.A != 0xdeadbeef || loc150.B != 0xcafebabe) throw new System.Exception();
			if (loc151.A != 0xdeadbeef || loc151.B != 0xcafebabe) throw new System.Exception();
			if (loc152.A != 0xdeadbeef || loc152.B != 0xcafebabe) throw new System.Exception();
			if (loc153.A != 0xdeadbeef || loc153.B != 0xcafebabe) throw new System.Exception();
			if (loc154.A != 0xdeadbeef || loc154.B != 0xcafebabe) throw new System.Exception();
			if (loc155.A != 0xdeadbeef || loc155.B != 0xcafebabe) throw new System.Exception();
			if (loc156.A != 0xdeadbeef || loc156.B != 0xcafebabe) throw new System.Exception();
			if (loc157.A != 0xdeadbeef || loc157.B != 0xcafebabe) throw new System.Exception();
			if (loc158.A != 0xdeadbeef || loc158.B != 0xcafebabe) throw new System.Exception();
			if (loc159.A != 0xdeadbeef || loc159.B != 0xcafebabe) throw new System.Exception();
			if (loc160.A != 0xdeadbeef || loc160.B != 0xcafebabe) throw new System.Exception();
			if (loc161.A != 0xdeadbeef || loc161.B != 0xcafebabe) throw new System.Exception();
			if (loc162.A != 0xdeadbeef || loc162.B != 0xcafebabe) throw new System.Exception();
			if (loc163.A != 0xdeadbeef || loc163.B != 0xcafebabe) throw new System.Exception();
			if (loc164.A != 0xdeadbeef || loc164.B != 0xcafebabe) throw new System.Exception();
			if (loc165.A != 0xdeadbeef || loc165.B != 0xcafebabe) throw new System.Exception();
			if (loc166.A != 0xdeadbeef || loc166.B != 0xcafebabe) throw new System.Exception();
			if (loc167.A != 0xdeadbeef || loc167.B != 0xcafebabe) throw new System.Exception();
			if (loc168.A != 0xdeadbeef || loc168.B != 0xcafebabe) throw new System.Exception();
			if (loc169.A != 0xdeadbeef || loc169.B != 0xcafebabe) throw new System.Exception();
			if (loc170.A != 0xdeadbeef || loc170.B != 0xcafebabe) throw new System.Exception();
			if (loc171.A != 0xdeadbeef || loc171.B != 0xcafebabe) throw new System.Exception();
			if (loc172.A != 0xdeadbeef || loc172.B != 0xcafebabe) throw new System.Exception();
			if (loc173.A != 0xdeadbeef || loc173.B != 0xcafebabe) throw new System.Exception();
			if (loc174.A != 0xdeadbeef || loc174.B != 0xcafebabe) throw new System.Exception();
			if (loc175.A != 0xdeadbeef || loc175.B != 0xcafebabe) throw new System.Exception();
			if (loc176.A != 0xdeadbeef || loc176.B != 0xcafebabe) throw new System.Exception();
			if (loc177.A != 0xdeadbeef || loc177.B != 0xcafebabe) throw new System.Exception();
			if (loc178.A != 0xdeadbeef || loc178.B != 0xcafebabe) throw new System.Exception();
			if (loc179.A != 0xdeadbeef || loc179.B != 0xcafebabe) throw new System.Exception();
			if (loc180.A != 0xdeadbeef || loc180.B != 0xcafebabe) throw new System.Exception();
			if (loc181.A != 0xdeadbeef || loc181.B != 0xcafebabe) throw new System.Exception();
			if (loc182.A != 0xdeadbeef || loc182.B != 0xcafebabe) throw new System.Exception();
			if (loc183.A != 0xdeadbeef || loc183.B != 0xcafebabe) throw new System.Exception();
			if (loc184.A != 0xdeadbeef || loc184.B != 0xcafebabe) throw new System.Exception();
			if (loc185.A != 0xdeadbeef || loc185.B != 0xcafebabe) throw new System.Exception();
			if (loc186.A != 0xdeadbeef || loc186.B != 0xcafebabe) throw new System.Exception();
			if (loc187.A != 0xdeadbeef || loc187.B != 0xcafebabe) throw new System.Exception();
			if (loc188.A != 0xdeadbeef || loc188.B != 0xcafebabe) throw new System.Exception();
			if (loc189.A != 0xdeadbeef || loc189.B != 0xcafebabe) throw new System.Exception();
			if (loc190.A != 0xdeadbeef || loc190.B != 0xcafebabe) throw new System.Exception();
			if (loc191.A != 0xdeadbeef || loc191.B != 0xcafebabe) throw new System.Exception();
			if (loc192.A != 0xdeadbeef || loc192.B != 0xcafebabe) throw new System.Exception();
			if (loc193.A != 0xdeadbeef || loc193.B != 0xcafebabe) throw new System.Exception();
			if (loc194.A != 0xdeadbeef || loc194.B != 0xcafebabe) throw new System.Exception();
			if (loc195.A != 0xdeadbeef || loc195.B != 0xcafebabe) throw new System.Exception();
			if (loc196.A != 0xdeadbeef || loc196.B != 0xcafebabe) throw new System.Exception();
			if (loc197.A != 0xdeadbeef || loc197.B != 0xcafebabe) throw new System.Exception();
			if (loc198.A != 0xdeadbeef || loc198.B != 0xcafebabe) throw new System.Exception();
			if (loc199.A != 0xdeadbeef || loc199.B != 0xcafebabe) throw new System.Exception();
			if (loc_i != 11) throw new System.Exception();
			if (loc_j != 12) throw new System.Exception();
		}
    }
}
