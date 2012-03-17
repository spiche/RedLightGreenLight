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

using Microsoft.Kinect;
using System.Timers;
using System.Windows.Threading;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor _sensor;
        static string color;
        static Timer timer;
        static Random random;
        static int rand;
        DispatcherTimer timer2;
        int t;
        int closestP1;
        int closestP2;
        static bool turnedRed;
        static bool reallyRed;
        static Timer redTimer;


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];

                if (_sensor.Status == KinectStatus.Connected)
                {
                    _sensor.ColorStream.Enable();
                    _sensor.DepthStream.Enable();
                    _sensor.SkeletonStream.Enable();
                    _sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
                    _sensor.Start();
                }

            }

            random = new Random();
            reallyRed = false;
            
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(timing);

            color = "red";

            t = 4;  //t is a variable storing the number of second left to count down...
            timer2 = new DispatcherTimer();
            timer2.Tick += new EventHandler(timer_Tick);
            timer2.Interval = new TimeSpan(0, 0, 1);  //this is (hours,minutes,seconds) format
            timer2.Start();
            label1.Content = "";

            //redTimer = new Timer(500);
            //timer.Elapsed += new ElapsedEventHandler(flagRed);

        }

        void timer_Tick(object sender, EventArgs e)
        {
            label1.Background = Brushes.Black;
            label1.Content = "Starting in... " + t;
            if (t > 0)
            {
                t--;
            }
            else
            {
                timer2.Stop();
                label1.Content = "";
                label1.Background = Brushes.Transparent;
                timer.Start();
            }
        }

        private static void timing(object source, ElapsedEventArgs e)
        {
            timer.Stop();
            rand = random.Next(3000, 7000);
            timer.Interval = rand;

            if (color == "red")
            {
                color = "green";
            }
            else
            {
                color = "red";
                turnedRed = true;
            }

            timer.Start();
        }

        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            if (turnedRed)
            {
                closestP1 = 9000;
                closestP2 = 9000;

                for (int depthIndex = 0, colorIndex = 0;
                     depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                     depthIndex++, colorIndex += 4)
                {
                    int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                    int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    if (player == 1 && depth < closestP1)
                    {
                        closestP1 = depth;
                    }
                    else if (player == 2 && depth < closestP1)
                    {
                        closestP2 = depth;
                    }
                }
                turnedRed = false;

            }
            else
            {
                for (int depthIndex = 0, colorIndex = 0;
                    depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                    depthIndex++, colorIndex += 4)
                {
                    int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                    int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    if (player == 1)
                    {
                        pixels[colorIndex + BlueIndex] = 255;
                        pixels[colorIndex + GreenIndex] = 255;
                        pixels[colorIndex + RedIndex] = 255;

                    }
                    else if (player == 2)
                    {
                        pixels[colorIndex + BlueIndex] = 0;
                        pixels[colorIndex + GreenIndex] = 0;
                        pixels[colorIndex + RedIndex] = 255;
                    }
                    else
                    {
                        pixels[colorIndex + BlueIndex] = 0;
                        pixels[colorIndex + GreenIndex] = 0;
                        pixels[colorIndex + RedIndex] = 0;

                    }

                    if (color == "red")
                    {
                        //if (player == 1 && depth < closestP1 + 500)
                        //{
                        //    label1.Background = Brushes.Black;
                        //    label1.Content = "loser...";
                        //    timer.Stop();
                        //}
                        //if (player == 2 && depth < closestP2 + 500)
                        //{
                        //    label1.Background = Brushes.Black;
                        //    label1.Content = "loser...";
                        //    timer.Stop();
                        //}
                    }
                    else
                    {

                        if (player == 1 && depth < 900)
                        {
                            label1.Background = Brushes.HotPink;
                            label1.Content = "PLAYER 1 WINS!!!";
                            timer.Stop();
                        }
                        else if (player == 2 && depth < 900)
                        {
                            label1.Background = Brushes.Blue;
                            label1.Content = "PLAYER 2 WINS!!!";
                            timer.Stop();
                        }
                    }
                }
            }
            return pixels;
        }

        private byte[] RedGreen(byte[] pixels)
        {
            byte[] frame = new byte[pixels.Length];
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            if (color == "red")
            {
                for (int idx = 0; idx < frame.Length; idx += 4)
                {
                    frame[idx + BlueIndex] = 0;
                    frame[idx + GreenIndex] = 0;
                    frame[idx + RedIndex] = pixels[idx + RedIndex];
                }
            }
            else
            {
                for (int idx = 0; idx < frame.Length; idx += 4)
                {
                    frame[idx + BlueIndex] = 0;
                    frame[idx + GreenIndex] = pixels[idx + GreenIndex];
                    frame[idx + RedIndex] = 0;
                }
            }

            return frame;
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }

                byte[] pixels = new byte[colorFrame.PixelDataLength];

                //copy data out into our byte array
                colorFrame.CopyPixelDataTo(pixels);

                byte[] pixels2 = RedGreen(pixels);
                int stride = colorFrame.Width * 4;
                if (color == "red" || color == "green")
                {
                    image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height,
                        96, 96, PixelFormats.Bgr32, null, pixels2, stride);
                }
                else
                {
                    image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height,
                    96, 96, PixelFormats.Bgr32, null, pixels, stride);
                }

            }


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                byte[] pixels = GenerateColoredBytes(depthFrame);
                int stride = depthFrame.Width * 4;

                image2.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                    96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(_sensor);
        }

    }

}


//if (depth <= 900)
//{
//    pixels[colorIndex + BlueIndex] = 255;
//    pixels[colorIndex + GreenIndex] = 0;
//    pixels[colorIndex + RedIndex] = 0;
//}
//else if (depth < 4000)
//{
//    pixels[colorIndex + BlueIndex] = 0;
//    pixels[colorIndex + GreenIndex] = 255;
//    pixels[colorIndex + RedIndex] = 0;
//}
//else
//{
//    pixels[colorIndex + BlueIndex] = 0;
//    pixels[colorIndex + GreenIndex] = 0;
//    pixels[colorIndex + RedIndex] = 255;
//}