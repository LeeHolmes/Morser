using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace Morser
{
    public partial class MorserUi : Form
    {
        InterceptKeys keyCapture;
        private Keys appKey = Keys.Control | Keys.Alt | Keys.M;
        private bool listening = false;

        List<double> trainingTimes = null;
        private bool training = false;

        public delegate void SendInput(string input);
        public SendInput inputDelegate;

        WaveIn waveInStream;
        private bool recordingKeyPress = false;
        private bool recordingStream = false;
        double lowestVolume = Double.MaxValue;
        double highestVolume = Double.MinValue;
        bool isExiting = false;
        object exitingLock = new Object();

        int codexUnits = 50;
        int WPM = 15;
        int ditMilliseconds;
        int dahMilliseconds;
        int interCharacterMilliseconds;
        int interWordMilliseconds;

        DateTime pressStarted;
        string charactersPressed;
        Timer monitorTimer;
        Timer wordTimer;

        Dictionary<string, string> morseMap;

        public MorserUi()
        {
            InitializeComponent();

            keyCapture = new InterceptKeys(this);
            inputDelegate = new SendInput(SendKey);

            // Register Control+Alt+M to toggle activation of this app
            keyCapture.RegisterGlobalHotKey(appKey);
            keyCapture.KeyDown += keyCapture_KeyDown;
            keyCapture.KeyUp += keyCapture_KeyUp;

            morseMap = GetMorseMap();

            charactersPressed = "";
            monitorTimer = new Timer();
            monitorTimer.Tick += new EventHandler(monitorTimer_Tick);

            wordTimer = new Timer();
            wordTimer.Tick += new EventHandler(wordTimer_Tick);

            CalculateTimesForWpm();

            waveInStream = new WaveIn();
            waveInStream.BufferMilliseconds = 20;
            waveInStream.DataAvailable += new EventHandler<WaveInEventArgs>(waveInStream_DataAvailable);

            this.Closing += new CancelEventHandler(OnClose);
        }

        private void CalculateTimesForWpm()
        {
            wpmTrackBar.Value = WPM;
            currentWpm.Text = WPM.ToString();

            double totalUnits = WPM * codexUnits;
            double unit = (int)((1000.0 * 60.0) / totalUnits);

            ditMilliseconds = (int) unit;
            dahMilliseconds = (int) (3.0 * unit);
            interCharacterMilliseconds = (int) (2.0 * unit);
            interWordMilliseconds = (int)(3.0 * unit);

            monitorTimer.Interval = (int) (interCharacterMilliseconds * 0.9);
            wordTimer.Interval = (int) (interWordMilliseconds * 0.9);
        }

        void OnClose(Object sender, CancelEventArgs e)
        {
            isExiting = true;

            if (recordingStream)
            {
                waveInStream.StopRecording();
            }
            waveInStream.Dispose();
            waveInStream = null;
        }

        void monitorTimer_Tick(object sender, EventArgs e)
        {
            monitorTimer.Stop();
            System.Diagnostics.Debug.WriteLine("Monitor Timer Elapsed");

            if (charactersPressed.Length > 0)
            {
                if (morseMap.ContainsKey(charactersPressed))
                {
                    this.Invoke(inputDelegate, morseMap[charactersPressed]);
                }
                System.Diagnostics.Debug.WriteLine("Pressed: " + charactersPressed);
                AppendRecentText(" ");

                if (autoSpace.Checked)
                {
                    wordTimer.Start();
                }
            }

            charactersPressed = "";
        }

        void wordTimer_Tick(object sender, EventArgs e)
        {
            wordTimer.Stop();
            System.Diagnostics.Debug.WriteLine("Word Timer Elapsed");
            
            AppendRecentText("/");
            this.Invoke(inputDelegate, " ");
        }

        void waveInStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (isExiting) { return; }

            if ((this.cbListening.Checked) && (this.listening || this.training))
            {
                byte[] buffer = e.Buffer;

                double peakVolume = 0;
                for (int counter = 0; counter < e.BytesRecorded; counter += 2)
                {
                    short sound = BitConverter.ToInt16(buffer, counter);
                    if (sound != 0)
                    {
                        sound = Convert.ToInt16(~sound | 1);
                    }
                    double sample = Math.Abs( sound );
                    if (sample > peakVolume)
                    {
                        peakVolume = sample;
                    }
                }

                double totalVolume = peakVolume;

                if (totalVolume < lowestVolume)
                {
                    lowestVolume = totalVolume;
                }
                if (totalVolume > highestVolume)
                {
                    double oldRange = highestVolume - lowestVolume;
                    highestVolume = totalVolume;
                    double newRange = highestVolume - lowestVolume;

                    if ((oldRange > 0.1) && (newRange > 0.1))
                    {
                        volumeThreshold.Value = (int)((double)volumeThreshold.Value * (oldRange / newRange));
                    }
                }

                audioVolumePicture.Height = (int)(((totalVolume - lowestVolume) / (highestVolume - lowestVolume)) * (audioPanel.Height));
                if ((audioVolumePicture.Height * 100 / audioPanel.Height) > volumeThreshold.Value)
                {
                    if (!this.recordingKeyPress)
                    {
                        System.Diagnostics.Debug.WriteLine("Sound START: " + totalVolume);

                        RecordMorseStart();
                        this.recordingKeyPress = true;
                    }
                }
                else
                {
                    if (this.recordingKeyPress)
                    {
                        System.Diagnostics.Debug.WriteLine("Sound STOP: " + totalVolume);
                        RecordMorseStop();
                        this.recordingKeyPress = false;
                    }
                }
            }
        }

        static void SendKey(string input)
        {
            SendKeys.SendWait(input);
        }

        // Handle KEYUP messages
        void keyCapture_KeyUp(object sender, InterceptKeys.KeyChangeEventArgs e)
        {
            if (this.listening || this.training)
            {
                if (e.VKCode == 0x28)
                {
                    e.Cancel = true;
                    System.Diagnostics.Debug.WriteLine("Key UP");

                    RecordMorseStop();
                }
            }
        }

        // Capture KEYDOWN messages
        void keyCapture_KeyDown(object sender, InterceptKeys.KeyChangeEventArgs e)
        {
            if (this.listening || this.training)
            {
                if (e.VKCode == 0x28)
                {
                    e.Cancel = true;
                    System.Diagnostics.Debug.WriteLine("Key DOWN");
                    RecordMorseStart();
                }
            }
        }

        private void RecordMorseStop()
        {
            DateTime pressEnded = DateTime.Now;
            TimeSpan pressTime = pressEnded - pressStarted;
            System.Diagnostics.Debug.WriteLine("Morse STOP. MS: " + pressTime.TotalMilliseconds);

            if (this.training)
            {
                this.trainingTimes.Add(pressTime.TotalMilliseconds);
            }
            else
            {
                // A dit is 3 times a dah. So assuming the middle-ground gives
                // us a safe cut point.
                
                if (pressTime.TotalMilliseconds < ((ditMilliseconds + dahMilliseconds) / 2))
                {
                    charactersPressed += ".";
                    AppendRecentText(".");
                }
                else
                {
                    charactersPressed += "-";
                    AppendRecentText("-");
                }
            }

            System.Diagnostics.Debug.WriteLine("Monitor Timer Started");
            monitorTimer.Start();
        }

        private void RecordMorseStart()
        {
            System.Diagnostics.Debug.WriteLine("Morse START");

            pressStarted = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Monitor Timer Stopped");
            monitorTimer.Stop();
            wordTimer.Stop();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // let the base class process the message
            base.WndProc(ref m);

            // if this is a WM_HOTKEY message, notify the parent object
            const int WM_HOTKEY = 0x312;
            if (m.Msg == WM_HOTKEY)
            {
                if (keyCapture.IsMessageForHotkey((short)m.WParam, appKey))
                {
                    if (this.listening)
                    {
                        this.Text = "Morser - Press Control-Alt-M to enable.";

                        this.cbListening.Checked = false;
                        this.cbListening.Enabled = false;
                        recordingStream = false;
                        waveInStream.StopRecording();
                    }
                    else
                    {
                        this.Text = "Morser - Press Control-Alt-M to disable.";
                        
                        waveInStream.StartRecording();
                        recordingStream = true;
                        this.cbListening.Enabled = true;
                        this.cbListening.Checked = true;
                    }

                    this.listening = !this.listening;
                }
            }
        }

        private void AppendRecentText(string text)
        {
            if (recentText.Text.Length + text.Length > 80)
            {
                recentText.Text = recentText.Text.Substring(text.Length);
            }
            recentText.Text += text;
        }

        private Dictionary<string, string> GetMorseMap()
        {
            Dictionary<string, string> _codes = new Dictionary<string, string>();
            _codes.Add(".-", "a");
            _codes.Add("-...", "b");
            _codes.Add("-.-.", "c");
            _codes.Add("-..", "d");
            _codes.Add(".", "e");
            _codes.Add("..-.", "f");
            _codes.Add("--.", "g");
            _codes.Add("....", "h");
            _codes.Add("..", "i");
            _codes.Add(".---", "j");
            _codes.Add("-.-", "k");
            _codes.Add(".-..", "l");
            _codes.Add("--", "m");
            _codes.Add("-.", "n");
            _codes.Add("---", "o");
            _codes.Add(".--.", "p");
            _codes.Add("--.-", "q");
            _codes.Add(".-.", "r");
            _codes.Add("...", "s");
            _codes.Add("-", "t");
            _codes.Add("..-", "u");
            _codes.Add("...-", "v");
            _codes.Add(".--", "w");
            _codes.Add("-..-", "x");
            _codes.Add("-.--", "y");
            _codes.Add("--..", "z");
            _codes.Add(".----", "1");
            _codes.Add("..---", "2");
            _codes.Add("...--", "3");
            _codes.Add("....-", "4");
            _codes.Add(".....", "5");
            _codes.Add("-....", "6");
            _codes.Add("--...", "7");
            _codes.Add("---..", "8");
            _codes.Add("----.", "9");
            _codes.Add("-----", "0");
            _codes.Add(".-.-.-", ".");
            _codes.Add("--..--", ",");
            _codes.Add("..--..", "?");
            _codes.Add(".----.", "'");
            _codes.Add("-.-.--", "!");
            _codes.Add("-..-.", "/");
            _codes.Add("-.--.", "(");
            _codes.Add("-.--.-", ")");
            _codes.Add(".-...", "&");
            _codes.Add("---...", ":");
            _codes.Add("-.-.-.", ";");
            _codes.Add("-...-", "=");
            _codes.Add(".-.-.", "+");
            _codes.Add("-....-", "-");
            _codes.Add("..--.-", "_");
            _codes.Add(".-..-.", "\"");
            _codes.Add("...-..-", "$");
            _codes.Add(".--.-.", "@");
            _codes.Add("........", "{BACKSPACE}");

            return _codes;
        }

        private void wmpTrackBar_Scroll(object sender, EventArgs e)
        {
            WPM = wpmTrackBar.Value;
            CalculateTimesForWpm();
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            // If we're already training, calculate stats
            if (training)
            {
                recordButton.Text = "Record";
                if (trainingTimes.Count > 0)
                {
                    // Sort the training times
                    // But remove the outliers
                    trainingTimes.Sort();
                    int removeCount = (trainingTimes.Count / 10);
                    trainingTimes.RemoveRange(0, removeCount);
                    trainingTimes.RemoveRange(trainingTimes.Count - removeCount, removeCount);

                    double maxGap = Double.MinValue;
                    int gapIndex = 0;

                    // Pass 1: Find the largest gap between press times, as that
                    // will be the boundary between short and long presses.
                    System.Diagnostics.Debug.WriteLine("Got press: " + trainingTimes[0]);
                    for (int index = 1; index < trainingTimes.Count; index++)
                    {
                        System.Diagnostics.Debug.WriteLine("Got press: " + trainingTimes[index]);
                        double gap = trainingTimes[index] - trainingTimes[index - 1];
                        if (gap > maxGap)
                        {
                            maxGap = gap;
                            gapIndex = index;
                        }
                    }

                    // Pass 2: Average everything below the largest gap - that will
                    // be the 'dit' time.
                    double timeSum = 0;
                    for (int index = 0; index < gapIndex; index++)
                    {
                        timeSum += trainingTimes[index];
                    }
                    timeSum /= gapIndex;
                    System.Diagnostics.Debug.WriteLine("Average DIT: " + timeSum);

                    WPM = (int)((1000 * 60) / (timeSum * codexUnits));
                    WPM = Math.Min(WPM, wpmTrackBar.Maximum);

                    CalculateTimesForWpm();
                }
            }
            else
            {
                trainingTimes = new List<double>();
                recordButton.Text = "Stop";
            }

            training = !training;
        }
    }
}