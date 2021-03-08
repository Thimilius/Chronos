namespace Chronos.Tests {
    /// <summary>
    /// Tests nested types.
    /// 
    /// Test from: https://github.com/mono/mono/blob/master/mono/tests/nested_type_visibility.cs
    /// </summary>
    public class Test_NestedTypes {
        private abstract class SuperClass {
            protected abstract class SuperInnerAbstractClass {
                protected class SuperInnerInnerClass {
                }
            }
        }

        private class ChildClass : SuperClass {
            private class ChildInnerClass : SuperInnerAbstractClass {
                private readonly SuperInnerInnerClass s_class = new SuperInnerInnerClass();
            }

            public ChildClass() {
                var childInnerClass = new ChildInnerClass();
            }
        }

        public static void Run() {
            new ChildClass();
        }
    }
}
