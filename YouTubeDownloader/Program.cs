using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using YoutubeExtractor;

namespace YouTubeDownloader
{
    class Program
    {

        private static VideoInfo video;     // Video info instance which we will recieve from youtube later
        private static string url = null;   // to save the YouTube URL entered  by the user
        private static int resolution;      // a varible to save the user input for Video resolution
        private static string dir = null;   // path for the current working directory to save video and audio files
        private static string inputFile;    // video file name with extension to send it to the converter
        private static string outputFile;   // desired output audio file name and extension

        static void Main(string[] args)
        {
            // Prints the following and moves the cursor into next line;
            Console.Write("Please copy and paste complete YouTube URL\n\nHERE: ");

            url = Console.ReadLine();       // Reads the URL and saves it to global;
            readRes();                      // a call to a function which reads Resolution as 32-bit int

            //Prints to the screen and moves the cursor a line down
            Console.WriteLine("Please be patient until downloader finishes job!");

            DownloadVideo();                // a call to a function which downloads the video    

            Console.Read();                 // keeps the console on until you press the Enter Key
        }

        private static void Converter()
        {
            // gets teh video title and extension from global and assigns inputfile
            inputFile = video.Title + video.VideoExtension; 
            // assigns outputfile name and extension
            outputFile = video.Title;

            // converter class instance
            var converter = new NReco.VideoConverter.FFMpegConverter();

            // pass input and output file names together with output extension
            converter.ConvertMedia(inputFile, outputFile + ".mp3", "mp3");
            // prints success message and makes beep sound
            Console.WriteLine("Successfully Converted!");
            Console.Beep();

        }


        private static void DownloadVideo()
        {
            // gets current working directory     ../bin/Debug     
            dir = Directory.GetCurrentDirectory();

            // creates unum instance of URL resolver class and passes url
            IEnumerable<VideoInfo> v = DownloadUrlResolver.GetDownloadUrls(url);

            // gets video info from URL resolver and assigns into global
            video = v.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == resolution);

            // checks if url needs special decryption to format it
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video); // formats and decrypts url
                Console.WriteLine("Decyrpetd");
            }

            // makes downloader instance and passes video info and full path to save it
            VideoDownloader vd = new VideoDownloader(video, Path.Combine(dir + "\\", video.Title + video.VideoExtension));

            // built in function gets called when download status changes
            vd.DownloadProgressChanged += Vd_DownloadProgressChanged;
            // built in function gets called when download finishes
            vd.DownloadFinished += Vd_DownloadFinished;

            // executes the download
            vd.Execute();

        }

        private static void Vd_DownloadFinished(object sender, EventArgs e)
        {
            Console.WriteLine("\n\nDownload has finihshed!");
            Console.WriteLine("\nNow converting into MP3...");
            Converter();
        }

        private static void Vd_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            ClearLine();
            Console.Write(new string('#', (int)(e.ProgressPercentage/4)));
            Console.Write("     " + (int)e.ProgressPercentage);            
        }


        public static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        private static void readRes()
        {
            //gets the current console background color for later use
            var color = Console.BackgroundColor;

            // infinite loop to keep reading from user 
            // in case the input is wrong
            // the loop breaks when the user inputs valid 
            // int value and saves resolution into global
            while(true)
            {
                // prints into console and moves the cursor down a line
                Console.Write("Please TYPE the desired resolution (360, 480, 720)\n\nHERE: ");

                // handles the expection thrown in case the user 
                //inputs string or anything other than int value
                try
                {
                    // control will go into catch() after this if the 
                    //input value is anything other than integer
                    int temp = Convert.ToInt32(Console.ReadLine());

                    // if the exception not thrown, change console color to default 
                    Console.BackgroundColor = color;    

                    // to check if the int value obtained matches with video resolutions
                    if(temp==360||temp==480||temp==720)
                    {
                        resolution = temp;
                    }
                    else
                        continue;
                    // if control reaches till here, everything is 
                    //fine and can break out of loop
                    break;  
                }
                catch(Exception e)
                {
                    // in case exception thrown, prints exception message and makes beep sound
                    // changes console color to dark blue
                    Console.BackgroundColor = ConsoleColor.DarkBlue;       
                    Console.Beep();
                    Console.WriteLine(e.Message + "\n\nTRY AGIAN!!!");
                }
                // changes color to default again  before exiting the function
                Console.BackgroundColor = color;        
            }
        }
    }
}
