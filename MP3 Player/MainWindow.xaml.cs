﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();
            loadFiles("Music", "*.mp3");
            btnRandom.Background.Opacity = 0;
            btnRepeat.Background.Opacity = 0;
        }

        public void loadFiles(string folder, string fileType)
        {
            DirectoryInfo dinfo = new DirectoryInfo(folder);
            FileInfo[] Files = dinfo.GetFiles(fileType);
            foreach (FileInfo file in Files)
            {
                lbList.Items.Add(file.Name);
            }
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Audio files (*.MP3)|*.MP3;|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string source = System.IO.Path.GetFileName(dlg.FileName);
                string filePath = dlg.FileName;
                string destFile = System.IO.Path.Combine("Music", source);
                System.IO.File.Copy(filePath, destFile, true);
                lbList.Items.Add(source);
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            playerPlay();
        }

        private void playerPlay()
        {
            if (!isPlaying)
            {
                player.URL = "Music/" + lbList.SelectedItem.ToString();
                player.controls.play();
                btnPlay.Content = "Pause";
                isPlaying = true;
                player.settings.volume = (int)slVolume.Value;
                slTime.Maximum = player.currentMedia.duration;
                lblMaxTime.Content = player.currentMedia.durationString;
                player.controls.currentPosition = position;
                ticks.Interval = TimeSpan.FromMilliseconds(1);
                ticks.Tick += ticks_Tick;
                tick = new timerTick(Timer_Tick);
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
            isPlaying = false;
            position = 0;
            slTime.Value = 0;
            lblTime.Content = "00:00";
            ticks.Stop();
            btnPlay.Content = "Play";
        }

        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.settings.volume = (int)slVolume.Value;
            volume = (int)slVolume.Value;
        }

        private void btnDelFile_Click(object sender, RoutedEventArgs e)
        {
            if(player.URL == "Music/" + lbList.SelectedItem.ToString())
            {
                playerStop();
            }
            System.IO.File.Delete("Music/" + lbList.SelectedItem.ToString());
            lbList.Items.Remove(lbList.SelectedItem);
        }

        private void Timer_Tick()
        {
            slTime.Value = player.controls.currentPosition;
            lblTime.Content = player.controls.currentPositionString;
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
            playerPlay();
        }

        private void lbList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbList.SelectedItem != null)
            {
                isPlaying = false;
                position = 0;
                playerPlay();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
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

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            playerStop();
            if (isRandom)
            {
                int random = -1;
                while (lbList.SelectedIndex == random)
                    random = rand.Next(lbList.Items.Count);
                lbList.SelectedIndex = rand.Next(lbList.Items.Count);
            }
            else if (lbList.SelectedIndex > 0)
                lbList.SelectedIndex = lbList.SelectedIndex - 1;
            playerPlay();
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            if (isMuted)
            {
                slVolume.Value = volume;
                btnMute.Content = "Mute";
                isMuted = false;
            } 
            else
            {
                int vol = (int)slVolume.Value;
                slVolume.Value = 0;
                volume = vol;
                btnMute.Content = "Unmute";
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
    }

}