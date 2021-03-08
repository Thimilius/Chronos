using System.Runtime.CompilerServices;

namespace System {
    public class Object {
        public Object() {
        
        }

        ~Object() {
        
        }

        public virtual string ToString() {
            return GetType().ToString();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern Type GetType();

        public virtual bool Equals(object obj) {
            return EqualsHelper(this, obj);
        }

        public static bool Equals(object objA, object objB) {
            if (objA == objB) {
                return true;
            }
            if (objA == null || objB == null) {
                return false;
            }
            return objA.Equals(objB);
        }

        public static bool ReferenceEquals(object objA, object objB) {
            return objA == objB;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        protected extern object MemberwiseClone();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool EqualsHelper(object a, object b);
    }
}