﻿using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;
using NP.Ava.UniDockService;
using NP.IoCy;
using NP.DependencyInjection.Interfaces;

namespace NP.GroupCompassSample
{
    public static class MyContainer
    {
        public static IDependencyInjectionContainer<object> TheContainer { get; }

        public static DockManager TheDockManager { get; } = new DockManager();

        static MyContainer()
        {
            var containerBuilder = new ContainerBuilder();

            TheDockManager.IsInEditableState = true;

            containerBuilder.RegisterType<IFloatingWindowFactory, MyCustomFloatingWindowFactory>();

            containerBuilder.RegisterSingletonInstance<IUniDockService>(TheDockManager);

            TheContainer = containerBuilder.Build();
        }
    }
}
