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
using System.Windows.Shapes;

namespace SimCityPak.Views.AdvancedEditors.DecalDictionary
{
    /// <summary>
    /// Interaction logic for EditDecalDictionaryEntry.xaml
    /// </summary>
    public partial class ViewEditDecalDictionaryEntry : Window
    {
        
        public ViewEditDecalDictionaryEntry(DecalImageModel decal)
        {
            this.DataContext = decal;
            InitializeComponent();
            RefreshImage();
        }

        public DecalImageModel DecalImage
        {
            get
            {
                return this.DataContext as DecalImageModel;
            }
        }


        public void RefreshImage()
        {
            DecalImageModel decal = this.DataContext as DecalImageModel;
            decal.RefreshPreview();
            imgPreview.Source = decal.ImageSource;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
            DecalImageModel decal = this.DataContext as DecalImageModel;
            dlg.InitialColor = decal.Color1;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                decal.Color1 = dlg.SelectedColor;
            }

            var expression = rectangle1.GetBindingExpression(Border.BackgroundProperty);
            if (expression != null) expression.UpdateTarget();

            RefreshImage();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
            DecalImageModel decal = this.DataContext as DecalImageModel;
            dlg.InitialColor = decal.Color2;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                decal.Color2 = dlg.SelectedColor;
            }

            var expression = rectangle2.GetBindingExpression(Border.BackgroundProperty);
            if (expression != null) expression.UpdateTarget();

            RefreshImage();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
            DecalImageModel decal = this.DataContext as DecalImageModel;
            dlg.InitialColor = decal.Color3;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                decal.Color3 = dlg.SelectedColor;
            }

            var expression = rectangle3.GetBindingExpression(Border.BackgroundProperty);
            if (expression != null) expression.UpdateTarget();

            RefreshImage();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ColorPickerControls.Dialogs.ColorPickerFullDialog dlg = new ColorPickerControls.Dialogs.ColorPickerFullDialog();
            DecalImageModel decal = this.DataContext as DecalImageModel;
            dlg.InitialColor = decal.Color4;
            if (dlg.ShowDialog().GetValueOrDefault(false))
            {
                decal.Color4 = dlg.SelectedColor;
            }

            var expression = rectangle4.GetBindingExpression(Border.BackgroundProperty);
            if (expression != null) expression.UpdateTarget();

            RefreshImage();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
