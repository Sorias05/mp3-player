using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System.Security.Policy;
using Microsoft.Win32;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Forms;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Timers;
using System.ComponentModel;
using System.Windows.Threading;

namespace MP3_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
        public delegate void timerTick();
        DispatcherTimer ticks = new DispatcherTimer();
        timerTick tick;
        bool isPlaying = false;
        bool isMuted = false;
        bool isRepeating = false;
        bool isRandom = false;
        Random rand = new Random();
        int volume = 0;
        double position = 0;
        int activeSongIndex = -1;
        int activePlaylistIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            LoadPlaylists("Music/");

            btnRandom.Background.Opacity = 0;
            btnRepeat.Background.Opacity = 0;
            btnMute.Background.Opacity = 0;

            //lbList.Drop += lbList_Drop;
            lbList.DragEnter += lbList_DragEnter;
        }

        public void loadFiles(string folder, string fileType)
        {
            DirectoryInfo dinfo = new DirectoryInfo(folder);
            FileInfo[] Files = dinfo.GetFiles(fileType);
            lbList.Items.Clear();
            foreach (FileInfo file in Files)
                lbList.Items.Add(file.Name);
            activeSongIndex = -1;
        }

        private void LoadPlaylists(string info)
        {
            DirectoryInfo di = new DirectoryInfo(info);
            DirectoryInfo[] diArr = di.GetDirectories();
            lbPlaylist.Items.Clear();
            foreach (DirectoryInfo dri in diArr)
                lbPlaylist.Items.Add(dri.Name);
            activePlaylistIndex = -1;
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            if(lbPlaylist.SelectedItem != null)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Audio files (*.MP3)|*.MP3;|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string source = System.IO.Path.GetFileName(dlg.FileName);
                    string filePath = dlg.FileName;
                    string destFile = System.IO.Path.Combine($"Music/{lbPlaylist.SelectedItem.ToString()}", source);
                    System.IO.File.Copy(filePath, destFile, true);
                    lbList.Items.Add(source);
                }
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if(lbList.SelectedItem != null)
            {
                playerPlay();
            }
        }

        private void playerPlay()
        {
            if (!isPlaying)
            {
                player.URL = $"Music/{lbPlaylist.SelectedItem.ToString()}/{lbList.SelectedItem.ToString()}";
                player.settings.volume = (int)slVolume.Value;
                player.controls.play();
                player.controls.currentPosition = position;

                activeSongIndex = lbList.SelectedIndex;
                btnPlay.Content = "Pause";
                isPlaying = true;
                ticks.Interval = TimeSpan.FromMilliseconds(1);
                ticks.Tick += ticks_Tick;
                tick = new timerTick(Timer_Tick);
                System.Threading.Thread.Sleep(100);
                ticks.Start();
            }
            else
            {
                player.controls.pause();
                btnPlay.Content = "Play";
                isPlaying = false;
                position = (double)player.controls.currentPosition;
                ticks.Stop();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            playerStop();
        }

        private void playerStop()
        {
            player.controls.stop();
            ticks.Stop();
            isPlaying = false;
            position = 0;
            slTime.Value = 0;
            lblTime.Content = "00:00";
            lblMaxTime.Content = "00:00";
            btnPlay.Content = "Play";
        }

        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.settings.volume = (int)slVolume.Value;
            volume = (int)slVolume.Value;
        }

        private void btnDelFile_Click(object sender, RoutedEventArgs e)
        {
            if(lbList.SelectedItem != null)
            {
                if (System.Windows.Forms.MessageBox.Show("Are you sure?", $"Delete {lbList.SelectedItem.ToString()}", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    if (lbList.SelectedIndex == activeSongIndex)
                        playerStop();
                    System.IO.File.Delete($"Music/{lbPlaylist.SelectedItem.ToString()}/{lbList.SelectedItem.ToString()}");
                    lbList.Items.Remove(lbList.SelectedItem);
                }
            }
        }

        private void Timer_Tick()
        {
            slTime.Value = player.controls.currentPosition;
            lblTime.Content = player.controls.currentPositionString;
            slTime.Maximum = player.currentMedia.duration;
            lblMaxTime.Content = player.currentMedia.durationString;
            if (player.controls.currentPosition >= player.currentMedia.duration - 0.5)
            {
                playerStop();
                if (isRandom)
                {
                    int random = -1;
                    while(lbList.SelectedIndex == random)
                        random = rand.Next(lbList.Items.Count);
                    lbList.SelectedIndex = rand.Next(lbList.Items.Count);
                }
                else if (lbList.SelectedIndex < lbList.Items.Count - 1)
                    lbList.SelectedIndex = lbList.SelectedIndex + 1;
                else if (isRepeating)
                    lbList.SelectedIndex = 0;
                playerPlay();
            }
        }

        void ticks_Tick(object sender, object e)
        {
            Dispatcher.Invoke(tick);
        }

        private void slTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            position = slTime.Value;
            playerPlay();
        }

        private void slTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isPlaying = true;
            playerPlay();
        }

        private void lbList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbList.SelectedItem != null)
            {
                isPlaying = false;
                position = 0;
                ticks.Stop();
                playerPlay();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (lbList.SelectedItem != null)
            {
                playerStop();
                if (isRandom)
                {
                    int random = -1;
                    while (lbList.SelectedIndex == random)
                        random = rand.Next(lbList.Items.Count);
                    lbList.SelectedIndex = rand.Next(lbList.Items.Count);
                }
                else if (lbList.SelectedIndex < lbList.Items.Count - 1)
                    lbList.SelectedIndex = lbList.SelectedIndex + 1;
                else if (isRepeating)
                    lbList.SelectedIndex = 0;
                playerPlay();
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (lbList.SelectedItem != null)
            {
                playerStop();
                if (isRandom)
                    lbList.SelectedIndex = activeSongIndex;
                else if (lbList.SelectedIndex > 0)
                    lbList.SelectedIndex = lbList.SelectedIndex - 1;
                playerPlay();
            }    
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            if (isMuted)
            {
                slVolume.Value = volume;
                btnMute.Content = "Mute";
                btnMute.Background.Opacity = 0;
                isMuted = false;
            } 
            else
            {
                int vol = (int)slVolume.Value;
                slVolume.Value = 0;
                volume = vol;
                btnMute.Content = "Unmute";
                btnMute.Background.Opacity = 100;
                isMuted = true;
            }
        }

        private void btnRepeat_Click(object sender, RoutedEventArgs e)
        {
            if (isRepeating)
            {
                btnRepeat.Background.Opacity = 0; 
                isRepeating = false;
            }
            else
            {
                btnRepeat.Background.Opacity = 100;
                isRepeating = true;
            }   
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            if (isRandom)
            {
                btnRandom.Background.Opacity = 0;
                isRandom = false;
            }
            else
            {
                btnRandom.Background.Opacity = 100;
                isRandom = true;
            }
        }

        private void lbPlaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbPlaylist.SelectedItem != null)
            {
                playerStop();
                loadFiles($"Music/{lbPlaylist.SelectedItem.ToString()}", "*.mp3");
                activePlaylistIndex = lbPlaylist.SelectedIndex;
                activeSongIndex = -1;
                lblMaxTime.Content = "00:00";
            }
        }

        private void btnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylistWindow addPlaylist = new AddPlaylistWindow();
            addPlaylist.DataChanged += AddPlaylistWindow_DataChanged;
            addPlaylist.Show();
        }

        private void btnDelPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(lbPlaylist.SelectedItem != null)
            {
                if (System.Windows.Forms.MessageBox.Show("Are you sure?", $"Delete {lbPlaylist.SelectedItem.ToString()}", System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo($"Music/{lbPlaylist.SelectedItem.ToString()}");
                    foreach (FileInfo file in di.GetFiles())
                        file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories())
                        dir.Delete(true);
                    if (lbPlaylist.SelectedIndex == activePlaylistIndex)
                    {
                        playerStop();
                        lbList.Items.Clear();
                    }

                    Directory.Delete($"Music/{lbPlaylist.SelectedItem.ToString()}", false);
                    LoadPlaylists("Music/");
                }
            }
        }

        private void AddPlaylistWindow_DataChanged(object sender, EventArgs e)
        {
            LoadPlaylists("Music/");
        }

        private void lbList_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) e.Effects = System.Windows.DragDropEffects.Copy;
        }

        private void lbList_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (activePlaylistIndex != -1)
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (System.IO.Path.GetExtension(file).ToLower() == ".mp3")
                    {
                        string source = System.IO.Path.GetFileName(file);
                        string filePath = file;
                        string destFile = System.IO.Path.Combine($"Music/{lbPlaylist.SelectedItem.ToString()}", source);
                        System.IO.File.Copy(filePath, destFile, true);
                        lbList.Items.Add(source);
                    }
                }
            }
        }

        private void slTime_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ticks.Stop();
            player.controls.pause();
        }

        private void slTime_MouseUp(object sender, MouseButtonEventArgs e)
        {
            position = slTime.Value;
            isPlaying = false;
            playerPlay();
        }
    }
}
