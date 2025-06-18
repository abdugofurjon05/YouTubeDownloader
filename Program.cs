using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var youtube = new YoutubeClient();
        var videoUrl = "https://youtu.be/0Cvzt9crhBs";

        try
        {
            // Get video metadata
            Console.WriteLine("Fetching video metadata...");
            var video = await youtube.Videos.GetAsync(videoUrl);
            Console.WriteLine($"Video Title: {video.Title}");
            Console.WriteLine($"Video Duration: {video.Duration}");

            // Get available streams
            Console.WriteLine("Fetching stream manifest...");
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
            var muxedStreams = streamManifest.GetMuxedStreams().ToList();

            if (muxedStreams.Any())
            {
                var streamInfo = muxedStreams.GetWithHighestVideoQuality();
                Console.WriteLine($"Selected stream:{streamInfo.VideoQuality}, Size: {streamInfo.Size}");

                // Generate a safe file name
                var invalidChars = Path.GetInvalidFileNameChars();
                var safeTitle = string.Concat(video.Title.Select(c => invalidChars.Contains(c) ? '_' : c));
                var fileName = $"{safeTitle}.mp4";

                Console.WriteLine($"Downloading to: {fileName}");
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName);
                Console.WriteLine("Download completed!");
            }
            else
            {
                Console.WriteLine("No muxed streams available for this video.");
            }

            // Optionally download video and audio streams separately
            var videoStream = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();
            var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();

            if (videoStream != null && audioStream != null)
            {
                Console.WriteLine("Downloading video and audio streams separately...");
                await youtube.Videos.Streams.DownloadAsync(videoStream, "video.mp4");
                await youtube.Videos.Streams.DownloadAsync(audioStream, "audio.mp3");
                Console.WriteLine("Download completed! (You will need to merge video and audio manually)");
            }
            else
            {
                Console.WriteLine("No suitable video or audio streams found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}