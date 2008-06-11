using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MapperTool
{
    public abstract class WorkNotifier
    {
        public abstract void WorkBegin();
        public abstract void WorkEnd();
    }

    public class BlinkingImageNotifier : WorkNotifier
    {
        BlinkingImageControl imgctrl;
        delegate void AsyncDelegate();
        private AsyncDelegate bstart, bstop;

        public BlinkingImageNotifier(BlinkingImageControl control)
        {
            imgctrl = control;
            bstart = new AsyncDelegate(this.BlinkStart);
            bstop = new AsyncDelegate(this.BlinkStop);
        }

        public override void WorkBegin()
        {
            imgctrl.BeginInvoke(bstart);
        }

        public override void WorkEnd()
        {
            imgctrl.BeginInvoke(bstop);
        }

        private void BlinkStart()
        {
            imgctrl.Blink = true;
        }
        private void BlinkStop()
        {
            imgctrl.Blink = false;
        }
    }

    public partial class BlinkingImageControl : UserControl
    {
        private bool _visibleonstop;

        public BlinkingImageControl()
        {
            InitializeComponent();
            _visibleonstop = true;
        }

        public Image Image
        {
            get
            {
                return picture.Image;
            }
            set
            {
                picture.Image = value;
            }
        }

        /// <remarks>intervallo di lampeggio in millisecondi</remarks>
        public int BlinkingInterval
        {
            get
            {
                return timerBlinking.Interval;
            }
            set
            {
                timerBlinking.Interval = value;
            }
        }

        public bool VisibleOnStop
        {
            get
            {
                return _visibleonstop;
            }
            set
            {
                _visibleonstop = value;
                if (!Blink)
                    this.Visible = value;
            }
        }

        public bool Blink
        {
            get
            {
                return timerBlinking.Enabled;
            }
            set
            {
                timerBlinking.Enabled = value;
                if (value == false)
                {
                    this.Visible = _visibleonstop;
                }
            }
        }

        private void timerBlinking_Tick(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
        }

        /*
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor != Color.Transparent)
                base.OnPaintBackground(e);
        }
        bool isPaintBackgroundComplete = false;
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.BackColor == Color.Transparent)
            {
                if (!isPaintBackgroundComplete)
                {
                    this.Visible = false;
                    this.Parent.Invalidate(Bounds);
                    this.Parent.Update();
                    this.Visible = true;
                    isPaintBackgroundComplete = true;
                    base.OnPaint(e);
                    return;
                }
                isPaintBackgroundComplete = false;
            }
        }
        */
    }
}
