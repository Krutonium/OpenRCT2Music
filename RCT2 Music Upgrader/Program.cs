using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.Linq;
using System.Timers;

namespace RCT2_Music_Upgrader
{
    
    internal class Program
    {
        
        public static async Task Main(string[] args)
        {
            //Music: https://www.youtube.com/watch?v=ykuT2RqB_LM
            Console.Clear();
            if (!File.Exists("raw.webm"))
            {
                YoutubeClient youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync("https://www.youtube.com/watch?v=ykuT2RqB_LM");
                Console.WriteLine("Downloading " + video.Title + " by " + video.Author);
                var manifest = await youtube.Videos.Streams.GetManifestAsync(video.Url);
                var stream = manifest.GetAudioOnly().WithHighestBitrate();
                var audio = stream.Url;
                WebClient client = new WebClient();
                Timer timer = new Timer();
                timer.Interval = 200;
                timer.Enabled = true;
                timer.Elapsed += TimerOnElapsed;
                timer.Start();
                client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
                client.DownloadDataCompleted += ClientOnDownloadDataCompleted;
                client.DownloadDataAsync(new Uri(audio));
                while (!done)
                {
                    System.Threading.Thread.Sleep(200);
                }
                File.WriteAllBytes("./raw.webm", rawMusic);
            }
            else
            {
                Console.WriteLine("Existing Music File Found, Continuing...");
            }
            
            DoRips();

        }

        private static void DoRips()
        {
            if (Directory.Exists("./out"))
            {
                Directory.Delete("./out", true);
            }

            Directory.CreateDirectory("./out");
            
            ExtractAudio("0:00", "1:35", "css18.dat"); //Roman Fanfare
            ExtractAudio("1:36", "1:55", "css19.dat"); //Oriental Style
            ExtractAudio("3:31", "2:18", "css20.dat"); //Martian Style
            //At this stage it's entirely POC, and *does* produce usable audio. Feel free to finish this up!
            //00:00 Roman Fanfare Style
            //01:35 Oriental Style
            //03:30 Martian Style
            //05:49 Jungle Drums Style
            //07:39 Toyland Style
            //09:45 Space Style
            //12:36 Horror Style
            //14:38 Techno Style
            //16:36 Water Style
            //18:32 Wild West Style
            //20:43 Jurassic Style
            //22:56 Rock Style
            //24:58 Rock Style 2
            //26:40 Ice Style
            //29:10 Snow Style
            //31:05 Medieval Style
        }

        private static void ExtractAudio(string timeStart, string Duration, string outFileName, string inputFile = "./raw.webm")
        {
            ProcessStartInfo settings = new ProcessStartInfo();
            settings.FileName = "ffmpeg";
            settings.Arguments = "-i " + inputFile + " -ss " + timeStart + " -t " + Duration + " -f wav -ar 22050 ./out/" + outFileName; //Roman Fanfare
            //Remove -ar 22050 for even better audio, at the expense of broken in-game audio
            var process = Process.Start(settings);
            process.WaitForExit();
        }
        private static bool done = false;
        private static byte[] rawMusic;
        private static void ClientOnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("Download Complete, cutting...");
            rawMusic = e.Result;
            done = true;
        }

        private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (rotator == "-")
            {
                rotator = "/";
            } else if (rotator == "/")
            {
                rotator = "|";
            } else if (rotator == "|")
            {
                rotator = @"\";
            } else if (rotator == @"\")
            {
                rotator = "-";
            }
        }

        private static string rotator = "-";
        private static void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double percent = ((double) e.BytesReceived / (double) e.TotalBytesToReceive)*100;
            string printable = percent.ToString("0");
            Console.SetCursorPosition(0, 1);
            Console.Write("Downloading - " + printable + "% " + rotator);
        }
    }
}