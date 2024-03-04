using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformTable.Ext
{
    public static class FPExt
    {
        public static Func<M, EmptyClass> ToFunc<M>(this Action<M> act)
            => (m) =>
            {
                act(m);
                return new EmptyClass();
            };

        public static Func<EmptyClass> ToFunc(this Action act)
           => () =>
           {
               act();
               return new EmptyClass();
           };
    }
    public class EmptyClass
    {
    }
}
