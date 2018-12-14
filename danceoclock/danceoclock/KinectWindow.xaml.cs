using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.Kinect;
using System.IO;
using System.Threading.Tasks;
using WMPLib;

namespace danceoclock
{

    public partial class KinectWindow : Window
    {
        // set up kinect
        KinectSensor Sensor;
        MultiSourceFrameReader Reader;
        public IList<Body> Bodies; // list of bodies detected
        public  Gesture newGesture = null;

        WindowsMediaPlayer Player1 = new WindowsMediaPlayer();
        WindowsMediaPlayer Player2 = new WindowsMediaPlayer();
        WindowsMediaPlayer Player3 = new WindowsMediaPlayer();

        int phase = 1; //section in the piece: 1 = forest
        string play2mode = "leaves1"; // first sound to play is leaves1

        // constructor for alarm mode, gestNamesList (list of names of all gestures used in order) comes from UI main
        public KinectWindow()
        {
            // Window style
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.SingleBorderWindow;

            InitializeComponent();

        }

        private void Player_PlayStateChange(int currentState)
        {
            if ((WMPPlayState)currentState == WMPPlayState.wmppsStopped)
            {
                //Player.controls.play();
            }
        }

        // specifically for player1: move on to next phase
        private void Player1_PlayStateChange(int currentState)
        {
            if ((WMPPlayState)currentState == WMPPlayState.wmppsStopped)
            {
                phase++;
                Console.WriteLine(phase);
            }
        }

        // specifically for player1: move on to next phase
        private void Playerfinal_PlayStateChange(int currentState)
        {
            if ((WMPPlayState)currentState == WMPPlayState.wmppsStopped)
            {
                Close();
            }
        }

        // when loading window, set up sensor
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Sensor = KinectSensor.GetDefault();

            if (Sensor != null)
            {
                Sensor.Open();

                Reader = Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                Reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }

        }

        // when closing window
        private void Window_Closed(object sender, EventArgs e)
        {
            // Close stuff for safety
            if (Reader != null) Reader.Dispose();
            if (Sensor != null) Sensor.Close();
        }

        // method for reading information from the sensor
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
        
