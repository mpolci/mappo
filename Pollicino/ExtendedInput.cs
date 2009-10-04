using System;
using System.Text;
using Sensors;
using System.Threading;

namespace MapperTools.Pollicino
{
    public class ExtendedInput: IDisposable
    {
        // sensors
        Sensors.IGSensor gSensor;
        HTCNavSensor htcnavsensor;
        // g-sensor and display switching data
        Sensors.ScreenOrientation lastOrientation;
        int lastOrientationTick;
        bool dispStatusOff;
        // nav sensor data
        private int htcnavRotationTick = 0;
        private double htcnavRotation = 0;
        private int htcnavRotationSleepTo = 0;
        // timing
        Timer timer;
        int lastTick;

        public ExtendedInput(System.Windows.Forms.Form form)
        {
            // G Sensor
            try
            {
                gSensor = GSensorFactory.CreateGSensor();
                dispStatusOff = false;
                lastOrientation = gSensor.Orientation;
                lastOrientationTick = Environment.TickCount;
            }
            catch (Exception) { }

            // HTC Nav Sensor
            try
            {
                htcnavsensor = new HTCNavSensor(form);
                htcnavsensor.Rotated += new NavSensorMoveHandler(this.HTCNavSensor_Rotated);
            }
            catch (Exception) { }

            // Timing
            lastTick = Environment.TickCount;
            timer = new Timer(new TimerCallback(this.timertick), null, 1000, 1000);
        }

        public event EventHandler OnShake;
        public event EventHandler OnShortShake;

        public delegate void NavHandler();
        public event NavHandler NavCW;
        public event NavHandler NavCCW;

        public bool GDisplayOff { get; set; }
        public bool GSensorEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private void timertick(object s)
        {
            int tick = Environment.TickCount;
            //int tickdelta = tick - lastTick;
            lastTick = tick;

            if (gSensor != null)
            {
                GVector v = gSensor.GetGVector();
                if (tick - lastOrientationTick >= 700)
                {
                    lastOrientationTick = tick;
                    OrientationCheck(v);
                }
            }
        }

        private void OrientationCheck(GVector v)
        {
            ScreenOrientation o = v.ToScreenOrientation();
            if (GDisplayOff && lastOrientation == o)
                if (o == ScreenOrientation.ReversePortrait)
                {
                    // TODO: bisognerebbe controllare il valore ritornato
                    PlatformSpecificCode.PowerForceDisplayOff();
                    dispStatusOff = true;
                }
                else if (dispStatusOff && (o == ScreenOrientation.Portrait || o == ScreenOrientation.FaceUp))
                {
                    // TODO: bisognerebbe controllare il valore ritornato
                    PlatformSpecificCode.PowerForceDisplayOn();
                    dispStatusOff = false;
                }
            lastOrientation = o;
        }

        private void HTCNavSensor_Rotated(double rotationsPerSecond, double radialDelta)
        {
            int nowtick = Environment.TickCount;
            if (nowtick > htcnavRotationSleepTo)
            {
                if (nowtick - htcnavRotationTick > 400)
                {
                    htcnavRotation = radialDelta;
                    //htcnavRotationTick = nowtick;
                }
                else
                    htcnavRotation += radialDelta;
                htcnavRotationTick = nowtick;
                if (Math.Abs(htcnavRotation) > 0.25)
                {
                    if (htcnavRotation > 0)
                    {
                        if (NavCW != null) NavCW();
                    }
                    else
                        if (NavCCW != null) NavCCW();
                    htcnavRotation = 0;
                    htcnavRotationSleepTo = nowtick + 1000;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            timer.Dispose();
            if (gSensor != null)
                gSensor.Dispose();
            if (htcnavsensor != null) 
                htcnavsensor.Dispose();
        }

        #endregion
    }
}
