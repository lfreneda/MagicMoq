#MagicMoq
========

##Mocking dependencies with style 8-)

MagicMoq is an extension to the Moq mocking framework – It is like an IoC container that injects mocked dependencies when resolving a type, making the job of creating testing subjects much easier (and nicer).

It is so easy to use as the Moq framework. Check it out:


###Assuming that you want to test this class:
```C#

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
```

###Without MagicMoq you would probably do something like this:
```C#
[Test]
public void CrappyTest()
{
    var fooMock = new Moq.Mock<IFooDependency>();
    var anotherFooMock = new Moq.Mock<IFooDependency>();
    var foo = new Foo(fooMock.Object, anotherFooMock.Object);
    
    foo.DoSomething();
    
    fooMock.Verify(f => f.DoSomethingDifferent(), Moq.Times.Once());
}

```

###But with MagicMoq you can get rid of the mocked dependencies, dealing with just what you want to test:
```C#
[Test]
public void NiceTest()
{
    var magic = new MagicMoq();
    var foo = magic.Resolve<Foo>();
    
    foo.DoSomething();
    
    foo.Verify(f => f.DoSomethingDifferent(), Moq.Times.Once());
    //you can even use the real Moq.Mock<> class using the GetMock method, like this:
    //foo.GetMock<IFooDependency>().Verify(f => f.DoSomethingDifferent(), Moq.Times.Once());
    //but I find the first approach much clean :-)
}
```

###And you can, obviously, create setups with the MagicMoq API:
```C#
[Test]
public void EvenMoreNiceTest()
{
    var magic = new MagicMoq();
    var foo = magic.Resolve<Foo>();
    
    magic.Setup<IFooDependency, int>(f => f.DoSomethingDifferent()).Returns(42);
    
    int result = foo.DoSomething();
    
    result.Should().Be(42);
}
```

###Another cool feature is the ability to "redirect" calls to a method of a dependency to the MagicMoq itself, making the method return another mocked dependency from the container:
```C#
        [Test]
        public void MagicTest()
        {
            var magic = new MagicMoq();

            magic.Setup<ISessionFactory, ISession>(a => a.OpenSession()).AndMagicallyResolve(magic);
            magic.Setup<ISession, ITransaction>(a => a.OpenTransaction()).AndMagicallyResolve(magic);

            var someoneThatUseSessionFactory = magic.Resolve<SomeoneThatUseISessionFactory>();

            someoneThatUseSessionFactory.DoSomething();

            magic.Verify<ISessionFactory>(a => a.OpenSession(), Times.Once());
            magic.Verify<ISession>(a => a.OpenTransaction(), Times.Once());
            magic.Verify<ISession>(a => a.DoSomething(), Times.Once());
            magic.Verify<ITransaction>(a => a.Commit(), Times.Once());
        }
        
        public interface ISessionFactory
        {
            ISession OpenSession();
        }

        public interface ISession
        {
            void DoSomething();
            ITransaction OpenTransaction();
        }

        public interface ITransaction
        {
            void Commit();
        }

        public class SomeoneThatUseISessionFactory
        {
            private ISessionFactory sessionFactory;

            public SomeoneThatUseISessionFactory(ISessionFactory sessionFactory)
            {
                this.sessionFactory = sessionFactory;
            }

            public void DoSomething()
            {
                var session = sessionFactory.OpenSession();
                var transaction = session.OpenTransaction();

                session.DoSomething();

                transaction.Commit();
            }
        }
```


