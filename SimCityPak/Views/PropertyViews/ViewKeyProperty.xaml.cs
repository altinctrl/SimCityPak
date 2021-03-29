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
using Gibbed.Spore.Properties;
using Gibbed.Spore.Package;
using SimCityPak.Views;
using SimCityPak.PackageReader;

namespace SimCityPak
{
    /// <summary>
    /// Interaction logic for ViewKeyProperty.xaml
    /// </summary>
    public partial class ViewKeyProperty : UserControl
    {
        public ViewKeyProperty()
        {
            InitializeComponent();
        }
        public List<DatabaseIndex>indexes { get; set; }
        MainWindow getMainWindow()
        {
            Window owner = Window.GetWindow(this);
            MainWindow main = null;
            if (owner is MainWindow)
            {
                main = owner as MainWindow;
            }
            else if (owner is ViewWindow)
            {
                main = ((ViewWindow)owner).Main;
            }
            return main;
        }
        private void openKey_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is KeyProperty)
            {

                MainWindow main = getMainWindow();
                if (main != null) 
                {
                    

                    if (indexes.Count == 1)
                    {
                        ViewWindow window = new ViewWindow(main, indexes[0]);
                        window.Show();
                    }
                    else if (indexes.Count == 0)
                    {
                        MessageBox.Show("This value is pointing to a instance in a package that is currently not loaded.", "No results");
                    }
                    else 
                    {
                        ViewMiniResult miniResult = new ViewMiniResult(main, indexes);
                        miniResult.Show();
                    }
                }

            }
        }

        private void mnuSearch_Click(object sender, RoutedEventArgs e)
        {
            Window owner = Window.GetWindow(this);
            MainWindow main = null;
            if (owner is MainWindow)
            {
                main = owner as MainWindow;
            }
            else if (owner is ViewWindow)
            {
                main = ((ViewWindow)owner).Main;
            }
            KeyProperty prop = this.DataContext as KeyProperty;
            SimCityPak.Views.ViewSearchProperties res = new SimCityPak.Views.ViewSearchProperties(main, prop.InstanceId.ToHex(), null, true);
            res.Show();

        }

        private void mnuEdit_Click(object sender, RoutedEventArgs e)
        {
         
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is KeyProperty)
            {
                KeyProperty prop = this.DataContext as KeyProperty;

                ViewEditKeyProperty editWindow = new ViewEditKeyProperty(prop);
                editWindow.ShowDialog();
                prop = editWindow.EditProperty;

                prop.OnPropertyChanged(new EventArgs());

                //update all of the textboxes
                var expression = txtInstanceName.GetBindingExpression(Run.TextProperty);
                if (expression != null) expression.UpdateTarget();
                expression = txtGroup.GetBindingExpression(Run.TextProperty);
                if (expression != null) expression.UpdateTarget();
                expression = txtType.GetBindingExpression(Run.TextProperty);
                if (expression != null) expression.UpdateTarget();
                expression = txtInstanceId.GetBindingExpression(Run.TextProperty);
                if (expression != null) expression.UpdateTarget();
            }
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is KeyProperty)
            {
                KeyProperty prop = this.DataContext as KeyProperty;
                indexes = DatabaseManager.Instance.Indices.FindAll(i => i.InstanceId == prop.InstanceId &&
                                                (prop.TypeId == 0 || i.TypeId == prop.TypeId) &&
                                                (prop.GroupContainer == 0 || i.GroupContainer == prop.GroupContainer));
                resultsCount.Text = String.Format("({0})", indexes.Count);
            }
        }
    }
}