            // Displaying the camera feed via color frame
            // comment out this part to only display skeleton for displaying movement instructions
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    camera.Source = frame.ToBitmap();
                }
            }

            // Displaying the skeleton via the body frame
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();
                    
                    Bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(Bodies);

                    foreach (var body in Bodies)
                    {
                        if(newGesture == null)
                        {
                            newGesture = new Gesture(body);
                        }
                        if (body != null)
                        {
                            
                            if (body.IsTracked)
                            {
                                canvas.DrawSkeleton(body, Colors.DarkBlue);
                            }

                            // current angles
                            List<double> Current = NextFrame(body);

                            //TODO: case switch for different sections that use players differently
                            if (phase == 1)
                            {
                                // trigger first backing track - forest - right arm above left

                                if (Current[2] > Current[1] + .3 && Player1.playState != WMPPlayState.wmppsPlaying && Player1.playState != WMPPlayState.wmppsTransitioning)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player1.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player1_PlayStateChange);
                                        Player1.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//explorality_back_1.mp3";
                                        Player1.controls.play();
                                    });
                                }

                                // trigger leaves sounds - right hand above head
                                if (Current[2] > Current[0] && Player2.playState != WMPPlayState.wmppsPlaying && Player2.playState != WMPPlayState.wmppsTransitioning)
                                {
                                    if (play2mode == "leaves1")
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            Player2.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                                            Player2.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//leaves1.mp3";
                                            Player2.controls.play();
                                            System.Threading.Thread.Sleep(2000);
                                            play2mode = "leaves2";
                                        });

                                    }
                                    else if (play2mode == "leaves2")
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            Player2.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                                            Player2.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//leaves2.mp3";
                                            Player2.controls.play();
                                        });
                                    }
                                }


                                // trigger footstep sounds - right foot above left
                                if (Current[4] > Current[3] + .1 && Player3.playState != WMPPlayState.wmppsPlaying)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player3.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                                        Player3.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//footstep.mp3";
                                        Player3.controls.play();
                                    });
                                }

                            }

                            else if (phase == 2)
                            {
                                // second backing track: cave - head below left elbow
                                if (Current[0] < Current[5] && Player1.playState != WMPPlayState.wmppsPlaying && Player1.playState != WMPPlayState.wmppsTransitioning)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player1.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player1_PlayStateChange);
                                        Player1.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//explorality_back_2.mp3";
                                        Player1.controls.play();
                                    });
                                }

                                // bats - right hand above head
                                if (play2mode != "nobats" && Current[2] > Current[0] && Player2.playState != WMPPlayState.wmppsPlaying && Player2.playState != WMPPlayState.wmppsTransitioning)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player2.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                                        Player2.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//bats.mp3";
                                        Player2.controls.play();
                                        play2mode = "nobats";
                                    });
                                }

                                // elevator - both hands over head
                                if (Current[0] < Current[1] && Current[0] < Current[2] && Player2.playState != WMPPlayState.wmppsPlaying && Player2.playState != WMPPlayState.wmppsTransitioning)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player2.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                                        Player2.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//elevator.mp3";
                                        Player2.controls.play();
                                    });
                                }
                            }

                            else if (phase >= 3)
                            {
                                // third backing track: high notes - right hand above head
                                if (Current[2] > Current[0] && Player1.playState != WMPPlayState.wmppsPlaying && Player1.playState != WMPPlayState.wmppsTransitioning)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player1.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Player_PlayStateChange);
                                        Player1.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//explorality_back_3.mp3";
                                        Player1.controls.play();
                                    });
                                    Console.WriteLine("played");
                                }

                                // beats: left foot above right foot 
                                if (Current[3] > Current[4] + .1 && Player2.playState != WMPPlayState.wmppsPlaying)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        Player2.PlayStateChange += new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(Playerfinal_PlayStateChange);
                                        Player2.URL = "C://Users//shanali//Desktop//Tech101-Final-Project//techtracks//beats.mp3";
                                        Player2.controls.play();
                                    });
                                }
                            }


                            //Console.WriteLine(Player1.playState);
                        }
                    }
                }
            }
        }

        // method for calculating the dot product of 2 vectors
        private static double Dot(double x1, double y1, double x2, double y2)
        {
            return x1 * x2 + y1 * y2;
        }

        // method for calculating angle between two vectors
        private static double Angle(double x1, double y1, double x2, double y2)
        {
            // check for zero vector
            if ((x1 == 0 && y1 == 0) || (x2 == 0 && y2 == 0))
            {
                return 0;
            }
            else
            {
                double theta = Math.Round((180 / Math.PI) * Math.Acos((double)Dot(x1, y1, x2, y2) /
                    (double)(Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2)) *
                    (Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2))))));

                // check for obtuse angle
                if (Dot(x1, y1, x2, y2) < 0)
                {
                    theta = 360 - theta;
                }

                return theta;
            }
        }

        // method for taking 3 joints and calculating the angle between them
        private static double JointsAngle(Joint a, Joint b, Joint c)
        {
            // first vector b->a
            double x1 = a.Position.X - b.Position.X;
            double y1 = a.Position.Y - b.Position.Y;

            // second vector b->c
            double x2 = c.Position.X - b.Position.X;
            double y2 = c.Position.Y - b.Position.Y;

            // return angle
            return Angle(x1, y1, x2, y2);
        }

        // collect information for the next frame
        public static List<double> NextFrame(Body body)
        {
            List<double> CurrentData = new List<double>();

            // calculate and add data


            // head height 0
            CurrentData.Add(body.Joints[JointType.Head].Position.Y);

            // left hand height 1
            CurrentData.Add(body.Joints[JointType.HandLeft].Position.Y);

            // right hand height 2
            CurrentData.Add(body.Joints[JointType.HandRight].Position.Y);

            // left foot height 3
            CurrentData.Add(body.Joints[JointType.FootLeft].Position.Y);

            // right foot height 4
            CurrentData.Add(body.Joints[JointType.FootRight].Position.Y);

            // left elbow height 5
            CurrentData.Add(body.Joints[JointType.ElbowLeft].Position.Y);

            /*
            // shoulderleftangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.Neck], body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]));

            // shoulderrightangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.Neck], body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight]));

            // elbowleftangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]));

            // elbowrightangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight], body.Joints[JointType.WristRight]));

            // spineangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.Neck], body.Joints[JointType.SpineBase], body.Joints[JointType.HipLeft]));

            // hipleftangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.KneeLeft], body.Joints[JointType.HipLeft], body.Joints[JointType.SpineBase]));

            // hiprightangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.KneeRight], body.Joints[JointType.HipRight], body.Joints[JointType.SpineBase]));

            // kneeleftangle 
            CurrentData.Add(JointsAngle(body.Joints[JointType.HipLeft], body.Joints[JointType.KneeLeft], body.Joints[JointType.AnkleLeft]));

            // kneerightangle
            CurrentData.Add(JointsAngle(body.Joints[JointType.HipRight], body.Joints[JointType.KneeRight], body.Joints[JointType.AnkleRight]));
            */

            return CurrentData;
        }
    }
}
