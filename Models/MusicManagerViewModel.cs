using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;

using System.Diagnostics;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using EXOApp.Converters;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace EXOApp.Models
{
    /// <summary>
    /// View model for Music Manager
    /// </summary>
    class MusicManagerViewModel : INotifyPropertyChanged
    {
        ObservableCollection<MusicListItem> musicTracks = new ObservableCollection<MusicListItem>();
        public ObservableCollection<MusicListItem> MusicTracks { get { return musicTracks; } }

        Models.PodModel pod;
        List<string> tracks;
        private readonly Helpers.MessageInterface _messageInterface;
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand AddMusicCommand { protected set; get; }
        public ICommand applyVolumeCommand { protected set; get; }
        public ICommand deleteMusicCommand { protected set; get; }
        public ICommand refreshDBCommand { protected set; get; }
        public ICommand ExitCommand { protected set; get; }

        public string PodNumber { get; set; }


        /// <summary>
        /// Constructor for the Music Manager
        /// </summary>
        public MusicManagerViewModel()
        {
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
            pod = Globals.getCurrentPod();
            AddMusicCommand = new Command(async () => await transferFile());
            ExitCommand = new Command(async () => await exit());
            refreshDBCommand = new Command(async () => await refreshMusicDB());
            deleteMusicCommand = new Command<string>(async (name) => await DeleteMusic(name));
            applyVolumeCommand = new Command<string>((name) => applyNewVolume(name));
            PodNumber = "EXO - " + pod.podNumber;
            foreach (MusicTrack mt in pod.musicTracks)
            {
                musicTracks.Add(new MusicListItem(mt, deleteMusicCommand, applyVolumeCommand));
            }
        }
        /// <summary>
        /// Compares the music on the Pod to the music in the Pod database. Fixes any inconsistencies
        /// </summary>
        async Task refreshMusicDB()
        {
            bool refresh = await _messageInterface.ShowAsyncAcceptCancel("Music Refresh", "This should only be used if you have previously uploaded tracks not detected.\nContinue?", "Yes", "No");
            if (!refresh)
            {
                return;
            }

            string output = await scanPod();
            tracks = output.Split('\r', '\n').ToList<string>();
            //Add new tracks to db
            foreach (string track in tracks)
            {
                bool found = false;
                foreach (MusicTrack mt in pod.musicTracks)
                {
                    if (track == mt.name || track == "")
                    {
                        found = true;
                    }

                }
                if (!found)
                {
                    Debug.WriteLine("Track not found 1st: " + track);
                    Debug.WriteLine(pod.addMusicTracks(track, premiumLevelCheck(track), "100"));
                }
            }
            //Remove missing tracks from db
            foreach (MusicTrack mt in pod.musicTracks)
            {
                bool found = false;
                foreach (string track in tracks)
                {
                    if (track == mt.name)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    pod.deleteMusicTracks(mt.name);
                }
            }
        }

        /// <summary>
        /// Gets a list of all the music tracks stored on the Pod
        /// </summary>
        /// <returns>A string containing the each music track on the Pod</returns>
        async Task<string> scanPod()
        {
            string host = pod.ipAddress.Replace("http://", "");
            host = host.Replace("/", "");
            string username = "pi";
            string password = "orbit";
            string result = "";
            await Task.Run(() =>
            {
                using (SshClient sshc = new SshClient(host, username, password))
                {
                    try
                    {
                        sshc.Connect();
                        var command = sshc.CreateCommand("ls /home/pi/Orbit/music");
                        command.Execute();
                        //sshc.RunCommand("sudo chmod -R 777 /home/pi/Orbit/music");
                        result = command.Result;
                        sshc.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("An exception has been caught " + e.ToString());
                        result = "Fail";
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// Brings up the windows dialog for picking a file
        /// </summary>
        /// <returns>The chosen file, must be an mp3 file, else returns null</returns>
        async Task<FileResult> pickAndShow()
        {
            try
            {
                var customFileType =
                    new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".mp3" } },
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Please select a music file",
                    FileTypes = customFileType,
                };
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        /// <summary>
        /// Transfer the mp3 file to the Pod, and updates the pod database accordingly.
        /// </summary>
        /// <returns></returns>
        async Task transferFile()
        {
            FileResult upload = await pickAndShow();
            if (upload == null)
            {
                return;
            }
            if (!upload.FileName.EndsWith(".mp3"))
            {
                await _messageInterface.ShowAsyncOK("Error 4100", "File type incorrect. Please upload a .mp3 file (case sensitive)", "Close");
                return;
            }
            string host = pod.ipAddress.Replace("http://", "");
            host = host.Replace("/", "");
            string username = "pi";
            string password = "orbit";

            string remoteDirectory = "/home/pi/Orbit/music";
            string trackName = upload.FileName;
            await Task.Run(async () =>
            {
                using (SftpClient sftp = new SftpClient(host, username, password))
                {
                    try
                    {

                        sftp.Connect();
                        sftp.ChangeDirectory(remoteDirectory);

                        using (var uplfileStream = await upload.OpenReadAsync())
                        {
                            sftp.UploadFile(uplfileStream, trackName, true);
                        }
                        sftp.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("An exception has been caught " + e.ToString());
                    }
                }
            });
            pod.addMusicTracks(trackName, premiumLevelCheck(trackName), "100");
            MusicTrack mt = new MusicTrack();
            mt.name = trackName;
            mt.premiumLevel = int.Parse(premiumLevelCheck(trackName));
            mt.defaultVolume = 100;
            _ = _messageInterface.ShowAsyncOK("Music Upload", "Track Upload Sucessful\nPlease Log out and log back in to refresh the dashboard", "Close");
            musicTracks.Add(new MusicListItem(mt, deleteMusicCommand, applyVolumeCommand));

        }

        /// <summary>
        /// Deletes a track on the Pod, and updates the database accordingly
        /// </summary>
        async Task DeleteMusic(string trackName)
        {
            bool delete = await _messageInterface.ShowAsyncAcceptCancel("Music Delete", "Are you sure you want to delete " + MusicNameFormatter.convertMusicName(trackName) + "?", "Yes", "No");
            if(!delete)
            {
                return;
            }

            string host = pod.ipAddress.Replace("http://", "");
            host = host.Replace("/", "");
            string username = "pi";
            string password = "orbit";
            string remoteDirectory = "/home/pi/Orbit/music/";
            await Task.Run(() =>
            {
                pod.deleteMusicTracks(trackName);
                using (SftpClient sftp = new SftpClient(host, username, password))
                {
                    try
                    {

                        sftp.Connect();
                        sftp.ChangeDirectory(remoteDirectory);
                        sftp.DeleteFile(remoteDirectory + trackName);
                        sftp.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("An exception has been caught " + e.ToString());
                    }
                }
            });
            MusicListItem deleteMt = null;
            MusicTrack toDelete = null;
            foreach (MusicListItem mt in musicTracks)
            {
                if (mt.TrackName == trackName)
                {
                    deleteMt = mt;
                    mt.musicTrack = toDelete;
                }
            }
            if (deleteMt != null)
            {
                musicTracks.Remove(deleteMt);
                pod.musicTracks.Remove(toDelete);
            }
            _ = _messageInterface.ShowAsyncOK("Music Delete", "Music Track Deleted Sucessfully\nPlease Log out and log back in to refresh the dashboard", "Close");

        }
        /// <summary>
        /// Set the an independant volume for the track
        /// </summary>
        void applyNewVolume(string trackName)
        {
            MusicTrack mt = pod.musicTracks.Find(x => x.name == trackName);
            if(mt == null)
            {
                _ = _messageInterface.ShowAsyncOK("Error 4111", "Unable to set music volume\n Please contact Wellness Support, quoting this Error Code", "Close");
                return;
            }
            
            int volume = -1;


            foreach(MusicListItem track in musicTracks)
            {
                if(trackName == track.TrackName)
                {
                    volume = track.Volume;
                }
            }
            if(volume == -1)
            {
                _ = _messageInterface.ShowAsyncOK("Error 4112", "Unable to set music volume\n Please contact Wellness Support, quoting this Error Code", "Close");
            }

            if(pod.changeTrackVolume(volume, trackName) != "\"success\"")
            {
                _ = _messageInterface.ShowAsyncOK("Error 4110", "Unable to set music volume\n Please try again and if the problem continues, contact Wellness Support", "Close");
                return;
            }
            mt.defaultVolume = volume;
            _ = _messageInterface.ShowAsyncOK("Music Volume", "New Volume Set Sucessfully", "Close");
        }

        /// <summary>
        ///  Checks the premium level of a track. Only 0 and 1 are used. Tracks with 0 cannot be deleted
        /// </summary>
        string premiumLevelCheck(string name)
        {
            if (name.StartsWith("df_"))
            {
                return "0";
            }
            else if (name.StartsWith("pd_"))
            {
                return "2";
            }
            else
            {
                return "1";
            }
        }

        /// <summary>
        /// Exits the music manager and returns to the dashboard
        /// </summary>
        async Task exit()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Internal class for handling each music track in the view
        /// </summary>
        internal class MusicListItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string TrackName { get; internal set; }
            public bool DeleteAvailable { get; internal set; }
            public int PremiumLevel { get; internal set; }
            public MusicTrack musicTrack { get; internal set; }
            public ICommand DeleteTrack { protected set; get; }
            public ICommand ApplyVolume { protected set; get; }
            int volume { get; set; }

            /// <summary>
            /// Constructor for music list items.
            /// </summary>
            public MusicListItem(MusicTrack musicTrack, ICommand deleteTrack, ICommand applyVolume)
            {
                this.musicTrack = musicTrack;
                TrackName = musicTrack.name;
                Volume = musicTrack.defaultVolume;
                PremiumLevel = musicTrack.premiumLevel;
                if(PremiumLevel == 0)
                {
                    DeleteAvailable = false;
                }
                else
                {
                    DeleteAvailable = true;
                }
                DeleteTrack = deleteTrack;
                ApplyVolume = applyVolume;
            }
            /// <summary>
            /// Sets the music track to a specified volume to be used during per track music volumes.
            /// </summary>
            public void updateValues()
            {
                Volume = musicTrack.defaultVolume;
            }

            public int Volume
            {
                set
                {
                    if (volume != value)
                    {
                        volume = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Volume"));
                        }
                    }
                }
                get
                {
                    return volume;
                }
            }

        }



    }
}
