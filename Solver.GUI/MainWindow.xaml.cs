using Innouvous.Utils;
using Solver.Engine;
using Solver.Engine.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Solver.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Quick and simple GUI for the API (so did not use MVVM... WIP)
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainWindowViewModel(InputGrid);
            this.DataContext = vm;
        }
        
     
        
    }
}
