using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace testYoutubeDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            //SaveVideoToDiskAsync("https://www.youtube.com/watch?v=PT2_F-1esPk");
        }

        public async void SaveVideoToDiskAsync(string link)
        {
            Debug.WriteLine("started: " + DateTime.Now.ToString());
            bool isVideo = downloadType.IsOn;
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(link);
            var vidBytes = video.GetBytesAsync();
            try
            {
                StorageFile videoFile = await DownloadsFolder.CreateFileAsync(video.FullName);
                await FileIO.WriteBytesAsync(videoFile, await vidBytes);
                if (!isVideo)
                {
                    StorageFile audioFile = await DownloadsFolder.CreateFileAsync(video.Title + ".mp3");
                    MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
                    MediaTranscoder transcoder = new MediaTranscoder();
                    PrepareTranscodeResult prepTrans = await transcoder.PrepareFileTranscodeAsync(videoFile, audioFile, MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High));
                    if (prepTrans.CanTranscode)
                    {
                        IAsyncActionWithProgress<double> TranscodeTask =  prepTrans.TranscodeAsync();
                        TranscodeTask.Completed += ((IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status) => videoFile.DeleteAsync());
                    }
                    else
                    {
                        Debug.WriteLine(prepTrans.FailureReason.ToString());
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public async void MediaEncoder(IStorageFile source, IStorageFile destination, MediaEncodingProfile profile)
        {

            MediaTranscoder transcoder = new MediaTranscoder();

            PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(source, destination, profile);

            if (prepareOp.CanTranscode)
            {
                var transcodeOp = prepareOp.TranscodeAsync();
            }
            else
            {
                switch (prepareOp.FailureReason)
                {
                    case TranscodeFailureReason.CodecNotFound:
                        System.Diagnostics.Debug.WriteLine("Codec not found.");
                        break;
                    case TranscodeFailureReason.InvalidProfile:
                        System.Diagnostics.Debug.WriteLine("Invalid profile.");
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unknown failure.");
                        break;
                }
            }
        }

        private void click(object sender, RoutedEventArgs e)
        {
            SaveVideoToDiskAsync(downloadurl.Text);
        }
    }
}
