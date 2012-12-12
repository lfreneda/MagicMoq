﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using FluentAssertions;

namespace MagicMoqTests
{
    [TestFixture]
    public class MagicMoqTests
    {
        [Test]
        public void ShouldCreateClassWithDependenciesAndVerify()
        {
            var magic = new MagicMoq.MagicMoq();
            var foo = magic.Resolve<Foo>();

            foo.DoSomething();

            magic.GetMock<IFooDependency>().Verify(a => a.DoSomethingDifferent(), Times.Once());
        }

        [Test]
        public void ShouldCreateClassWithDependenciesAndSetup()
        {
            var magic = new MagicMoq.MagicMoq();
            var foo = magic.Resolve<Foo>();

            magic.GetMock<IFooDependency>().Setup(a => a.DoSomethingDifferent()).Returns(2);

            foo.DoSomething().Should().Be(2);
        }

        [Test]
        public void ShouldCreateClassWithDependenciesAndVerify_WithMoqAPI()
        {
            var magic = new MagicMoq.MagicMoq();
            var foo = magic.Resolve<Foo>();

            foo.DoSomething();

            magic.Verify<IFooDependency>(a => a.DoSomethingDifferent(), Times.Once());
        }

        [Test]
        public void ShouldCreateClassWithDependenciesAndSetup_WithMoqAPI()
        {
            var magic = new MagicMoq.MagicMoq();
            var foo = magic.Resolve<Foo>();

            magic.Setup<IFooDependency, int>(a => a.DoSomethingDifferent()).Returns(2);

            foo.DoSomething().Should().Be(2);
        }

        [Test]
        public void ShouldCreateClassWithConcreteDependency()
        {
            var magic = new MagicMoq.MagicMoq();
            magic.SetInstance(new ConcreteDependency(11));

            var classWithConcreteDependency = magic.Resolve<ClassWithConcreteDependency>();

            classWithConcreteDependency.DifferentStuff().Should().Be(11);
        }

        #region Classes and dependencies

        public class Foo
        {
            private readonly IFooDependency dependency;

            public Foo(IFooDependency dependency, IAnotherFooDependency anotherDependency)
            {
                if (null == dependency) throw new ArgumentNullException();
                if (null == anotherDependency) throw new ArgumentNullException();

                this.dependency = dependency;
            }

            public int DoSomething()
            {
                return this.dependency.DoSomethingDifferent();
            }
        }

        public interface IFooDependency { int DoSomethingDifferent(); }
        public class FooDependency : IFooDependency
        {
            public int DoSomethingDifferent()
            {
                return -1;
            }
        }

        public interface IAnotherFooDependency { void Blofs();}

        public class ClassWithConcreteDependency
        {
            private ConcreteDependency dependency;
            public ClassWithConcreteDependency(ConcreteDependency dependency)
            {
                this.dependency = dependency;
            }

            public int DifferentStuff()
            {
                return dependency.MoreDifferentThings();
            }
        }

        public class ConcreteDependency
        {
            private int someInt;

            public ConcreteDependency(int? someInt)
            {
                this.someInt = someInt ?? -100;
            }
            public int MoreDifferentThings()
            {
                return someInt;
            }
        }

        #endregion
    }
}
