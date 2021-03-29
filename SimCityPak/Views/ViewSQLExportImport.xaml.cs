using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using Gibbed.Spore.Package;
using Gibbed.Spore.Properties;
using SimCityPak.PackageReader;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Data.SQLite;

namespace SimCityPak.Views
{
    /// <summary>
    /// Interaction logic for ViewSQLExportImport.xaml
    /// </summary>
    public partial class ViewSQLExportImport : Window
    {
        private List<string> _tables;

        public ViewSQLExportImport()
        {
            InitializeComponent();
            
            _tables = new List<string>();
            List<string> databases = new List<string>();

            foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.s3db"))
            {
                databases.Add(file.Substring(Directory.GetCurrentDirectory().Length + 1));
            }

            cmbDatabases.ItemsSource = databases;
        }

        private bool executeSql(SQLiteConnection dbConn, string cmd)
        {
            SQLiteCommand query = dbConn.CreateCommand();
            query.CommandText = cmd;
            try
            {
                query.ExecuteNonQuery();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "SQLite file|*.sql";
            int affectedRows = 0;

            SQLiteConnection dbConn = new SQLiteConnection(String.Format("Data Source={0}", cmbDatabases.SelectedItem.ToString()));
            dbConn.Open();

            try
            {
                if (fileDialog.ShowDialog().GetValueOrDefault(false))
                {
                    foreach (string fileName in fileDialog.FileNames)
                    {
                        if (File.Exists(fileName))
                        {
                            using (TextReader reader = File.OpenText(fileName))
                            {
                                executeSql(dbConn, "BEGIN TRANSACTION;"); // avoids diskflushing

                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (executeSql(dbConn, line) == false)
                                    {
                                        System.Windows.Forms.MessageBox.Show("Error executing query at line: " + line);
                                        dbConn.Close();
                                        return;
                                    }
                                    affectedRows++;
                                }

                                executeSql(dbConn, "END TRANSACTION;"); // commits mem cache to disk
                            }
                        }
                    }
                }
            }
            catch
            {
                dbConn.Close();
                System.Windows.Forms.MessageBox.Show("Unable to read file", "Error");
            }

            dbConn.Close();
            System.Windows.Forms.MessageBox.Show("Updated OK executed: " + affectedRows + " lines", "Success!");
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "SQL File|*.sql";
            fileDialog.OverwritePrompt = false; // appends if file exists
            int exportedRows = 0;

            if (!fileDialog.ShowDialog().GetValueOrDefault(false))
            {
                return;
            }

            // and dumps the DB
            if (File.Exists(cmbDatabases.SelectedItem.ToString()))
            {
                List<string> result = new List<string>();
                result.Add("-- SimCityPak SQL Export");
                result.Add("-- Database: " + cmbDatabases.SelectedItem);
                result.Add("-- Table: " + cmbTables.SelectedItem);

                SQLiteConnection dbConn = new SQLiteConnection(String.Format("Data Source={0}", cmbDatabases.SelectedItem.ToString()));
                dbConn.Open();

                SQLiteCommand query = dbConn.CreateCommand();
                query.CommandText = "select * from " + cmbTables.SelectedItem.ToString();
                SQLiteDataReader reader = query.ExecuteReader();

                while (reader.Read())
                {
                    string newResultLine = "insert or replace into " + cmbTables.SelectedItem.ToString() + " (";
                    string keysCvs = "";
                    string valuesCvs = "";

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        keysCvs += reader.GetName(i).ToString() + ",";
                        valuesCvs += "'" + reader.GetValue(i).ToString() + "',";
                    }

                    newResultLine += keysCvs.TrimEnd(',');
                    newResultLine += ") values (";
                    newResultLine += valuesCvs.TrimEnd(',');
                    newResultLine += ");";

                    result.Add(newResultLine);
                    exportedRows++;
                }

                reader.Close();
                dbConn.Close();

                StreamWriter file;
                try
                {
                    file = new StreamWriter(fileDialog.FileName, false); // flush if exists
                }
                catch { System.Windows.MessageBox.Show("Unable to save to sql file", "Error!"); return; }

                foreach (string line in result)
                {
                    file.WriteLine(line);
                }

                file.Close();
                System.Windows.MessageBox.Show("Table " + cmbTables.SelectedItem + " exported (" + exportedRows + " records saved)", "Success!");
            }
        }

        private void cmbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tables.Clear();

            if (File.Exists(cmbDatabases.SelectedItem.ToString()))
            {
                //_tables.Add("* (all)"); not supported yet...

                btnImport.IsEnabled = true;
                cmbTables.IsEnabled = true;

                SQLiteConnection dbConn = new SQLiteConnection(String.Format("Data Source={0}", cmbDatabases.SelectedItem.ToString()));
                dbConn.Open();

                SQLiteCommand query = dbConn.CreateCommand();
                query.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";

                SQLiteDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    _tables.Add(reader["name"].ToString());
                }

                reader.Close();
                dbConn.Close();

                cmbTables.ItemsSource = _tables;
            }
            else
            {
                btnExport.IsEnabled = false;
                btnImport.IsEnabled = false;
                cmbTables.IsEnabled = false;
            }
        }

        private void cmbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTables.SelectedItem.ToString() != "")
            {
                btnExport.IsEnabled = true;
            }
            else
            {
                btnExport.IsEnabled = true;
            }
        }
    }
}
