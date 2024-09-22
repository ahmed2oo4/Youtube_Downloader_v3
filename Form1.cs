using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace YouTubeDownloaderApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Set form background color
            this.BackColor = System.Drawing.Color.FromArgb(0, 51, 102); // Dark Blue

            // Configure TextBox
            txtUrl.BackColor = System.Drawing.Color.White;
            txtUrl.ForeColor = System.Drawing.Color.Black;

            // Configure ComboBox
            cmbQuality.BackColor = System.Drawing.Color.White;
            cmbQuality.ForeColor = System.Drawing.Color.Black;

            // Configure Button
            btnDownload.BackColor = System.Drawing.Color.FromArgb(102, 163, 255); // Light Blue
            btnDownload.ForeColor = System.Drawing.Color.White;

            // Configure ProgressBar
            progressBar.BackColor = System.Drawing.Color.LightGray;
            progressBar.ForeColor = System.Drawing.Color.Blue;

            // Configure Label
            lblStatus.ForeColor = System.Drawing.Color.White; // White text for status

            // Populate the quality options
            cmbQuality.Items.Add("144p");
            cmbQuality.Items.Add("360p");
            cmbQuality.Items.Add("720p");
            cmbQuality.Items.Add("1080p");
            cmbQuality.SelectedIndex = 0; // Default selection
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove invalid characters from the file name
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), "_"); // Replace with underscore
            }
            return fileName;
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            lblStatus.Text = "Downloading...";
            progressBar.Value = 0;

            try
            {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(url); // Use the URL directly

                // Get available streams
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var streams = streamManifest.GetMuxedStreams().ToList(); // Accessing MuxedStreams

                // Choose the selected quality
                var selectedQuality = cmbQuality.SelectedItem.ToString();
                var selectedStream = streams.FirstOrDefault(s => s.VideoQuality.Label == selectedQuality);

                if (selectedStream != null)
                {
                    string sanitizedTitle = SanitizeFileName(video.Title);
                    var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{sanitizedTitle}.{selectedStream.Container}");

                    // Start the download
                    await youtube.Videos.Streams.DownloadAsync(selectedStream, filePath, new Progress<double>(progress =>
                    {
                        progressBar.Value = (int)(progress * 100);
                    }));

                    lblStatus.Text = "Download completed!";
                }
                else
                {
                    lblStatus.Text = "Selected quality not available.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }
    }
}
