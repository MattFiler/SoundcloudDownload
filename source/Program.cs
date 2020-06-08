using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace SoundcloudDownload
{
    class Program
    {
        enum CurrentDownloadState { DOWNLOADING_MP3, DOWNLOADING_ARTWORK, APPLYING_METADATA, TIDYING };

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter soundcloud playlist or track URL:");
                string soundcloudURL = Console.ReadLine();

                try
                {
                    Console.WriteLine("Processing...");
                    JToken metadata = GetMetadata(soundcloudURL);
                    if (metadata["error"].Value<string>() != "Success")
                    {
                        Console.WriteLine(metadata["error"].Value<string>());
                        continue;
                    }
                    string outputPath = "output/";
                    if (metadata["is_playlist"].Value<bool>()) outputPath += string.Join("_", metadata["name"].Value<string>().Split(Path.GetInvalidFileNameChars())) + "/";
                    Directory.CreateDirectory(outputPath);

                    Console.WriteLine("Downloading...");
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
                        catch (Exception e)
                        {
                            switch (currentState)
                            {
                                case CurrentDownloadState.DOWNLOADING_MP3:
                                    Console.WriteLine("Failed to download track \"" + song["metadata"]["title"] + "\"!");
                                    break;
                                case CurrentDownloadState.DOWNLOADING_ARTWORK:
                                    Console.WriteLine("Failed to download artwork for \"" + song["metadata"]["title"] + "\".");
                                    break;
                                case CurrentDownloadState.APPLYING_METADATA:
                                    Console.WriteLine("Failed to apply metadata for \"" + song["metadata"]["title"] + "\".");
                                    break;
                                case CurrentDownloadState.TIDYING:
                                    Console.WriteLine("Failed to tidy files after downloading \"" + song["metadata"]["title"] + "\".");
                                    break;
                            }
                            Console.WriteLine("Reason: " + e.Message);
                        }
                    }

                    Console.WriteLine("Done!");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to download!");
                    Console.WriteLine("Reason: " + e.Message);
                }
            }
        }

        static private JToken GetMetadata(string url)
        {
            var request = WebRequest.Create("http://myfiles.mattfiler.co.uk/soundcloud/to_json.php?url=" + url);
            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream(), ASCIIEncoding.ASCII))
            {
                return JToken.Parse(reader.ReadToEnd());
            }
        }
    }
}
