using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MP3_Player
{
    /// <summary>
    /// Interaction logic for AddPlaylistWindow.xaml
    /// </summary>
    public partial class AddPlaylistWindow : Window
    {
        public delegate void DataChangedEventHandler(object sender, EventArgs e);

        public event DataChangedEventHandler DataChanged;
        public AddPlaylistWindow()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Length < 3)
                System.Windows.MessageBox.Show("Name should contain minimum 3 symbols!");
            else
            {
                if (!Directory.Exists($"Music/{txtName.Text}"))
                {
                    Directory.CreateDirectory($"Music/{txtName.Text}");
                    DataChangedEventHandler handler = DataChanged;
                    if (handler != null)
                        handler(this, new EventArgs());
                    Close();
                }
                else
                    System.Windows.MessageBox.Show("Playlist with this name is already exists!");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
