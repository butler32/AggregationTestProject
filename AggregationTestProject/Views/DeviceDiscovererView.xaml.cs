﻿using AggregationTestProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unity;

namespace AggregationTestProject.Views
{
    /// <summary>
    /// Interaction logic for DeviceDiscovererView.xaml
    /// </summary>
    public partial class DeviceDiscovererView : UserControl
    {
        public DeviceDiscovererView()
        {
            InitializeComponent();

            DataContext = Program.Container.Resolve<DeviceDiscovererViewModel>();
        }
    }
}
