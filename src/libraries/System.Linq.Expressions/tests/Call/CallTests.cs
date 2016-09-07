// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace System.Linq.Expressions.Tests
{
    public static class CallTests
    {
        private struct Mutable
        {
            private int x;

            public int X
            {
                get { return x; }
                set { x = value; }
            }

            public int this[int i]
            {
                get { return x; }
                set { x = value; }
            }

            public int Foo()
            {
                return x++;
            }
        }

        private class Wrapper<T>
        {
            public const int Zero = 0;
            public T Field;
#pragma warning disable 649 // For testing purposes
            public readonly T ReadOnlyField;
#pragma warning restore
            public T Property
            {
                get { return Field; }
                set { Field = value; }
            }
        }

        private static class Methods
        {
            public static void ByRef(ref int x) { ++x; }
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void UnboxReturnsReference(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(object));
            var unbox = Expression.Unbox(p, typeof(Mutable));
            var call = Expression.Call(unbox, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<object, int>>(call, p).Compile(useInterpreter);

            object boxed = new Mutable();
            Assert.Equal(0, lambda(boxed));
            Assert.Equal(1, lambda(boxed));
            Assert.Equal(2, lambda(boxed));
            Assert.Equal(3, lambda(boxed));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ArrayWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Mutable[]));
            var indexed = Expression.ArrayIndex(p, Expression.Constant(0));
            var call = Expression.Call(indexed, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Mutable[], int>>(call, p).Compile(useInterpreter);

            var array = new Mutable[1];
            Assert.Equal(0, lambda(array));
            Assert.Equal(1, lambda(array));
            Assert.Equal(2, lambda(array));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void MultiRankArrayWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Mutable[,]));
            var indexed = Expression.ArrayIndex(p, Expression.Constant(0), Expression.Constant(0));
            var call = Expression.Call(indexed, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Mutable[,], int>>(call, p).Compile(useInterpreter);

            var array = new Mutable[1, 1];
            Assert.Equal(0, lambda(array));
            Assert.Equal(1, lambda(array));
            Assert.Equal(2, lambda(array));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ArrayAccessWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Mutable[]));
            var indexed = Expression.ArrayAccess(p, Expression.Constant(0));
            var call = Expression.Call(indexed, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Mutable[], int>>(call, p).Compile(useInterpreter);

            var array = new Mutable[1];
            Assert.Equal(0, lambda(array));
            Assert.Equal(1, lambda(array));
            Assert.Equal(2, lambda(array));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void MultiRankArrayAccessWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Mutable[,]));
            var indexed = Expression.ArrayAccess(p, Expression.Constant(0), Expression.Constant(0));
            var call = Expression.Call(indexed, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Mutable[,], int>>(call, p).Compile(useInterpreter);

            var array = new Mutable[1, 1];
            Assert.Equal(0, lambda(array));
            Assert.Equal(1, lambda(array));
            Assert.Equal(2, lambda(array));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void IndexedPropertyAccessNoWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(List<Mutable>));
            var indexed = Expression.Property(p, typeof(List<Mutable>).GetProperty("Item"), Expression.Constant(0));
            var call = Expression.Call(indexed, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<List<Mutable>, int>>(call, p).Compile(useInterpreter);

            var list = new List<Mutable> { new Mutable() };
            Assert.Equal(0, lambda(list));
            Assert.Equal(0, lambda(list));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void FieldAccessWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Wrapper<Mutable>));
            var member = Expression.Field(p, typeof(Wrapper<Mutable>).GetField("Field"));
            var call = Expression.Call(member, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Wrapper<Mutable>, int>>(call, p).Compile(useInterpreter);

            var wrapper = new Wrapper<Mutable>();
            Assert.Equal(0, lambda(wrapper));
            Assert.Equal(1, lambda(wrapper));
            Assert.Equal(2, lambda(wrapper));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void PropertyAccessNoWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Wrapper<Mutable>));
            var member = Expression.Property(p, typeof(Wrapper<Mutable>).GetProperty("Property"));
            var call = Expression.Call(member, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Wrapper<Mutable>, int>>(call, p).Compile(useInterpreter);

            var wrapper = new Wrapper<Mutable>();
            Assert.Equal(0, lambda(wrapper));
            Assert.Equal(0, lambda(wrapper));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ReadonlyFieldAccessWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Wrapper<Mutable>));
            var member = Expression.Field(p, typeof(Wrapper<Mutable>).GetField("ReadOnlyField"));
            var call = Expression.Call(member, typeof(Mutable).GetMethod("Foo"));
            var lambda = Expression.Lambda<Func<Wrapper<Mutable>, int>>(call, p).Compile(useInterpreter);

            var wrapper = new Wrapper<Mutable>();
            Assert.Equal(0, lambda(wrapper));
            Assert.Equal(0, lambda(wrapper));
            Assert.Equal(0, lambda(wrapper));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void ConstFieldAccessWriteBack(bool useInterpreter)
        {
            var member = Expression.Field(null, typeof(Wrapper<Mutable>).GetField("Zero"));
            var call = Expression.Call(member, typeof(int).GetMethod("GetType"));
            var lambda = Expression.Lambda<Func<Type>>(call).Compile(useInterpreter);

            var wrapper = new Wrapper<Mutable>();
            Assert.Equal(typeof(int), lambda());
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CallByRefMutableStructPropertyWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Mutable));
            var x = Expression.Property(p, "X");
            var call = Expression.Call(typeof(Methods).GetMethod("ByRef"), x);
            var body = Expression.Block(call, x);
            var lambda = Expression.Lambda<Func<Mutable, int>>(body, p).Compile(useInterpreter);

            var m = new Mutable() { X = 41 };
            Assert.Equal(42, lambda(m));
        }

        [Theory]
        [ClassData(typeof(CompilationTypes))]
        public static void CallByRefMutableStructIndexWriteBack(bool useInterpreter)
        {
            var p = Expression.Parameter(typeof(Mutable));
            var x = Expression.MakeIndex(p, typeof(Mutable).GetProperty("Item"), new[] { Expression.Constant(0) });
            var call = Expression.Call(typeof(Methods).GetMethod("ByRef"), x);
            var body = Expression.Block(call, x);
            var lambda = Expression.Lambda<Func<Mutable, int>>(body, p).Compile(useInterpreter);

            var m = new Mutable() { X = 41 };
            Assert.Equal(42, lambda(m));
        }

        private static Expression s_valid => Expression.Constant(5);

        private static MethodInfo s_method0 = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.Method0));
        private static MethodInfo s_method1 = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.Method1));
        private static MethodInfo s_method2 = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.Method2));
        private static MethodInfo s_method3 = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.Method3));
        private static MethodInfo s_method4 = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.Method4));
        private static MethodInfo s_method5 = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.Method5));

        public static IEnumerable<object[]> Method_Invalid_TestData()
        {
            yield return new object[] { null, typeof(ArgumentNullException) };
            yield return new object[] { typeof(GenericClass<>).GetMethod(nameof(GenericClass<string>.NonGenericMethod)), typeof(ArgumentException) };
            yield return new object[] { typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.GenericMethod)), typeof(ArgumentException) };
        }

        [Theory]
        [MemberData(nameof(Method_Invalid_TestData))]
        public static void Method_Invalid(MethodInfo method, Type exceptionType)
        {
            AssertArgumentException(() => Expression.Call(null, method), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(null, method, s_valid, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(null, method, s_valid, s_valid, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(null, method, new Expression[0]), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(null, method, (IEnumerable<Expression>)new Expression[0]), exceptionType, "method");

            AssertArgumentException(() => Expression.Call(method, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(method, s_valid, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(method, s_valid, s_valid, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(method, s_valid, s_valid, s_valid, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(method, s_valid, s_valid, s_valid, s_valid, s_valid), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(method, new Expression[0]), exceptionType, "method");
            AssertArgumentException(() => Expression.Call(method, (IEnumerable<Expression>)new Expression[0]), exceptionType, "method");
        }
        
        [Fact]
        public static void InstanceMethod_NullInstance_ThrowsArgumentException()
        {
            MethodInfo method = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.InstanceMethod));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, s_valid, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, s_valid, s_valid, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, s_valid, s_valid, s_valid, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, s_valid, s_valid, s_valid, s_valid, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, new Expression[0]));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, (IEnumerable<Expression>)new Expression[0]));

            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, s_valid, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, s_valid, s_valid, s_valid));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, new Expression[0]));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, (IEnumerable<Expression>)new Expression[0]));
        }

        [Fact]
        public static void StaticMethod_NonNullInstance_ThrowsArgumentException()
        {
            Expression instance = Expression.Constant(new NonGenericClass());
            MethodInfo method = typeof(NonGenericClass).GetMethod(nameof(NonGenericClass.StaticMethod));
            Assert.Throws<ArgumentException>("instance", () => Expression.Call(instance, method, s_valid));
            Assert.Throws<ArgumentException>("instance", () => Expression.Call(instance, method, s_valid, s_valid));
            Assert.Throws<ArgumentException>("instance", () => Expression.Call(instance, method, s_valid, s_valid, s_valid));
            Assert.Throws<ArgumentException>("instance", () => Expression.Call(instance, method, new Expression[0]));
            Assert.Throws<ArgumentException>("instance", () => Expression.Call(instance, method, (IEnumerable<Expression>)new Expression[0]));
        }

        public static IEnumerable<object[]> InvalidArg_TestData()
        {
            yield return new object[] { null, typeof(ArgumentNullException) };
            yield return new object[] { Expression.Property(null, typeof(Unreadable<string>), nameof(Unreadable<string>.WriteOnly)), typeof(ArgumentException) };
            yield return new object[] { Expression.Constant("abc"), typeof(ArgumentException) };
        }

        [Theory]
        [MemberData(nameof(InvalidArg_TestData))]
        public static void Arg0_Invalid(Expression arg, Type exceptionType)
        {
            AssertArgumentException(() => Expression.Call(s_method1, arg), exceptionType, "arg0");
            AssertArgumentException(() => Expression.Call(s_method2, arg, s_valid), exceptionType, "arg0");
            AssertArgumentException(() => Expression.Call(s_method3, arg, s_valid, s_valid), exceptionType, "arg0");
            AssertArgumentException(() => Expression.Call(s_method4, arg, s_valid, s_valid, s_valid), exceptionType, "arg0");
            AssertArgumentException(() => Expression.Call(s_method5, arg, s_valid, s_valid, s_valid, s_valid), exceptionType, "arg0");

            AssertArgumentException(() => Expression.Call(null, s_method1, arg), exceptionType, "arg0");
            AssertArgumentException(() => Expression.Call(null, s_method2, arg, s_valid), exceptionType, "arg0");
            AssertArgumentException(() => Expression.Call(null, s_method3, arg, s_valid, s_valid), exceptionType, "arg0");
        }

        [Theory]
        [MemberData(nameof(InvalidArg_TestData))]
        public static void Arg1_Invalid(Expression arg, Type exceptionType)
        {
            AssertArgumentException(() => Expression.Call(s_method2, s_valid, arg), exceptionType, "arg1");
            AssertArgumentException(() => Expression.Call(s_method3, s_valid, arg, s_valid), exceptionType, "arg1");
            AssertArgumentException(() => Expression.Call(s_method4, s_valid, arg, s_valid, s_valid), exceptionType, "arg1");
            AssertArgumentException(() => Expression.Call(s_method5, s_valid, arg, s_valid, s_valid, s_valid), exceptionType, "arg1");
            
            AssertArgumentException(() => Expression.Call(null, s_method2, s_valid, arg), exceptionType, "arg1");
            AssertArgumentException(() => Expression.Call(null, s_method3, s_valid, arg, s_valid), exceptionType, "arg1");
        }

        [Theory]
        [MemberData(nameof(InvalidArg_TestData))]
        public static void Arg2_Invalid(Expression arg, Type exceptionType)
        {
            AssertArgumentException(() => Expression.Call(s_method3, s_valid, s_valid, arg), exceptionType, "arg2");
            AssertArgumentException(() => Expression.Call(s_method4, s_valid, s_valid, arg, s_valid), exceptionType, "arg2");
            AssertArgumentException(() => Expression.Call(s_method5, s_valid, s_valid, arg, s_valid, s_valid), exceptionType, "arg2");
            
            AssertArgumentException(() => Expression.Call(null, s_method3, s_valid, s_valid, arg), exceptionType, "arg2");
        }

        [Theory]
        [MemberData(nameof(InvalidArg_TestData))]
        public static void Arg3_Invalid(Expression arg, Type exceptionType)
        {
            AssertArgumentException(() => Expression.Call(s_method4, s_valid, s_valid, s_valid, arg), exceptionType, "arg3");
            AssertArgumentException(() => Expression.Call(s_method5, s_valid, s_valid, s_valid, arg, s_valid), exceptionType, "arg3");
        }

        [Theory]
        [MemberData(nameof(InvalidArg_TestData))]
        public static void Arg4_Invalid(Expression arg, Type exceptionType)
        {
            AssertArgumentException(() => Expression.Call(s_method5, s_valid, s_valid, s_valid, s_valid, arg), exceptionType, "arg4");
        }

        private static void AssertArgumentException(Action action, Type exceptionType, string paramName)
        {
            ArgumentException ex = (ArgumentException)Assert.Throws(exceptionType, action);
            Assert.Equal(paramName, ex.ParamName);
        }

        [Theory]
        [InlineData(typeof(NonGenericClass), nameof(NonGenericClass.Method0), 0)]
        [InlineData(typeof(NonGenericClass), nameof(NonGenericClass.Method1), 1)]
        [InlineData(typeof(NonGenericClass), nameof(NonGenericClass.Method2), 2)]
        [InlineData(typeof(NonGenericClass), nameof(NonGenericClass.Method3), 3)]
        [InlineData(typeof(NonGenericClass), nameof(NonGenericClass.Method4), 4)]
        [InlineData(typeof(NonGenericClass), nameof(NonGenericClass.Method5), 5)]
        public static void InvalidArgumentCount_ThrowsArgumentException(Type type, string name, int count)
        {
            MethodInfo method = type.GetMethod(name);
            Expression arg = Expression.Constant("abc");
            if (count != 0)
            {
                Assert.Throws<ArgumentException>("method", () => Expression.Call(method));
                Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method));
            }
            if (count != 1)
            {
                Assert.Throws<ArgumentException>("method", () => Expression.Call(method, arg));
                Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, arg));
            }
            if (count != 2)
            {
                Assert.Throws<ArgumentException>("method", () => Expression.Call(method, arg, arg));
                Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, arg, arg));
            }
            if (count != 3)
            {
                Assert.Throws<ArgumentException>("method", () => Expression.Call(method, arg, arg, arg));
                Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, arg, arg, arg));
            }
            if (count != 4)
            {
                Assert.Throws<ArgumentException>("method", () => Expression.Call(method, arg, arg, arg, arg));
            }
            if (count != 5)
            {
                Assert.Throws<ArgumentException>("method", () => Expression.Call(method, arg, arg, arg, arg, arg));
            }
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, Enumerable.Repeat(arg, count + 1).ToArray()));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(method, Enumerable.Repeat(arg, count + 1)));

            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, Enumerable.Repeat(arg, count + 1).ToArray()));
            Assert.Throws<ArgumentException>("method", () => Expression.Call(null, method, Enumerable.Repeat(arg, count + 1)));
        }

        [Fact]
        public static void MethodName_NullInstance_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("instance", () => Expression.Call((Expression)null, "methodName", new Type[0], new Expression[0]));
        }

        [Fact]
        public static void MethodName_NullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("type", () => Expression.Call((Type)null, "methodName", new Type[0], new Expression[0]));
        }

        [Fact]
        public static void NullMethodName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("methodName", () => Expression.Call(Expression.Constant(new NonGenericClass()), null, new Type[0], new Expression[0]));
            Assert.Throws<ArgumentNullException>("methodName", () => Expression.Call(typeof(NonGenericClass), null, new Type[0], new Expression[0]));
        }

        [Fact]
        public static void MethodName_DoesNotExist_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Expression.Call(Expression.Constant(new NonGenericClass()), "NoSuchMethod", null));
            Assert.Throws<InvalidOperationException>(() => Expression.Call(typeof(NonGenericClass), "NoSuchMethod", null));
        }

        public static IEnumerable<object[]> InvalidTypeArgs_TestData()
        {
            yield return new object[] { null };
            yield return new object[] { new Type[0] };
            yield return new object[] { new Type[2] };
        }

        [Theory]
        [MemberData(nameof(InvalidTypeArgs_TestData))]
        public static void MethodName_NoSuchGenericMethodWithTypeArgs_ThrowsInvalidOperationException(Type[] typeArgs)
        {
            Assert.Throws<InvalidOperationException>(() => Expression.Call(Expression.Constant(new NonGenericClass()), nameof(NonGenericClass.GenericInstanceMethod), typeArgs));
            Assert.Throws<InvalidOperationException>(() => Expression.Call(typeof(NonGenericClass), nameof(NonGenericClass.GenericStaticMethod), typeArgs));
        }

        [Fact]
        public static void MethodName_TypeArgsDontMatchConstraints_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(null, () => Expression.Call(Expression.Constant(new NonGenericClass()), nameof(NonGenericClass.ConstrainedInstanceMethod), new Type[] { typeof(object) }));
            Assert.Throws<ArgumentException>(null, () => Expression.Call(typeof(NonGenericClass), nameof(NonGenericClass.ConstrainedStaticMethod), new Type[] { typeof(object) }));
        }

        [Fact]
        public static void MethodName_NonGenericMethodHasTypeArgs_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Expression.Call(Expression.Constant(new NonGenericClass()), nameof(NonGenericClass.InstanceMethod), new Type[1]));
            Assert.Throws<InvalidOperationException>(() => Expression.Call(typeof(NonGenericClass), nameof(NonGenericClass.StaticMethod), new Type[1]));
        }

        [Fact]
        public static void MethodName_TypeArgsHasNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(null, () => Expression.Call(Expression.Constant(new NonGenericClass()), nameof(NonGenericClass.GenericInstanceMethod), new Type[] { null }));
            Assert.Throws<ArgumentNullException>(null, () => Expression.Call(typeof(NonGenericClass), nameof(NonGenericClass.GenericStaticMethod), new Type[] { null }));
        }

        [Fact]
        public static void MethodName_ArgumentsHasNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("arguments", () => Expression.Call(Expression.Constant(new NonGenericClass()), nameof(NonGenericClass.InstanceMethod1), new Type[0], new Expression[] { null }));
            Assert.Throws<ArgumentNullException>("arguments", () => Expression.Call(typeof(NonGenericClass), nameof(NonGenericClass.StaticMethod1), new Type[0], new Expression[] { null }));
        }

        [Fact]
        public static void MethodName_ArgumentsHasNullValueButDifferentCount_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => Expression.Call(Expression.Constant(new NonGenericClass()), nameof(NonGenericClass.InstanceMethod1), new Type[0], new Expression[] { null, Expression.Constant("") }));
            Assert.Throws<InvalidOperationException>(() => Expression.Call(typeof(NonGenericClass), nameof(NonGenericClass.StaticMethod1), new Type[0], new Expression[] { null, Expression.Constant("") }));
        }

        [Fact]
        public static void ToStringTest()
        {
            // NB: Static methods are inconsistent compared to static members; the declaring type is not included

            var e1 = Expression.Call(null, typeof(SomeMethods).GetMethod(nameof(SomeMethods.S0), BindingFlags.Static | BindingFlags.Public));
            Assert.Equal("S0()", e1.ToString());

            var e2 = Expression.Call(null, typeof(SomeMethods).GetMethod(nameof(SomeMethods.S1), BindingFlags.Static | BindingFlags.Public), Expression.Parameter(typeof(int), "x"));
            Assert.Equal("S1(x)", e2.ToString());

            var e3 = Expression.Call(null, typeof(SomeMethods).GetMethod(nameof(SomeMethods.S2), BindingFlags.Static | BindingFlags.Public), Expression.Parameter(typeof(int), "x"), Expression.Parameter(typeof(int), "y"));
            Assert.Equal("S2(x, y)", e3.ToString());

            var e4 = Expression.Call(Expression.Parameter(typeof(SomeMethods), "o"), typeof(SomeMethods).GetMethod(nameof(SomeMethods.I0), BindingFlags.Instance | BindingFlags.Public));
            Assert.Equal("o.I0()", e4.ToString());

            var e5 = Expression.Call(Expression.Parameter(typeof(SomeMethods), "o"), typeof(SomeMethods).GetMethod(nameof(SomeMethods.I1), BindingFlags.Instance | BindingFlags.Public), Expression.Parameter(typeof(int), "x"));
            Assert.Equal("o.I1(x)", e5.ToString());

            var e6 = Expression.Call(Expression.Parameter(typeof(SomeMethods), "o"), typeof(SomeMethods).GetMethod(nameof(SomeMethods.I2), BindingFlags.Instance | BindingFlags.Public), Expression.Parameter(typeof(int), "x"), Expression.Parameter(typeof(int), "y"));
            Assert.Equal("o.I2(x, y)", e6.ToString());

            var e7 = Expression.Call(null, typeof(ExtensionMethods).GetMethod(nameof(ExtensionMethods.E0), BindingFlags.Static | BindingFlags.Public), Expression.Parameter(typeof(int), "x"));
            Assert.Equal("x.E0()", e7.ToString());

            var e8 = Expression.Call(null, typeof(ExtensionMethods).GetMethod(nameof(ExtensionMethods.E1), BindingFlags.Static | BindingFlags.Public), Expression.Parameter(typeof(int), "x"), Expression.Parameter(typeof(int), "y"));
            Assert.Equal("x.E1(y)", e8.ToString());

            var e9 = Expression.Call(null, typeof(ExtensionMethods).GetMethod(nameof(ExtensionMethods.E2), BindingFlags.Static | BindingFlags.Public), Expression.Parameter(typeof(int), "x"), Expression.Parameter(typeof(int), "y"), Expression.Parameter(typeof(int), "z"));
            Assert.Equal("x.E2(y, z)", e9.ToString());
        }

        public class GenericClass<T>
        {
            public static void NonGenericMethod() { }
        }

        public static class Unreadable<T>
        {
            public static T WriteOnly { set { } }
        }

        public class NonGenericClass
        {
            public static void GenericMethod<T>() { }
            public void InstanceMethod() { }
            public static void StaticMethod() { }

            public static void Method0() { }
            public static void Method1(int i1) { }
            public static void Method2(int i1, int i2) { }
            public static void Method3(int i1, int i2, int i3) { }
            public static void Method4(int i1, int i2, int i3, int i4) { }
            public static void Method5(int i1, int i2, int i3, int i4, int i5) { }

            public void staticSameName(uint i1) { }
            public void instanceSameName(int i1) { }

            public static void StaticSameName(uint i1) { }
            public static void staticSameName(int i1) { }

            public void GenericInstanceMethod<T>(T t1) { }
            public static void GenericStaticMethod<T>(T t1) { }

            public void ConstrainedInstanceMethod<T>(T t1) where T : struct { }
            public static void ConstrainedStaticMethod<T>(T t1) where T : struct { }

            public void InstanceMethod1(int i1) { }
            public static void StaticMethod1(int i1) { }
        }
    }

    class SomeMethods
    {
        public static void S0() {}
        public static void S1(int x) {}
        public static void S2(int x, int y) {}

        public void I0() {}
        public void I1(int x) {}
        public void I2(int x, int y) {}
    }
    
    static class ExtensionMethods
    {
         public static void E0(this int x) {}
         public static void E1(this int x, int y) {}
         public static void E2(this int x, int y, int z) {}
    }
}
