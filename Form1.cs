using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
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
		long sizeOfUpdate = 0;

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

        private void UpdateApplication()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                ad.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(ad_CheckForUpdateCompleted);
                ad.CheckForUpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_CheckForUpdateProgressChanged);

                ad.CheckForUpdateAsync();
            }
        }

        void ad_CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            downloadStatus.Text = String.Format("Downloading: {0}. {1:D}K of {2:D}K downloaded.", GetProgressString(e.State), e.BytesCompleted / 1024, e.BytesTotal / 1024);
        }

        string GetProgressString(DeploymentProgressState state)
        {
            if (state == DeploymentProgressState.DownloadingApplicationFiles)
            {
                return "application files";
            }
            else if (state == DeploymentProgressState.DownloadingApplicationInformation)
            {
                return "application manifest";
            }
            else
            {
                return "deployment manifest";
            }
        }

        void ad_CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("ERROR: Could not retrieve new version of the application. Reason: \n" + e.Error.Message + "\nPlease report this error to the system administrator.");
                downloadStatus.Clear();
                return;
            }
            else if (e.Cancelled == true)
            {
                MessageBox.Show("The update was cancelled.");
                downloadStatus.Clear();
            }

            // Ask the user if they would like to update the application now.
            if (e.UpdateAvailable)
            {
                sizeOfUpdate = e.UpdateSizeBytes;

                if (!e.IsUpdateRequired)
                {
                    DialogResult dr = MessageBox.Show("An update is available. Would you like to update the application now?\n\nEstimated Download Time: ", "Update Available", MessageBoxButtons.OKCancel);
                    if (DialogResult.OK == dr)
                    {
                        BeginUpdate();
                    }
                }
                else
                {
                    MessageBox.Show("A mandatory update is available for your application. We will install the update now, after which we will save all of your in-progress data and restart your application.");
                    BeginUpdate();
                }
            }
            else
            {
                MessageBox.Show("No Update Available");
                downloadStatus.Clear();
            }
        }

        private void BeginUpdate()
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.UpdateCompleted += new AsyncCompletedEventHandler(ad_UpdateCompleted);

            // Indicate progress in the application's status bar.
            ad.UpdateProgressChanged += new DeploymentProgressChangedEventHandler(ad_UpdateProgressChanged);
            ad.UpdateAsync();
        }

        void ad_UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            String progressText = String.Format("{0:D}K out of {1:D}K downloaded - {2:D}% complete", e.BytesCompleted / 1024, e.BytesTotal / 1024, e.ProgressPercentage);
            downloadStatus.Text = progressText;
        }

        void ad_UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("The update of the application's latest version was cancelled.");
                downloadStatus.Clear();
                return;
            }
            else if (e.Error != null)
            {
                MessageBox.Show("ERROR: Could not install the latest version of the application. Reason: \n" + e.Error.Message + "\nPlease report this error to the system administrator.");
                downloadStatus.Clear();
                return;
            }

            DialogResult dr = MessageBox.Show("The application has been updated. Restart? (If you do not restart now, the new version will not take effect until after you quit and launch the application again.)", "Restart Application", MessageBoxButtons.OKCancel);
            if (DialogResult.OK == dr)
            {
                Application.Restart();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			UpdateApplication();
        }
    }
}
