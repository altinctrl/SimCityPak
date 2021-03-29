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

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewTextFind.xaml
    /// </summary>
    public partial class ViewTextFind : Window
    {
        private TextBox parentTextBox; //Handle to TextBox
        private int currentPosition; //Current search position
        private int lastLine;
        private int lastCol;
        private bool done;

        public ViewTextFind(TextBox parentTextBox)
        {
            InitializeComponent();
            txtSearch.Focus();

            this.parentTextBox = parentTextBox;
            this.currentPosition = 0;
            this.done = false;

            parentTextBox.KeyDown +=new KeyEventHandler(parentTextBox_KeyDown);
        }

        private void parentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Gets keyboard shortcuts
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.F) //Opens find window
                {
                    Show();
                    Focus(); //In case window already exists
                }
            }

            if (e.Key == Key.F3) //Run search from previous find window and open search window
            {
                if (!this.IsVisible)
                    Show();

                doSearch();
            }
        }

        public void Reset()
        {
            this.currentPosition = 0;
            this.lastCol = 0;
            this.lastLine = 0;
            this.done = false;
        }

        public void doSearch()
        {
            if (done) //Reset search position if a previous search has been finished
            {
                this.Reset();
                return;
            }

            if (this.currentPosition > parentTextBox.Text.Length) //Could apply when changing text content in our TextBox parent
                this.currentPosition = 0;

            if (txtSearch.Text == "") //New search, always show window
            {
                this.Focus();
                return;
            }

            if (chkCaseSensitive.IsChecked == false)
                this.currentPosition = parentTextBox.Text.ToLower().IndexOf(txtSearch.Text.ToLower(), this.currentPosition + 1); //Case in-sensitive search
            else this.currentPosition = parentTextBox.Text.IndexOf(txtSearch.Text, this.currentPosition + 1); //Case sensitive search

            if (this.currentPosition != -1)
            {
                // select the newly found neelde
                parentTextBox.Select(this.currentPosition, txtSearch.Text.Length);
                
                // jump view to newly found needle
                int line = parentTextBox.GetLineIndexFromCharacterIndex(parentTextBox.SelectionStart);
                if (this.lastLine != line)
                {
                    this.lastCol = 0;
                    this.lastLine = line;
                }
                
                this.lastCol = parentTextBox.GetLineText(line).IndexOf(parentTextBox.SelectedText, this.lastCol);
                this.lastCol++;

                parentTextBox.ScrollToLine(line);
                if (this.lastCol > (parentTextBox.ActualWidth / parentTextBox.FontSize))
                    parentTextBox.ScrollToHorizontalOffset(this.lastCol*(parentTextBox.FontSize/2));
                else parentTextBox.ScrollToHorizontalOffset(0);
                
                // required or selection will not be visible
                parentTextBox.Focus();
            }
            else
            {
                MessageBox.Show("Cannot find: \"" + txtSearch.Text + "\"", "Find text");
                txtSearch.SelectAll(); //Preselect all text, because we want probably want to change it anyway
                this.Focus(); //Brings back search form for easy editing
                this.done = true; //To filter KeyDown event on parent
            }
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            doSearch();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Keyboard shortcuts
            if (e.Key == Key.Escape)
                this.Hide();

            if (e.Key == Key.F3 || e.Key == Key.Enter)
                doSearch();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
