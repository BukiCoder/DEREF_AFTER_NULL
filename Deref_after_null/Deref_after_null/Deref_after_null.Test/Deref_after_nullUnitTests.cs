using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Deref_after_null.Test.CSharpCodeFixVerifier<
    Deref_after_null.Deref_after_nullAnalyzer,
    Deref_after_null.Deref_after_nullCodeFixProvider>;

namespace Deref_after_null.Test
{
    [TestClass]
    public class Deref_after_nullUnitTest
    {
        [TestMethod]
        public async Task TestMethod10()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
               public object m;
            }
            string f(A s)
            {
               if (s == null){}
               else if (s.m == null){}
               else{}
               [|s|].ToString();
               return [|[|s|].m|].ToString();

            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod12()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public object m;
                public object t;
            }
            string f(A s)
            {

                if ((s.m == null) || s.t == null)
                {

                    if (s.m == null)
                    {
                        if (s == null)
                        {
                            if ([|s|].m == null)
                            {

                            }
                        }
                    }
                }
                else
                {

                }
                [|s|].ToString();
                return [|[|s|].m|].ToString();

            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod13()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            string get_s()
            {
                return null;
            }
            string f()
            {

                object k = get_s();
                if (k == null)
                {
                    object p = get_s();
                    if (p == null) [|p|].ToString();
                }
                object p1 = get_s();
                return p1.ToString();
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod14()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            string get_s()
            {
                return null;
            }
            string ff(object s)
            {
                var k = get_s();
                if (k == null)
                {
                    var p = get_s();
                    if (p != null) p.ToString();
                }
                var p1 = get_s();
                return p1.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod15()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            bool SundayToday() { return false; }
            string f(object s)
            {

                if (SundayToday())
                {
                    if (s != null) return s.ToString();
                }
                return [|s|].ToString();
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod16()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            string foo(object s)
            {

                if (s != null)
                   { return s.ToString();}
                return [|s|].ToString();
            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod17()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public object m;
                public object t;
            }
            string f(A s)
            {

                if ((s.m == null) || s.t == null)
                {

                    if (s.m == null)
                    {
                        if (s == null)
                        {
                            if ([|s|].m == null)
                            {

                            }
                        }
                    }
                }
                s = new A();
                s.m = new A();
                s.ToString();
                return s.m.ToString(); //DEREF_AFTER_NULL??
            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod18()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public A m;
                public A t;
            }
            string fy(A s)
            {

                if (s.m == null) s.ToString();

                s = s.m;

                return [|s|].m.ToString();


            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod19()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public A m;
                public A t;
            }
            A someF() { return new A(); }
            string fy(A s)
            {

                if (s.m == null) s.ToString();
                A k = someF();
                A b = someF();
                if (b == null) [|b|].ToString();
                if (b != null) k = b;
                s.m = k;
                return s.m.ToString(); //DEREF_AFTER_NULL??


            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod20()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s)
            {
                if (s == null)
                {

                }
                else
                {
                    s.ToString();
                }
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod21()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s)
            {
                if (s == null)
                {

                }
                else
                {
                    s = null;
                    [|s|].ToString();
                }
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod22()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public A m;
                public A t;
            }
            void gg(A s)
            {
                if (s == null)
                {

                }
                else if (s.m != null)
                {

                    s.m.ToString();
                }
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod23()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(StringBuilder s)
            {
                if (s == null)
                {
                    s = new StringBuilder();
                }
                else
                {

                    s = null;
                }
                [|s|].ToString();
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod24()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(StringBuilder s, StringBuilder t)
            {
                if (s == null && t == null)
                {
                    s = new StringBuilder();
                }
                else if (t == null)
                {
                    t = new StringBuilder();
                    s = null;
                }
                else
                {
                    gg(null, null);
                }
                [|s|].ToString();
                [|t|].ToString();
            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod25()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(StringBuilder s, StringBuilder t)
            {
                if (s == null || t == null)
                {
                    s = new StringBuilder();
                }
                else if (t == null)
                {
                    t = new StringBuilder();
                    s = null;
                }
                else
                {
                    gg(null, null);
                }
                s.ToString();
                [|t|].ToString();
            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod26()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s)
            {

                if (s == null) return;
                s.ToString();
            }
            void gg1(object s)
            {

                if (s != null) return;
                [|s|].ToString();
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }


        [TestMethod]
        public async Task TestMethod27()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t)
            {
                if (s == null || t == null)
                {
                  [|s|].ToString();
                    return;
                }

                s.ToString();
                t.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod28()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            bool PlaySaxForMySoul() { return true; }
            void gg(object s, object t)
            {
                if (PlaySaxForMySoul())
                {
                    if (s == null || t == null)
                    {
                     [|s|].ToString();
                        return;
                    }
                }


                s.ToString();
                t.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod29()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            bool PlaySaxForMySoul() { return true; }
            void gg(object s, object t)
            {

                if (s == null || t == null)
                {
                    if (PlaySaxForMySoul())
                    {
                    [|s|].ToString();
                        return;
                    }
                }


                [|s|].ToString();
                [|t|].ToString();
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod30()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t)
            {

                while (s == null || t == null)
                {


                }

                s.ToString();
                t.ToString();
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod31()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t)
            {

                while (s == null || t == null)
                {
                    return;

                }

                s.ToString();
                t.ToString();
            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod32()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t)
            {

                while (s != null && t != null)
                {


                }

                s.ToString();
                t.ToString();
            }


        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod33()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t, int x)
            {

                while (s == null || t == null || x > 2)
                {


                }

                s.ToString();
                t.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod34()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            bool PlaySaxForMySoul() { return true; }
            void gg(StringBuilder s, object t, int x)
            {

                while (s == null)
                {

                    t = s;
                    if (PlaySaxForMySoul())
                    {
                        s = new StringBuilder();
                    }
                }

                s.ToString();
                t.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }






        [TestMethod]
        public async Task TestMethod35()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            class A { public A h; }
            string fg(A s, object h)
            {
                if (s.h == null)
                {
                    return [|s.h|].ToString();
                }
                h.ToString();
                return null;
            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod36()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            string fg(object[] s)
            {
                if (s[0] == null)
                {
                    return [|s[0]|].ToString();
                }
                return null;

            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }


        [TestMethod]
        public async Task TestMethod37()
        {
            var test = @"
    using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;
            using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class B
        {
            void fg()
            {
                StringBuilder a = new StringBuilder();
                StringBuilder c = new StringBuilder();
                c = a;
                if (a == null)
                {
                [|c|].ToString();
                }

            }

        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
