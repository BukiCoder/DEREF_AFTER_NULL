using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Deref_aftet_null_2.Test.CSharpCodeFixVerifier<
    Deref_aftet_null_2.Deref_aftet_null_2Analyzer,
    Deref_aftet_null_2.Deref_aftet_null_2CodeFixProvider>;

namespace Deref_aftet_null_2.Test
{
    [TestClass]
    public class Deref_aftet_null_2UnitTest
    {
        //No diagnostics expected to show up

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

            A s = null;
            string f(int x)
            {
                if (x > 5)
                {
                    if (s == null) { }
                }
                else { s = new A(); }

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
                        if (s == null) { }
                    }
                }
                else { }
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
        public async Task TestMethod4()
        {
            var test = @"
    using System;

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
        public async Task TestMethod5()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            string foo(object s)
            {
                if (s != null) { return s.ToString();}
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
            string fy(A s)
            {

                if (s.m == null) s.ToString();
                A k = someF();
                A b = someF();
      
                if (b != null) k = b;
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
        public async Task TestMethod9()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object p, int x)
            {
                if (s == null || p == null || x > 2)
                {
                     
                }
                else
                {
                 
                }
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
            void f(ref object s) { }
            void gg(object s)
            {
                if (s == null)
                {

                }
                f(ref s);
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
        public async Task TestMethod12()
        {
            var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class B
        {
            void gg(Exception s)
            {
                if (s == null)
                {
                    s = new Exception();
                }
               
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
        class B
        {
            void gg(object sa)
            {
                if (sa == null) return;
                sa.ToString();
            }
            void gg_throw(object sa)
            {
                if (sa == null) throw new Exception();
                sa.ToString();
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
        public async Task TestMethod14()
        {
            var test = @"
    using System;

    namespace ConsoleApplication1
    {
        class B
        {
            void gg(object s, object t)
            {
                if (s == null || t == null)
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
            void gg(object s, object t)
            {

                if (s == null || t == null)
                {
                       return;
                }

                s.ToString();
                t.ToString();
            }

            void gg_loop(object s, object t)
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
        public async Task TestMethod17()
        {
            var test = @"
    using System;

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
            void gg(StringBuilder s, object t, int x)
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
            using System.Collections.ObjectModel;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;

    namespace ORTS.TrackViewer.Editing
    {
        /// <summary>
        /// Defines the internal representation of a MSTS path. 
        /// The path itself is actually defined as a linked-list. That leaves the following things for this class
        ///     A link to the start point of the path
        ///     metadata like name of path, start end end point. These are public fields
        ///     A boolean describing whether the path has a well-defined end (meaning a node that has been designated as an end-node
        ///             instead of simply being the last node).
        ///     Routines to create the linked-list path from the definitions in a .pat file.
        ///     
        /// The class contains history functions like Undo and redo.
        /// </summary>
        public class TrainpathNode
        {
            public TrainpathNode NextMainNode;
            public TrainpathNode PrevNode;
            public TrainpathNode NextSidingNode;
            public int NextMainTvnIndex;
            public bool NextSidingTvnIndex;

        }

        public class TrPathNode
        {
            public bool HasNextSidingNode;
            public bool HasNextMainNode;
        }
        public class Trainpath
        {
            object dynamicWeather = new TrainpathNode();
            void f(object s, object a, bool weatherChangeOn)
            {
                if (a != null && s != null)
                {
                    // Start a weather change sequence in activity mode
                    // if not yet weather changes, create the instance
                    if (dynamicWeather == null)
                    {
                        dynamicWeather = new TrainpathNode();
                    }


                }

                if (weatherChangeOn)
                {
                    // Manage the weather change sequence
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
           
            void gg(object s, object t, int x)
            {

                if(s == null || t == null)
                {
                   s = t;
                }
                s.ToString();
                [|t|].ToString();
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
           
            void gg(object s, object t, int x)
            {

              if(s == null)
              {  
              }
              while(s == null)
              {
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

            void gg(object s, object t, int x)
            {


                while (x > 0)
                {
                    if (s != null)
                    { }
                }
                [|s|].ToString();

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

            void gg(object s, object t, int x)
            {
                while (x > 0 && s == null)
                {
                    if (s != null)
                    { }
                }
                s.ToString();
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
                        if (x > 0)
                        {
                            return;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                s.ToString();

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
            void gg(A s, object t)
            {
                if (s == null || s.a == null) return;
                s.ToString();
                s.a.ToString();
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
            public A[] a;
            public int x;

        }
        class B
        {
            A s;
            bool f() { return true; }
            void gg(object t, int x)
            {

                if (s.a == null || s.x == 0 || f())
                {
                    if (Equals(s, s.a)) { Equals(s, s.a); }
                    return;
                }
                if (s.x == 0 || x > s.a[0].x)
                {
                    if (Equals(s, s.a)) { Equals(s, s.a); }

                    return;
                }
                var av = s.a[0].x;
                s.a.ToString();
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
            void gg(object t)
            {
                if (s == null || s.a == null) { }
                [|s|].ToString();
                [|[|s|].a|].ToString();
                f(s);
                s.ToString();
                s.a.ToString();
            }
        }
    }";


            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}