using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using NP.Ava.UniDock;
using NP.Ava.Visuals;
using System;
using System.Linq;

namespace NP.DockItemsWidthSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Closed += MainWindow_Closed;

            DoSmthButton.Click += DoSmthButton_Click;
        }

        private void DoSmthButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
