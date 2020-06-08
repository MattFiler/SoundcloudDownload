using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SoundcloudDownload
{
    public partial class SoundcloudDownload : Form
    {
        enum CurrentDownloadState { DOWNLOADING_MP3, DOWNLOADING_ARTWORK, APPLYING_METADATA, TIDYING };

        public SoundcloudDownload()
        {
            InitializeComponent();
        }

        private void SoundcloudDownload_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false; 
        }

        private void startDownload_Click(object sender, EventArgs e)
        {
            startDownload.Enabled = false;
            soundcloudURLs.ReadOnly = true;

            Thread t = new Thread(() => DownloadFiles());
            t.Start();
        }

        private void DownloadFiles()
        {
            progressBar.Value = 0;
            progressBar.Maximum = 100 * soundcloudURLs.Lines.Count();
            List<Failure> failedDownloads = new List<Failure>();

            foreach (string url in soundcloudURLs.Lines)
            {
                try
                {
                    JToken metadata = GetMetadata(url);
                    if (metadata["error"].Value<string>() != "Success")
                    {
                        MessageBox.Show(metadata["error"].Value<string>(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                    string outputPath = "output/";
                    if (metadata["is_playlist"].Value<bool>()) outputPath += string.Join("_", metadata["name"].Value<string>().Split(Path.GetInvalidFileNameChars())) + "/";
                    Directory.CreateDirectory(outputPath);
                    progressBar.Value += 50;

                    foreach (JObject song in metadata["songs"])
                    {
                        CurrentDownloadState currentState = CurrentDownloadState.DOWNLOADING_MP3;
                        try
                        {
                            WebClient client = new WebClient();
                            string fileName = outputPath + string.Join("_", song["metadata"]["title"].Value<string>().Split(Path.GetInvalidFileNameChars()));
                            client.DownloadFile(song["urls"]["mp3"].Value<string>(), fileName + ".mp3");

                            currentState = CurrentDownloadState.APPLYING_METADATA;
                            var mp3file = TagLib.File.Create(fileName + ".mp3");
                            mp3file.Tag.Title = song["metadata"]["title"].Value<string>();
                            mp3file.Tag.AlbumArtists = new string[] { song["metadata"]["artist"].Value<string>() };
                            mp3file.Tag.Performers = new string[] { song["metadata"]["artist"].Value<string>() };
                            if (song["urls"]["artwork"].Value<string>() != "")
                            {
                                currentState = CurrentDownloadState.DOWNLOADING_ARTWORK;
                                client.DownloadFile(song["urls"]["artwork"].Value<string>(), fileName + ".png");
                                TagLib.IPicture[] pictures = new TagLib.IPicture[] { new TagLib.Picture(fileName + ".png") };
                                mp3file.Tag.Pictures = pictures;
                            }
                            mp3file.Save();

                            currentState = CurrentDownloadState.TIDYING;
                            File.Delete(fileName + ".png");
                        }
                        catch (Exception _e)
                        {
                            Failure newFail = new Failure();
                            newFail.filename = song["metadata"]["title"].Value<string>();
                            switch (currentState)
                            {
                                case CurrentDownloadState.DOWNLOADING_MP3:
                                    newFail.reason = "Failed to download track \"" + song["metadata"]["title"] + "\"!";
                                    break;
                                case CurrentDownloadState.DOWNLOADING_ARTWORK:
                                    newFail.reason = "Failed to download artwork for \"" + song["metadata"]["title"] + "\".";
                                    break;
                                case CurrentDownloadState.APPLYING_METADATA:
                                    newFail.reason = "Failed to apply metadata for \"" + song["metadata"]["title"] + "\".";
                                    break;
                                case CurrentDownloadState.TIDYING:
                                    newFail.reason = "Failed to tidy files after downloading \"" + song["metadata"]["title"] + "\".";
                                    break;
                            }
                            newFail.reason += "\nReason: " + _e.Message;
                            failedDownloads.Add(newFail);
                        }
                        progressBar.Value += (50 / metadata["songs"].Count());
                    }
                }
                catch (Exception _e)
                {
                    MessageBox.Show("Reason: " + _e.Message, "Failed to download!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("Completed, with " + failedDownloads.Count + " issues.", "Complete.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (failedDownloads.Count != 0)
            {
                List<string> toWrite = new List<string>();
                foreach (Failure fail in failedDownloads)
                {
                    toWrite.Add("Filename: " + fail.filename + "\nError: " + fail.reason + "\n\n");
                }
                File.WriteAllLines("error.log", toWrite);
                MessageBox.Show("Issues have been logged.", "Issues encountered.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Process.Start("error.log");
            }
            progressBar.Value = progressBar.Maximum;
            startDownload.Enabled = true;
            soundcloudURLs.ReadOnly = false;
            soundcloudURLs.Text = "";
        }

        private JToken GetMetadata(string url)
        {
            var request = WebRequest.Create("http://myfiles.mattfiler.co.uk/soundcloud/to_json.php?url=" + url);
            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
            {
                return JToken.Parse(reader.ReadToEnd());
            }
        }
    }

    public class Failure
    {
        public string filename;
        public string reason;
    }
}
