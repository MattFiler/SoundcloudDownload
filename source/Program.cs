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
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter soundcloud playlist or track URL:");
                string soundcloudURL = Console.ReadLine();

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
                    try
                    {
                        WebClient client = new WebClient();
                        string fileName = outputPath + string.Join("_", song["metadata"]["title"].Value<string>().Split(Path.GetInvalidFileNameChars()));
                        client.DownloadFile(song["urls"]["mp3"].Value<string>(), fileName + ".mp3");
                        client.DownloadFile(song["urls"]["artwork"].Value<string>(), fileName + ".png");
                        var mp3file = TagLib.File.Create(fileName + ".mp3");
                        mp3file.Tag.Title = song["metadata"]["title"].Value<string>();
                        mp3file.Tag.AlbumArtists = new string[] { song["metadata"]["artist"].Value<string>() };
                        mp3file.Tag.Performers = new string[] { song["metadata"]["artist"].Value<string>() };
                        TagLib.IPicture[] pictures = new TagLib.IPicture[] { new TagLib.Picture(fileName + ".png") };
                        mp3file.Tag.Pictures = pictures;
                        mp3file.Save();
                        File.Delete(fileName + ".png");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to download \"" + song["metadata"]["title"] + "\" - reason: " + e.Message);
                    }
                }

                Console.WriteLine("Done!");
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
