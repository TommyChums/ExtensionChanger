using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace FIleEndings
{
    public partial class Form1 : Form {
	    private string path;
		private List<string> files = new List<string>();

        public Form1() {
            InitializeComponent();
        }

        private void SelectFolder(object sender, EventArgs e) {
			files.Clear();
			FolderBrowserDialog dialog = new FolderBrowserDialog() {
				Description = @"Select the folder with files for changing"
			};

			var result = dialog.ShowDialog();

			if (result == DialogResult.OK) {
				path = dialog.SelectedPath;
				textBox1.Text = path;

				foreach (var f in Directory.GetFiles(path)) {
					files.Add(f);
				}

				ShowFiles();
            }

        }

        private void ShowFiles() {
	        if (files.Count < 1) {
				textBox2.Clear();
				return;
	        }

			textBox2.Clear();
            var paths = files.ToArray();
            textBox2.Lines = paths;
        }

        private void ChangeFileExtension(string path, string extension) {
            if (!extension.StartsWith(".")) extension.Insert(0, ".");
            
	        File.Move(path, Path.ChangeExtension(path, extension));
        }

        private void ClearLastFile(object sender, EventArgs e) {
	        if (files.Count < 1) return;
	        files.RemoveAt(files.Count - 1);
			ShowFiles();
        }

        private void ClearAllFiles(object sender, EventArgs e) {
	        files.Clear();
			ShowFiles();
        }

        private void ShowDiscordCache(object sender, EventArgs e) {
	        files.Clear();
	        if (!Directory.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\discord\Cache")) return;
	        textBox1.Text = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\discord\Cache";

	        foreach (var f in Directory.GetFiles(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\discord\Cache")) {
		        files.Add(f);
	        }
	        ShowFiles();
        }

        private void SelectFile(object sender, EventArgs e) {
	        OpenFileDialog dialog = new OpenFileDialog() {
		        Title = @"Select File(s) to change extension",
		        CheckFileExists = true,
		        Multiselect = true
	        };

	        var result = dialog.ShowDialog();

	        if (result == DialogResult.OK) {
		        var names = dialog.FileNames;
		        
		        foreach (var f in names) {
			        if (files.Contains(f)) continue;

			        files.Add(f);
		        }

		        ShowFiles();
	        }
        }

        private void Begin(object sender, EventArgs e) {

	        var extension = comboBox1.Text.Trim();

	        if (files.Count < 1) {
		        MessageBox.Show("Please select a file.");
				return;
	        }

	        var result = DialogResult;

            if (extension.Length < 1)
		        result = MessageBox.Show("Are you sure you wish to remove all extensions?", "Continue", MessageBoxButtons.YesNo);
            else
		        result = MessageBox.Show("Change all files to " + extension + "?", "Continue", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes) {
	            var percentage = (files.Count / files.Count) * 100;
	            int i = 0;
                bool success = true;
                foreach (var file in files) {
	                try {
		                ChangeFileExtension(file, extension);
	                }
	                catch (Exception ex) {
		                result = MessageBox.Show("Error trying to change " + file + Environment.NewLine + ex.Message, "Move on?", MessageBoxButtons.YesNo);
		                if (result == DialogResult.Yes) continue;

		                else {
                            success = false;
                            progressBar1.Value = 0;
                            return;
		                }
	                }
                    i++;
                    percentage = (i / files.Count) * 100;
	                progressBar1.Value = percentage;
                }

                if(success) MessageBox.Show("Successfully changed all file extensions");
                progressBar1.Value = 0;
            }
        }
    }
}
