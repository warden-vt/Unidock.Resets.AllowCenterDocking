using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;
using NP.Ava.UniDockService;
using NP.Ava.Visuals.Behaviors;
using NP.DependencyInjection.Interfaces;
using NP.IoCy;

namespace UniDockAllowCenterDockingResets
{
    public static class DockContainer
    {
        public static IDependencyInjectionContainer<object?> Container { get; }

        public static DockManager DockManager { get; } = new DockManager();

        static DockContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<IStackGroupFactory, StackGroupFactory>();
            containerBuilder.RegisterType<IFloatingWindowFactory, CustomFloatingWindowFactory>();
            containerBuilder.RegisterSingletonInstance<DockManager>(DockManager);
            containerBuilder.RegisterSingletonInstance<IUniDockService>(DockManager);

            Container = containerBuilder.Build();
        }
    }

    public class CustomFloatingWindowFactory : IFloatingWindowFactory
    {
        public FloatingWindow CreateFloatingWindow()
        {
            FloatingWindow dockWindow = new();

            dockWindow.Classes.AddRange(["PlainFloatingWindow"/*, "MyFloatingWindow"*/]);
            dockWindow.TitleClasses = "WindowTitle";
            dockWindow.IsDockWindow = true;

            return dockWindow;
        }

        public void CreateFloatingWindow(double width, double height)
        {
            FloatingWindow dockWindow = CreateFloatingWindow();
            dockWindow.Classes.AddRange(["PlainFloatingWindow"/*, "MyFloatingWindow"*/]);
            dockWindow.TitleClasses = "WindowTitle";
            dockWindow.Content = dockWindow.TheDockGroup;
            DockAttachedProperties.SetAllowCenterDocking(dockWindow, false);

            Window? parentWindow = (DockContainer.DockManager.AllGroups.First() as Control)?.GetVisualAncestors().OfType<Window>().First(); //MainWindow

            if (parentWindow != null)
            {
                dockWindow.ProducingUserDefinedWindowGroup = parentWindow.FindControl<RootDockGroup>("MainWindow_RootGroup");  //Main Window Root Group

                DockAttachedProperties.SetTheDockManager(dockWindow, DockContainer.DockManager);
                DockAttachedProperties.SetDockChildWindowOwner(dockWindow, parentWindow);

                dockWindow.Width = width;
                dockWindow.Height = height;
            }

            DockItem dockItem = new() { Content = new TextBlock() { Text="Hello", Foreground = Brushes.Black}, DefaultDockGroupId = "MainWindow_RootGroup" };

            dockWindow.TheDockGroup.DockChildren.Add(dockItem);
            
            dockWindow.ShowDockWindow();
        }
    }
}
