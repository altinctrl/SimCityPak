using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimCityPak.Views.AdvancedEditors
{
    /// <summary>
    /// Interaction logic for ViewPathEntry.xaml
    /// </summary>
    public partial class ViewPathEntry : UserControl
    {
        public ViewPathEntry()
        {
            InitializeComponent();
        }

        public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent("Delete", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewPathEntry));
        public static readonly RoutedEvent CopyEvent = EventManager.RegisterRoutedEvent("Copy", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ViewPathEntry));

        public event RoutedEventHandler OnDelete
        {
            add { AddHandler(DeleteEvent, value); }
            remove { RemoveHandler(DeleteEvent, value); }
        }

        public event RoutedEventHandler OnCopy
        {
            add { AddHandler(CopyEvent, value); }
            remove { RemoveHandler(CopyEvent, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(DeleteEvent);
            RaiseEvent(newEventArgs);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(CopyEvent);
            RaiseEvent(newEventArgs);
        }


    }
}
