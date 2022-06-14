using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// Leberkässemmel

namespace lista9
{
    public class SimpleContainer
    {
        Dictionary<Type, bool> RegisteredBases = new Dictionary<Type, bool>();
        Dictionary<Type, object> RegisteredSingletons = new Dictionary<Type, object>();
        Dictionary<Type, Type> RegisteredExtensions = new Dictionary<Type, Type>();
        public void RegisterType<T>(bool singleton) where T : class
        {
            RegisteredBases.Add(typeof(T), singleton);
            if (singleton)
            {
                RegisteredSingletons.Add(typeof(T), Resolve<T>());
            }
        }
        public void RegisterType<From, To>(bool singleton) where To : From
        {
            RegisteredExtensions.Add(typeof(From), typeof(To));
            if (singleton)
            {
                RegisteredSingletons.Add(typeof(From), Resolve<To>());
            }
        }
        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type t)
        {
            while (RegisteredExtensions.ContainsKey(t))
            {
                if (RegisteredSingletons.ContainsKey(t)) return RegisteredSingletons[t];
                t = RegisteredExtensions[t];
            }
            if (RegisteredBases.Keys.Contains(t) && RegisteredBases[t])
            {
                return RegisteredSingletons[t];
            }
            if (t.IsAbstract || t.IsInterface) throw new Exception("Type is interface or abstract and is not registered");

            var constructors = t.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new Exception("No constructor known");
            }

            // 1. Attribute 2. length of parameters

            ConstructorInfo best_con = null;

            foreach (var constructor in constructors)
            {
                if (constructor.CustomAttributes.Any(c => c.AttributeType == typeof(DependencyConstructorAttribute)))
                {
                    if (best_con != null)
                    {
                        throw new Exception("multiple DependencyConstructorAttribute");
                    }
                    best_con = constructor;
                }
            }
            if (best_con is null)
            {
                constructors = constructors.OrderByDescending(x => x.GetParameters().Length).ToArray();
                if (constructors.Length > 1 && constructors[0].GetParameters().Length == constructors[1].GetParameters().Length) throw new Exception("multiple constructors of the same max length");
                best_con = constructors[0];
            }

            List<object> parameters = new List<object>();
            foreach (var param in best_con.GetParameters())
            {
                if (param.ParameterType == t) throw new Exception("looped :(");
                parameters.Add(Resolve(param.ParameterType));
            }
            object masterpiece = best_con.Invoke(parameters.ToArray());

            foreach (var prop in t.GetProperties())
            {
                if (prop.CustomAttributes.Any(c => c.AttributeType == typeof(DependencyPropertyAttribute)))
                {
                    prop.SetValue(masterpiece, Resolve(prop.PropertyType));
                }
            }

            return masterpiece;
        }

        public void RegisterInstance<T>(T instance)
        {
            RegisteredBases.Add(typeof(T), true);
            RegisteredSingletons.Add(typeof(T), instance);
        }

        public void Clean()
        {
            RegisteredBases.Clear();
            RegisteredSingletons.Clear();
            RegisteredExtensions.Clear();
        }

        public void BuildUp<T>(T Instance)
        {
            Type t = typeof(T);
            foreach (var prop in t.GetProperties())
            {
                if (prop.CustomAttributes.Any(c => c.AttributeType == typeof(DependencyPropertyAttribute)))
                {
                    prop.SetValue(Instance, Resolve(prop.PropertyType));
                }
            }
        }
    }
    public class Boo : Foo { }
    public class Foo : IFoo
    {

    }

    public interface IFoo
    {

    }

    public class Wojtek
    {
        private Foo _kicus;
        [DependencyConstructor]
        public Wojtek(Foo kicus)
        {
            _kicus = kicus;
        }
        public Wojtek(Foo kicus, Boo bobr)
        {
            _kicus = kicus;
        }
    }

    public class DependencyConstructorAttribute : Attribute
    {
    }

    public class DependencyPropertyAttribute : Attribute
    {
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            //SimpleContainer c = new SimpleContainer();
            //c.RegisterType<Foo>(false);
            //c.RegisterType<Wojtek>(false);
            //Wojtek sniady = c.Resolve<Wojtek>();
            //Console.ReadLine();
        }
    }
}
