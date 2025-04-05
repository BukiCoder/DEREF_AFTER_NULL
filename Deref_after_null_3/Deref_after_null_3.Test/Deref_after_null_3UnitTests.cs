using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Deref_after_null_3.Test.CSharpCodeFixVerifier<
    Deref_after_null_3.Deref_after_null_3Analyzer,
    Deref_after_null_3.Deref_after_null_3CodeFixProvider>;

namespace Deref_after_null_3.Test
{
    [TestClass]
    public class Deref_after_null_3UnitTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {

            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {

            public class A
            {
                public object m;
            }
            
            string f(int x, A s)
            {
                if (x > 5)
                {
                    if (s == null) { f(0, null); }
                }
                else 
                { 
                    s = new A(); 
                }
             
                return [|s|].ToString();
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;

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
                if ((s.m == null) || s.t is null)
                {
                    if (s.m == null)
                    {
                        if (s == null) 
                        { 
                           f(null);
                        }
                    }
                }
                else 
                { 
                    f(null);
                }
                [|s|].ToString();
                return [|[|s|].m|].ToString();
            }
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }



        [TestMethod]
        public async Task TestMethod3()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {         
            string f(object k)
            {              
                if (k == null)
                {
                    if (k != null) 
                    {
                        k.ToString();
                    }
                }
                var s = k;
                [|k|].ToString();
                return [|s|].ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod4()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            bool SundayToday(bool b) { return b; }
            string f(object s, bool b)
            {
                if (SundayToday(b))
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
        public async Task TestMethod5()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            string f(object s)
            {
                if (s != null) { return s.ToString(); }
                return [|s|].ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }


        [TestMethod]
        public async Task TestMethod6()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public A m;
                public A t;
            }
            string f(A s)
            {
                if (s.m == null) { s.ToString(); }
                s = s.m;
                return [|s|].m.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod7()
        {
            var test = @"
    using System;

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
            string f(A s)
            {

                if (s.m == null) s.ToString();
                A k = someF();
                A b = someF();
      
                if (b != null) k = b;
                k.ToString();
                s.m = k;
                return s.m.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod8()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            void f(object s)
            {
                if (s is null)
                {
                    if (s is not null) s.ToString();
                    else
                    {
                        s?.ToString();
                        [|s|].ToString();
                    }
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
        public async Task TestMethod9()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            void f(object s, object p, int x)
            {
                if (s == null || p == null || x > 2) { }
                else { }
                [|p|].ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod10()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            void f(ref object s)
            {
                s = new Exception();
            }
            void ff(object s)
            {
                if (s is not null) { }
                [|s|].ToString();
                if (s is null || s.ToString() is null)
                {
                    f(ref s);
                    s.ToString();
                }
                s.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
        [TestMethod]
        public async Task TestMethod11()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public A m;
                public A t;
            }
            void f(A s)
            {
                if (s.m != null)
                {
                    s.m.ToString();
                }
                if ([|s.m|].m == null) { }
                s.ToString();
                s.m?.ToString();
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
    namespace ConsoleApplication1
    {
        class B
        {
            void f(Exception s, Exception g)
            {
                if (s is null || g is null)
                {
                    s = new Exception();
                }
                [|g|].ToString();
                s.ToString();
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

    namespace ConsoleApplication1
    {
        using Env = System.Environment;
        class B
        {     
            void f(object s)
            {   
                if (s == null) Env.Exit(0);
                s.ToString();
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

    namespace ConsoleApplication1
    {
        class B
        {
            void f(object s, object t)
            {
                if (s == null && t == null)
                {
                    return;
                }
                [|s|].ToString();
                [|t|].ToString();
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
        public async Task TestMethod16()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {        
            void f(object s, object t)
            {

                if (s == null || t == null)
                {
                    return;
                }

                s.ToString();
                t.ToString();

                if(s == null) { }
                s.ToString();             

                s = t;
                if(s == null) { }
                s.ToString();

                s = null;
                [|s|].ToString();
                if(s == null) { }
                [|s|].ToString();
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

    namespace ConsoleApplication1
    {
        class B
        {
            void f(object s, object t, int x, object r)
            {
                while (s == null || t == null || x > 2)
                {
                    if(t == null) { }
                    if(r == null) { }
                }

                s.ToString();
                t.ToString();
                [|r|].ToString();
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
    using System.Text;

    namespace ConsoleApplication1
    {
        class B
        {
            bool PlaySaxForMySoul() { return true; }
            void f(StringBuilder s, object t, int x)
            {
                while (s == null)
                {
                    t = s;
                    if (PlaySaxForMySoul())
                    {
                        s = new StringBuilder();
                    }
                    if (t == null) { }
                }

                s.ToString();
                [|t|].ToString();
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

    namespace ORTS.TrackViewer.Editing
    {     
        public class TrainpathNode { }
        public class TrPathNode { }
        public class Trainpath
        {
            object dynamicWeather = new TrainpathNode();
            void f(object s, object a, bool weatherChangeOn)
            {
                if (a != null && s != null)
                {
                    if (dynamicWeather == null)
                    {
                        dynamicWeather = new TrainpathNode();
                    }
                }
                if (weatherChangeOn)
                {
                    dynamicWeather.ToString();
                }
            }
        }
    }
";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestMethod20()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
           
            B(object s, object t, int x)
            {
                if(t is null)
                {
                  t = new Exception();
                }
                if(s is null)
                {
                   s = t;
                }
                s.ToString();
                t.ToString();
                
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

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t, int x)
            {

              if(s == null)
              {  
              }
              while(s == null)
              {
                  x++;
                  if(x > 0) s = new Exception();
              }
              s.ToString();
               
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

    namespace ConsoleApplication1
    {
        class B
        {
            void f(object s, object t, int x)
            {
                if (t == null)
                {
                    if (x > 0)
                    {
                        t = new Exception();
                    }
                    else
                    {
                        t = new Exception();
                    }
                }
                t.ToString();
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

    namespace ConsoleApplication1
    {
        class B
        {
            void f(object s, object t, int x)
            {
                while (x > 0 && s == null)
                {
                    if (s != null) { }
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

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t, int x)
            {

                while (true)
                {
                    if (s == null)
                    {
                      
                    }
                    break;
                }
                [|s|].ToString();

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

    namespace ConsoleApplication1
    {
        class A
        {
            public A a;
        }
        class B
        {
            void f(A s, object t, int x)
            {
                if (s.a.a.a == null && s.a == null || x > 2)
                {
                    if (s == null) { }
                    [|s|].ToString();
                }
                [|s|].ToString();
                [|[|s|].a|].a.ToString();
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

    namespace ConsoleApplication1
    {
        class A
        {
            public A a;
        }
        
        class B
        {
            A s;
            void f(A f) { s = new A(); }
            void f(object t)
            {
                if (s == null || s.a == null) { }
                [|s|].ToString();
                [|[|s|].a|].ToString();
                f(s);
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
    using System.Text;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(StringBuilder s, object t, int x)
            {
                if(t == null)
                {
                }
                if (s == null)
                {
                    return;
                }
                s.ToString();
                [|t|].ToString();
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

    namespace ConsoleApplication1
    {
        class A
        {
            public A a;
        }
       
        class B
        {
            void f(object t, A s)
            {
                if(s?.a.a?.a == null) { }
                [|s|].ToString();
                [|[|s|].a.a|].ToString();
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

    namespace ConsoleApplication1
    {
        class A
        {
            public A a;
        }
        class B
        {
            void f(object t, A s)
            {
                if (s == null) { }
                do
                {
                   [|s|].ToString();
                }
                while (s == null);
                s.ToString();
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

    namespace ConsoleApplication1
    {
        class A
        {
            public A a;
        }
        class B
        {

            void f(object t, A s)
            {
                s = new A();
                if(s == null)
                {
                    s = null;      
                }
                s.ToString();
                s.a.ToString();
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

    namespace ConsoleApplication1
    {
        class B
        {
            public class A
            {
                public object m;
            }

            A s = null;
            string f(int x)
            {
                if (s == null) { }
                else { s = new A(); }
                s = new A();
                s = null;
                return [|s|].ToString();
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

    namespace ConsoleApplication1
    {
        class A
        {
            public A a;
        }
        class B
        {

            void f(object t, A[] s)
            {
               
                if(s == null) { }
                foreach(var i in [|s|]) { } 
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

    namespace ConsoleApplication1
    {
        class B
        {
           
            void f(object s, object t, int x)
            {

                if(s != null && t == null)
                {
                   s = t;
                }
                [|s|].ToString();
                [|t|].ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }

    }
}
