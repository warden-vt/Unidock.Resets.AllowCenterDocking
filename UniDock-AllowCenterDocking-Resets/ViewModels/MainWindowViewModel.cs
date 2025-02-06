using NP.Ava.UniDock;
using NP.Ava.UniDock.Factories;
using NP.IoCy;
using UniDockAllowCenterDockingResets;

namespace UniDock_AllowCenterDocking_Resets.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting { get; } = "Welcome to Avalonia!";

        public void CreateFloatingWindowsCommand()
        {
            var floatingWindowFactory = DockContainer.Container.Resolve<IFloatingWindowFactory>() as CustomFloatingWindowFactory;
            floatingWindowFactory?.CreateFloatingWindow(640, 480);

            
            
        }
    }
}
