﻿// (c) Nick Polyak 2018 - http://awebpros.com/
// License: Apache License 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using FluentAssertions;
using NP.IoCy;
using TestAllRegisterAndResolveMethods.Implementations;
using TestAllRegisterAndResolveMethods.Interfaces;
using System;
using NP.DependencyInjection.Interfaces;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace TestAllRegisterAndResolveMethods
{
    public static class Program
    {
        public static bool IsSingleton<T>
        (
            this IDependencyInjectionContainer<string?> container,
            string? key = null
        )
        {
            T obj1 = container.Resolve<T>(key);

            obj1.Should().NotBeNull();

            T obj2 = container.Resolve<T>(key);
            obj2.Should().NotBeNull();

            return object.ReferenceEquals(obj1, obj2);
        }

        public static IOrg CreateOrg()
        {
            IOrg org = new Org();

            org.Manager = new Person();

            org.OrgName = "Other Department Store";
            org.Manager.PersonName = "Joe Doe";

            org.Manager.Address = new Address();
            org.Manager.Address.City = "Boston";
            org.Manager.Address.ZipCode = "12345";

            org.Log = new ConsoleLog();

            return org;
        }


        public static void TestOrg
        (
            this IDependencyInjectionContainer<string?> container, 
            bool isSingleton, 
            string? key = null)
        {
            container.IsSingleton<IOrg>(key).Should().Be(isSingleton);
            IOrg org = container.Resolve<IOrg>(key);
            org.OrgName.Should().Be("Other Department Store");

        }

        public static void Main(string[] args)
        {
            // create container builder
            IContainerBuilderWithMultiCells<string?> containerBuilder = new ContainerBuilder<string?>(true);

            #region BOOTSTRAPPING
            // bootstrap container 
            // (map the types)
            containerBuilder.RegisterType<IPerson, Person>();
            containerBuilder.RegisterType<IAddress, Address>();
            containerBuilder.RegisterType<IOrg, Org>();

            containerBuilder.RegisterSingletonType<ILog, FileLog>();

            containerBuilder.RegisterType<ILog, ConsoleLog>("MyLog");
            #endregion BOOTSTRAPPING

            // Create container
            IDependencyInjectionContainer<string?> container = containerBuilder.Build();

            // resolve and compose organization
            // all its 'Parts' will be added at
            // this stage. 
            IOrg org = container.Resolve<IOrg>();

            #region Set Org Data

            org.ProjLead.Should().BeNull();

            org.Manager.Should().NotBeNull();

            IPerson person = container.Resolve<IPerson>();

            person.Should().NotBeSameAs(org.Manager);

            ILog log = container.Resolve<ILog>();

            log.Should().BeSameAs(org.Log);

            org.Log2.Should().NotBeNull();
            org.Log2.Should().BeOfType<ConsoleLog>();

            ILog log2 = container.Resolve<ILog>("MyLog");

            log2.Should().NotBeNull();

            log2.Should().NotBeSameAs(org.Log2);
            log2.Should().BeOfType<ConsoleLog>();

            org.OrgName = "Nicks Department Store";
            org.Manager.PersonName = "Nick Polyak";
            org.Manager.Address.City = "Miami";
            org.Manager.Address.ZipCode = "12345";

            #endregion Set Org Data

            // Create file MyLogFile.txt in the same folder as the executable
            // and write department store info in it;
            org.LogOrgInfo();

            ConsoleLog consoleLog = new ConsoleLog();
            containerBuilder.RegisterSingletonInstance<ILog>(consoleLog);

            // replace registration of ILog to ConsoleLog (instead of FileLog) in another container. 
            var anotherContainer = containerBuilder.Build();

            // resolve org from another Container.
            IOrg orgWithConsoleLog = anotherContainer.Resolve<IOrg>();

            orgWithConsoleLog.Log.Should().NotBeNull();
            orgWithConsoleLog.Log.Should().BeSameAs(consoleLog);

            #region Set Child Org Data

            orgWithConsoleLog.OrgName = "Nicks Department Store";
            orgWithConsoleLog.Manager.PersonName = "Nick Polyak";
            orgWithConsoleLog.Manager.Address.City = "Miami";
            orgWithConsoleLog.Manager.Address.ZipCode = "12345";

            #endregion Set Child Org Data

            // send org data to console instead of a file.
            orgWithConsoleLog.LogOrgInfo();

            containerBuilder.RegisterFactoryMethod(CreateOrg);
            var container3 = containerBuilder.Build();
            container3.TestOrg(false);

            // test that double keys (even with different ResolvingTypes) are prevented
            //containerBuilder.RegisterFactoryMethod(CreateOrg, "MyLog");

            containerBuilder.RegisterFactoryMethod(CreateOrg, "TheOrg");
            container3 = containerBuilder.Build();
            container3.TestOrg(false, "TheOrg");

            containerBuilder.RegisterSingletonFactoryMethod(CreateOrg);
            container3 = containerBuilder.Build();
            container3.TestOrg(true);

            containerBuilder.RegisterSingletonFactoryMethod(CreateOrg, "TheOrg");
            container3 = containerBuilder.Build();
            container3.TestOrg(true, "TheOrg");


            containerBuilder.UnRegister(typeof(IOrg));
            containerBuilder.UnRegister(typeof(IOrg), "TheOrg");
            container3 = containerBuilder.Build();

            org = container3.Resolve<IOrg>();
            org.Should().BeNull();
            org = container3.Resolve<IOrg>("TheOrg");
            org.Should().BeNull();

            MethodInfo createOrgMethodInfo =
                typeof(Program).GetMethod(nameof(CreateOrg));

            containerBuilder.RegisterSingletonFactoryMethodInfo<IOrg>(createOrgMethodInfo, "TheOrg");
            container3 = containerBuilder.Build();
            container3.TestOrg(true, "TheOrg");

            containerBuilder.RegisterFactoryMethodInfo(createOrgMethodInfo, null, "TheOrg");
            container3 = containerBuilder.Build();
            container3.TestOrg(false, "TheOrg");

            var containerBuilder4 = new ContainerBuilder<string?>(true);

            containerBuilder4.RegisterAttributedClass(typeof(AnotherOrg));
            containerBuilder4.RegisterAttributedClass(typeof(AnotherPerson));
            containerBuilder4.RegisterAttributedClass(typeof(ConsoleLog));
            containerBuilder4.RegisterType<IAddress, Address>("TheAddress");

            var container4 = containerBuilder4.Build();

            IOrgGettersOnly orgGettersOnly =
                container4.Resolve<IOrgGettersOnly>("TheOrg");

            IOrgGettersOnly orgGettersOnly1 =
                container4.Resolve<IOrgGettersOnly>("TheOrg");

            object.ReferenceEquals(orgGettersOnly, orgGettersOnly1).Should().NotBe(true);

            object.ReferenceEquals(orgGettersOnly1.Manager, orgGettersOnly1.Manager).Should().Be(true);

            container4.IsSingleton<IOrgGettersOnly>("TheOrg").Should().NotBe(true);


            orgGettersOnly.Manager.Address.Should().NotBeNull();

            // make sure ILog is a singleton.
            container4.IsSingleton<ILog>().Should().BeTrue();

            var containerBuilder5 = new ContainerBuilder<string>(true);

            containerBuilder5.RegisterAttributedStaticFactoryMethodsFromClass(typeof(FactoryMethods)); 

            var container5 = containerBuilder5.Build();

            IOrg org5 = container5.Resolve<IOrg>("TheOrg");

            org5.OrgName.Should().Be("Other Department Store");
            org5.Manager.PersonName.Should().Be("Joe Doe");
            org5.Manager.Address.City.Should().Be("Providence");

            IOrg anotherOrg5 = container5.Resolve<IOrg>("TheOrg");

            org5.Should().NotBeSameAs(anotherOrg5);
            org5.Manager.Should().BeSameAs(anotherOrg5.Manager);

            IAddress address5 = container5.Resolve<IAddress>("TheAddress");

            address5.Should().NotBeSameAs(org5.Manager.Address);

            // test multi cells (IoCy only)
            var containerBuilder6 = new ContainerBuilder<string>(true);

            containerBuilder6.RegisterMultiCellObjInstance(typeof(string), "Str1", "MyStrings");
            containerBuilder6.RegisterMultiCellObjInstance(typeof(string), new[] { "Str2", "Str3" }, "MyStrings");

            var container6 = containerBuilder6.Build();

            IEnumerable<string> myStrings = container6.Resolve<IEnumerable<string>>("MyStrings");

            myStrings.Should().BeEquivalentTo(new[] { "Str1", "Str2", "Str3" });

            var containerBuilder7 = new ContainerBuilder<string>(true);

            containerBuilder7.RegisterMultiCellType<ILog, ConsoleLog>("MyLogs");
            containerBuilder7.RegisterMultiCellType<ILog, FileLog>("MyLogs");

            var container7 = containerBuilder7.Build();

            IEnumerable<ILog> logs = container7.Resolve<IEnumerable<ILog>>("MyLogs");

            logs.Count().Should().Be(2);
            logs.First().Should().BeOfType<ConsoleLog>();
            logs.Skip(1).First().Should().BeOfType<FileLog>();

            var containerBuilder8 = new ContainerBuilder<string>(true);

            containerBuilder8.RegisterAttributedClass(typeof(MyOrg));
            containerBuilder8.RegisterAttributedStaticFactoryMethodsFromClass(typeof(OrgFactory));
            containerBuilder8.RegisterType<OrgsContainer, OrgsContainer>();

            var container8 = containerBuilder8.Build();

            var orgsContainer = container8.Resolve<OrgsContainer>();

            var orgNames = orgsContainer.Orgs.Select(org => org.OrgName).ToList();

            orgNames.Should().BeEquivalentTo(new[] { "MyOrg1", "MyOrg2", "MyOrg3", "MyOrg4" });

            Console.WriteLine("THE END");

            Console.ReadKey();
        }
    }
}
