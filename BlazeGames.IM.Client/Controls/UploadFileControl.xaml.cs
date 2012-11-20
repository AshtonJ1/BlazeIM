using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for UploadFileControl.xaml
    /// </summary>
    public partial class UploadFileControl : UserControl
    {
        public string Url { get; set; }

        public UploadFileControl()
        {
            InitializeComponent();
        }

        public void SetProgress(int Progress)
        {
            if (Progress > 100)
                Progress = 100;

            upload_progress.Value = Progress;
            upload_progresstxt.Content = string.Format("Uploading ({0}%)...", Progress);
        }

        public void SetImage(ImageSource image)
        {
            upload__thumbnail.Source = image;
        }

        public void UploadComplete(string Url)
        {
            this.Url = Url;

            upload_progress.Visibility = System.Windows.Visibility.Hidden;
            upload_progresstxt.Content = Url;
        }
    }
}
