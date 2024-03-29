﻿using System.Windows;

namespace TemperatureReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();      

            var vm = DataContext as MainVM;
            Closing += vm.OnWindowClosing;
        }       
    }
}
