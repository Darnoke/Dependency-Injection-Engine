using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using lista9;

namespace UnitTestLista10
{
    [TestClass]
    public class UnitTest1
    {
        SimpleContainer c = new SimpleContainer();

        [TestMethod]
        public void Base()
        {
            c.Clean();

            c.RegisterType<Foo>(false);

            Foo f = c.Resolve<Foo>();

            c.RegisterType<IFoo, Foo>(false);

            IFoo ff = c.Resolve<IFoo>();

            Assert.AreNotEqual(f, ff);

            c.RegisterType<Foo, Boo>(true);

            ff = c.Resolve<IFoo>();

            IFoo fff = c.Resolve<IFoo>();

            Assert.AreEqual(ff, fff);
        }

        [TestMethod]
        public void Task1()
        {
            c.Clean();

            IFoo foo1 = new Foo();
            c.RegisterInstance<IFoo>(foo1);
            IFoo foo2 = c.Resolve<IFoo>();

            Assert.AreEqual(foo1, foo2);
        }

        public class A
        {
            public B b;
            public A(B b)
            {
                this.b = b;
            }
        }
        public class B { }

        [TestMethod]
        public void Task2a()
        {
            c.Clean();

            A a = c.Resolve<A>();

            Assert.IsNotNull(a);
            Assert.IsNotNull(a.b);
        }

        public class X
        {
            public X(Y d, string s) { }
        }
        public class Y { }

        [TestMethod]
        [ExpectedException(typeof(Exception), "No constructor known")]
        public void Task2b()
        {
            c.Clean();

            X x = c.Resolve<X>();
        }

        [TestMethod]
        public void Task2c()
        {
            c.Clean();

            c.RegisterInstance("ala ma kota");
            X x = c.Resolve<X>();

            Assert.IsNotNull(x);
        }

        public class Apple { }
        public class O
        {
            public P b;
            public IC c;
            [DependencyProperty]
            public Apple jablko { get; set; }
            public O(P b, IC c)
            {
                this.b = b;
            }
        }
        public class P { }
        public interface IC { }
        public class C : IC { }

        [TestMethod]
        public void Task2d()
        {
            c.Clean();

            c.RegisterType<IC, C>(false);
            O a = c.Resolve<O>();

            Assert.IsNotNull(a);
            Assert.IsNotNull(a.b);
        }

        
        class Car { }
        class Teacher
        {
            public Apple apple;
            private Car car;
            public Teacher(Apple a, Car c)
            {
                apple = a;
                car = c;
            }
            [DependencyConstructor]
            public Teacher(Car c)
            {
                car = c;
            }
        }


        [TestMethod]
        public void Task2e()
        {
            c.Clean();

            Teacher t = c.Resolve<Teacher>();

            Assert.IsNull(t.apple);
        }

        class Robot
        {
            public Robot(Robot r)
            {

            }

            
            
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "looped :(")] // dziala tylko na glebokosci 1 :(
        public void Task2f()
        {
            c.Clean();

            Robot r = c.Resolve<Robot>();
        }

        [TestMethod]
        public void Task3a()
        {
            c.Clean();

            c.RegisterType<IC, C>(false);
            O a = c.Resolve<O>();

            Assert.IsNotNull(a.jablko);
        }

        public class Kajak
        {
            [DependencyProperty]
            public Apple jablko { get; set; }
        }

        [TestMethod]
        public void Task3b()
        {
            c.Clean();

            Kajak k = new Kajak();
            c.BuildUp<Kajak>(k);

            Assert.IsNotNull(k.jablko);
        }
    }
}
