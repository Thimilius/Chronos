using System;
using System.Diagnostics;

namespace Chronos.Tests {
    /// <summary>
    /// Tests casting an array.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/array-cast.cs
    /// </summary>
    public class Test_ArrayCast {
        public static void Run() {
            Attribute[] attr_array = new Attribute[1];
            object obj = (object)attr_array;
            object[] obj_array = (object[])obj;

            obj_array = obj as object[];

            Debug.Assert(obj_array != null);
        }
    }
}
