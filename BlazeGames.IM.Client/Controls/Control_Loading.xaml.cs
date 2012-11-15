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

namespace BlazeGames.IM.Client
{
    /// <summary>
    /// Interaction logic for Control_Loading.xaml
    /// </summary>
    internal partial class Control_Loading : UserControl
    {
        public Control_Loading()
        {
            InitializeComponent();
        }

        private double OffsetSecondsValue;

        /// <summary>
        /// The number of seconds from the control being loaded that the animation should 

        /// be offset. This defaults to zero, meaning the animation will start as soon as 

        /// the control is loaded.
        /// </summary> 
        public double OffsetSeconds
        {

            get
            {
                return this.OffsetSecondsValue;
            }
            set
            {
                this.OffsetSecondsValue = value;

                this.OffsetKeyFrameKeyTimes();

            }

        }

        public Brush Color
        {
            get
            {
                return this.Rectangle.Fill;
            }
            set
            {
                this.Rectangle.Fill = value;
            }
        }

        /// <summary>

        /// Offsets the four keyframes of the animation with the set offset value. This 
        /// allows rectangles to be visually staggered if more than one are being used together.

        /// </summary>

        private void OffsetKeyFrameKeyTimes()
        {
            TimeSpan offset = TimeSpan.FromSeconds(OffsetSeconds);
            KeyFrame1.KeyTime = KeyFrame1.KeyTime.TimeSpan.Add(offset);
            KeyFrame2.KeyTime = KeyFrame2.KeyTime.TimeSpan.Add(offset);
            KeyFrame3.KeyTime = KeyFrame3.KeyTime.TimeSpan.Add(offset);
            KeyFrame4.KeyTime = KeyFrame4.KeyTime.TimeSpan.Add(offset);
        }

        private void UserControl_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                KeyFrame2.Value = this.ActualWidth / 3;
                KeyFrame3.Value = KeyFrame2.Value * 2;
                KeyFrame4.Value = this.ActualWidth + 10;
                KeyFrame5.Value = this.ActualWidth + 10;
            }
        }

        private void UserControl_IsVisibleChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Hidden)
                storyboard.Stop();
            else
            {
                storyboard.Stop();
                storyboard.Begin();
            }
        }
    }
}
