using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace COH2_SMF_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += ConvertSMFFiles;
            worker.ProgressChanged += WorkerProgressChanged;
            worker.RunWorkerAsync();
        }

        private void ConvertSMFFiles(object sender, DoWorkEventArgs e)
        {
            ToggleInputGrid();

            try
            {
                int currentProgress = 0;
                (sender as BackgroundWorker).ReportProgress(currentProgress);

                string smfFilesLocation = string.Empty;
                string destinationLocation = string.Empty;

                Dispatcher.Invoke(() =>
                {
                    smfFilesLocation = tbSMFLocation.Text;
                    destinationLocation = tbDestinationLocation.Text;
                });

                if (!string.IsNullOrEmpty(smfFilesLocation) && !string.IsNullOrEmpty(destinationLocation))
                {
                    List<string> smfFiles = Directory.GetFiles(smfFilesLocation, "*.smf", SearchOption.AllDirectories).ToList();

                    Dispatcher.Invoke(() =>
                    {
                        pbGlobalProgress.Maximum = smfFiles.Count;
                    });

                    foreach (string file in smfFiles)
                    {
                        currentProgress++;
                        (sender as BackgroundWorker).ReportProgress(currentProgress);

                        string subDirDestination = file.Substring(smfFilesLocation.Length + 1).Replace(".smf", ".wav");
                        string destinationFile = Path.Combine(destinationLocation, subDirDestination);

                        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));

                        byte[] original = File.ReadAllBytes(file);
                        byte[] modified = new byte[original.Length - 16];

                        Buffer.BlockCopy(original, 16, modified, 0, modified.Length);

                        File.WriteAllBytes(destinationFile, modified);
                    }

                    ProcessComplete();
                }
                else
                {
                    MessageBox.Show("Please choose smf files and destination location first.");
                }
            }
            catch (Exception ex)
            {
                ManageException(ex);
            }
            finally
            {
                ToggleInputGrid();
            }
        }

        private void btnSMFLocation_Click(object sender, RoutedEventArgs e)
        {
            ChooseLocation("SMF files location", true, ref tbSMFLocation);
        }

        private void btnDestinationLocation_Click(object sender, RoutedEventArgs e)
        {
            ChooseLocation("Destination location", true, ref tbDestinationLocation);
        }

        /// <summary>
        /// Copy the path of the file in case of a drag and drop over a textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                TextBox textBox = sender as TextBox;
                textBox.Text = Path.GetFullPath(files[0]);
            }
        }

        private void textBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Move;
        }

        private void ChooseLocation(string title, bool isFolderPicker, ref TextBox textBox)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = title;
            dlg.IsFolderPicker = isFolderPicker;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBox.Text = dlg.FileName;
            }
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbGlobalProgress.Value = e.ProgressPercentage;
        }

        private void ProcessComplete()
        {
            MessageBox.Show("Process Complete!");
        }

        private void ManageException(Exception ex)
        {
            MessageBox.Show($"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{ex.InnerException}");
        }

        private void ToggleInputGrid()
        {
            Dispatcher.Invoke(() =>
            {
                mainWindow.IsEnabled = !mainWindow.IsEnabled;
            });
        }
    }
}
