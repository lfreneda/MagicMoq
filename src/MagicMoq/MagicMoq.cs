﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using System.Reflection;

namespace MagicMoq
{
    public class Moqer
    {
        private readonly Dictionary<Type, Func<object>> providers = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, Mock> mocks = new Dictionary<Type, Mock>();

        public void Bind<TKey, TImpl>() where TImpl : TKey
        {
            providers.Add(typeof(TKey), () => Resolve<TImpl>());
        }

        public T Resolve<T>()
        {
            Func<object> provider;

            return providers.TryGetValue(typeof(T), out provider) ? (T)provider() : ResolveByGreediestConstructor<T>();
        }

        private object ResolveByGreediestConstructor(Type type)
        {
            var constructor = GetGreediestConstructor(type);
            ParameterInfo[] parameters = null;

            if (null != constructor)
                parameters = constructor.GetParameters();

            if (type.IsInterface == false && (parameters == null || parameters.Length == 0))
                return Activator.CreateInstance(type);
            else if (type.IsInterface)
            {
                return ResolveInternal(type);
            }
            else
            {
                var resolvedParameters = from p in parameters
                                         select ResolveInternal(p.ParameterType);

                return constructor.Invoke(resolvedParameters.ToArray());
            }
        }

        private T ResolveByGreediestConstructor<T>()
        {
            var type = typeof(T);
            return (T)ResolveByGreediestConstructor(type);
        }

        private ConstructorInfo GetGreediestConstructor(Type type)
        {
            return type.GetConstructors().OrderBy(a => a.GetParameters().Length).FirstOrDefault();
        }

        private object ResolveInternal(Type parameterType)
        {
            Func<object> provider;

            if (providers.TryGetValue(parameterType, out provider))
                return provider();
            else
            {
                var constructor = GetGreediestConstructor(parameterType);

                if (constructor == null || constructor.GetParameters().Length == 0)
                {
                    var mock = CreateOrResolveAMock(parameterType);

                    return mock.Object; 
                }
                else
                {
                    return ResolveByGreediestConstructor(parameterType);
                }
            }
        }

        private Mock CreateOrResolveAMock(Type type)
        {
            Mock mock;
            if (mocks.TryGetValue(type, out mock))
                return mock;
            else
            {
                var genericMock = typeof(Mock<>).MakeGenericType(type);

                mock = (Mock)Activator.CreateInstance(genericMock);
                mocks.Add(type, mock);
                return mock;
            }
        }

        public void Bind<T>(T instance)
        {
            Bind(instance, instance.GetType());
        }

        public void Bind(object instance, Type type)
        {
            providers[type] = () => instance;
        }

        public void SetInstance(object instance)
        {
            Bind(instance);
        }

        public Mock<T> GetMock<T>()
            where T : class
        {
            return (Mock<T>)CreateOrResolveAMock(typeof(T));
        }

        #region MoqAPI

        public ISetup<T> Setup<T>(Expression<Action<T>> expression) where T : class
        {
            return GetMock<T>().Setup(expression);
        }

        public ISetup<T, TResult> Setup<T, TResult>(Expression<Func<T, TResult>> expression) where T : class
        {
            return GetMock<T>().Setup(expression);
        }

        public void Verify<T>(Expression<Action<T>> expression) where T : class
        {
            GetMock<T>().Verify(expression);
        }

        public void Verify<T>(Expression<Action<T>> expression, string failMessage) where T : class
        {
            GetMock<T>().Verify(expression, failMessage);
        }

        public void Verify<T>(Expression<Action<T>> expression, Times times) where T : class
        {
            GetMock<T>().Verify(expression, times);
        }

        public void Verify<T>(Expression<Action<T>> expression, Times times, string failMessage) where T : class
        {
            GetMock<T>().Verify(expression, times, failMessage);
        }

        #endregion
    }

    public static class ISetupExtensions
    {
        public static ISetup<T, TResult> AndResolveWith<T, TResult>(this ISetup<T, TResult> setup, Moqer magic)
            where T : class
        {
            setup.Returns(magic.Resolve<TResult>());

            return setup;
        }
    }
}
