using KrKrSceneManager;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScnEditorGUI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _currentFilename = string.Empty;
        int _selectedIndex = -1;
        bool _resourceMode, _lastCtrl, _changedFile;
        readonly PSBResManager _prm = new PSBResManager();
        PSBAnalyzer _scn;

        /// <summary>
        /// Reset all controls to initial state
        /// </summary>
        private void ResetControls()
        {
            this._currentFilename = string.Empty;
            this._selectedIndex = -1;
            this._resourceMode = false;
            this._lastCtrl = false;
            this._changedFile = false;
            this.Title = "Scn Editor";
            this.buttonSave.IsEnabled = false;
            this.buttonFind.IsEnabled = false;
            this.buttonClose.IsEnabled = false;
            this.listBoxMain.Items.Clear();
            this.textBoxMain.Text = string.Empty;
        }

        /// <summary>
        /// Open file
        /// </summary>
        /// <param name="path">Full path of file to open</param>
        private void OpenFile(string path)
        {
            if (path.EndsWith(".pimg"))
            {
                this._resourceMode = true;
                FileEntry[] resource = this._prm.Import(File.ReadAllBytes(path));
                for (int index = 0; index < resource.Length; index++)
                {
                    File.WriteAllBytes(
                        Path.GetDirectoryName(path) + @"\" + index + ".res",
                        resource[index].Data
                    );
                }

                MessageBox.Show(
                    "Resources Extracted in the Program Directory...",
                    "Resource Manager",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            else
            {
                this._resourceMode = false;
                this._scn = new PSBAnalyzer(File.ReadAllBytes(path));
                foreach (string text in this._scn.Import())
                {
                    this.listBoxMain.Items.Add(text);
                }

                if(this.listBoxMain.Items.Count > 0)
                {
                    this.listBoxMain.ScrollIntoView(this.listBoxMain.Items[0]);
                }

                if (this._scn.UnkOpCodes)
                {
                    MessageBox.Show("Maybe the reorder is wrong... try create a issue");
                }
                if (this._scn.HaveEmbedded)
                {
                    MessageBox.Show("Looks this PSB contains a Embedded File, try open as .pimg");
                }
            }
        }

        /// <summary>
        /// Displays a save file dialog.
        /// </summary>
        /// <returns>
        /// If the user clicks the OK button of the dialog that is displayed
        /// (e.g. Microsoft.Win32.OpenFileDialog, Microsoft.Win32.SaveFileDialog), 
        /// true is returned; otherwise, false.
        /// </returns>
        private bool OpenSaveFileDialog()
        {
            bool showDialogResult;
            SaveFileDialog save = new SaveFileDialog
            {
                FileName = Path.GetFileName(_currentFilename)
            };

            if (this._resourceMode)
            {
                save.Filter = "Pack of Resources | *.pimg";
                showDialogResult = save.ShowDialog() ?? false;
                if (showDialogResult == true)
                {
                    FileEntry[] Images = new FileEntry[this._prm.EntryCount];
                    for (int index = 0; index < Images.Length; index++)
                    {
                        Images[index] = new FileEntry
                        {
                            Data = File.ReadAllBytes(
                                AppDomain.CurrentDomain.BaseDirectory + index + ".res"
                            )
                        };
                    }
                    byte[] result = this._prm.Export(Images);
                    File.WriteAllBytes(save.FileName, result);
                }
            }
            else
            {
                save.Filter = "KiriKiri Compiled Files (*.scn.m) | *.m|"
                    + "KiriKiri Compiled Files (*.psb) | *.psb|"
                    + "Pack of Resources (*.pimg) | *.pimg|"
                    + "All types | *.*";
                showDialogResult = save.ShowDialog() ?? false;
                if (showDialogResult == true)
                {
                    string[] strings = new string[this.listBoxMain.Items.Count];
                    for (int index = 0; index < strings.Length; index++)
                    {
                        strings[index] = this.listBoxMain.Items[index].ToString();
                    }

                    MessageBoxResult result = MessageBox.Show(
                        "Would you like to compress the script? \n\nIt doesn't work with some games.",
                        "Scn Editor",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );
                    this._scn.CompressPackage = result == MessageBoxResult.Yes;
                    PSBStrMan.CompressionLevel = CompressionLevel.Z_BEST_COMPRESSION; //optional
                    byte[] outfile = this._scn.Export(strings);
                    File.WriteAllBytes(save.FileName, outfile);
                }
            }

            if (showDialogResult)
            {
                this._currentFilename = save.FileName;
                this.Title = this._currentFilename + " - id: " + this._selectedIndex;
                this._changedFile = false;

                MessageBox.Show("File saved.");
            }

            return showDialogResult;
        }

        /// <summary>
        /// Check if the file has been changed before closing
        /// </summary>
        /// <returns>If can close file true returned; otherwise, false</returns>
        private bool CanCloseFile()
        {
            bool yes = true;

            if (this._changedFile)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Save file \"{Path.GetFileName(this._currentFilename)}\" ?",
                    "Save",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    yes = this.OpenSaveFileDialog();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    yes = false;
                }
            }

            return yes;
        }

        /// <summary>
        /// Update item text
        /// </summary>
        private void UpdateItemText()
        {
            if(this._selectedIndex != -1)
            {
                this.listBoxMain.Items[this._selectedIndex] = this.textBoxMain.Text;
                this.listBoxMain.SelectedIndex = this._selectedIndex;
                this.listBoxMain.ScrollIntoView(this.listBoxMain.Items[this._selectedIndex]);
                this.Title = this._currentFilename + " - id: " + this._selectedIndex;
                this._changedFile = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                this._lastCtrl = true;
            }
            else if (this._currentFilename != string.Empty && this._lastCtrl)
            {
                this._lastCtrl = false;

                if (e.Key == Key.S) // Ctrl + S => Save
                {
                    this.OpenSaveFileDialog();
                    e.Handled = true; // Stops other controls on the window receiving event.
                }
                else if(e.Key == Key.F) // Ctrl + F => Find
                {
                    Find modalFind = new Find(this);
                    modalFind.ShowDialog();
                    e.Handled = true;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = ! this.CanCloseFile();
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            if (this.CanCloseFile())
            {
                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    FileName = "",
                    Filter = "KiriKiri Compiled Files | *.scn; *.m; *.psb|Pack of Resources | *.pimg"
                };
                if (fileDialog.ShowDialog() == true)
                {
                    this.ResetControls();
                    this.OpenFile(fileDialog.FileName);
                    this._currentFilename = fileDialog.FileName;
                    this.Title = fileDialog.FileName;
                    this.buttonSave.IsEnabled = true;
                    this.buttonFind.IsEnabled = true;
                    this.buttonClose.IsEnabled = true;
                    this.textBoxMain.Text = string.Empty;
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.OpenSaveFileDialog();
        }

        private void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            Find modalFind = new Find(this);
            modalFind.ShowDialog();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (this.CanCloseFile())
            {
                this.ResetControls();
            }
        }

        private void ListBoxMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listBoxMain.SelectedIndex != -1)
            {
                this._selectedIndex = this.listBoxMain.SelectedIndex;
                this.textBoxMain.Text = this.listBoxMain.Items[this._selectedIndex].ToString();
                this.Title = this._currentFilename + " - id: " + this._selectedIndex;
            }
        }

        private void TextBoxMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                this.UpdateItemText();
            }
        }

        private void TextBoxMain_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (this._lastCtrl && e.Key == Key.V)
            {
                this.UpdateItemText();
            }
        }

        private void TextBoxMain_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.textBoxMain.SelectAll();
        }

        public MainWindow()
        {
            InitializeComponent();
            MessageBox.Show("This GUI don't is a stable translation tool, this program is a Demo"
                + " and a opensource project to allow you make your program to edit any scn file"
                + " (with sig PSB or MDF) or TJS2 Files (with sig TJS2100)\n\nHow to use:\n* Open"
                + " the file\n* Select the string in listbox and edit in the text box\n* Press enter"
                + "to update the string\n\nThis program is unstable!");

            string maximized = ConfigurationManager.AppSettings["maximized"];
            string fontSize = ConfigurationManager.AppSettings["font_size"];
            string textboxSelectAllWhenClick = ConfigurationManager.AppSettings["textbox_select_all_when_click"];
            string textboxEditItemWhenCtrlV = ConfigurationManager.AppSettings["textbox_edit_item_when_ctrl_v"];

            if(maximized == "true")
            {
                this.WindowState = WindowState.Maximized;
            }

            this.FontSize = float.Parse(fontSize);

            if (textboxSelectAllWhenClick == "true")
            {
                this.textBoxMain.PreviewMouseUp += new MouseButtonEventHandler(this.TextBoxMain_PreviewMouseUp);
            }

            if (textboxEditItemWhenCtrlV == "true")
            {
                this.textBoxMain.PreviewKeyUp += new KeyEventHandler(this.TextBoxMain_PreviewKeyUp);
            }
        }

        /// <summary>
        /// Find string on list box
        /// </summary>
        /// <param name="searchString">String for search</param>
        public void FindString(string searchString)
        {
            bool findOk = false;

            // Ensure we have a proper string to search for.
            if (!string.IsNullOrEmpty(searchString))
            {
                // Find the item in the list and store the index to the item.
                for (int index = 0; index < this.listBoxMain.Items.Count; index++)
                {
                    if (this.listBoxMain.Items[index].ToString().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        this.Title = this._currentFilename + " - id: " + index;
                        this.listBoxMain.SelectedIndex = index;
                        this.listBoxMain.ScrollIntoView(this.listBoxMain.Items[index]);
                        this.textBoxMain.Text = this.listBoxMain.Items[index].ToString();
                        findOk = true;
                        break;
                    }
                }
            }

            if (!findOk)
            {
                MessageBox.Show("The search string did not match any items in the list");
            }
        }
    }
}
