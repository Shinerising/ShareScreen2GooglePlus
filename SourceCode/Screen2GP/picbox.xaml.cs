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
using System.Windows.Shapes;

namespace Screen2GP
{
    /// <summary>
    /// Interaction logic for picbox.xaml
    /// </summary>
    public partial class picbox : Window
    {

        private Point mLoc, oLoc;
        private bool mDown = false;
        private int ms = -1;
        private int m = -1;

        public picbox()
        {
            InitializeComponent();
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            box.Width = SystemParameters.PrimaryScreenWidth * 0.3;
            box.Height = SystemParameters.PrimaryScreenHeight * 0.3;
            Canvas.SetLeft(box, SystemParameters.PrimaryScreenWidth * 0.35);
            Canvas.SetTop(box, SystemParameters.PrimaryScreenHeight * 0.35);
            RectMask(Canvas.GetLeft(box), Canvas.GetTop(box), Canvas.GetLeft(box) + box.Width, Canvas.GetTop(box) + box.Height);
            Canvas.SetZIndex(box, 10);
        }

        private void RectMask(double x0, double y0, double x1, double y1)
        {
            double r;
            r = x0;if (r < 0) r = 0;
            Rec1.Width = r;
            r = x1; if (r < 0) r = 0;
            Rec2.Width = r;
            r = SystemParameters.PrimaryScreenWidth - x1; if (r < 0) r = 0;
            Rec3.Width = r;
            r = SystemParameters.PrimaryScreenWidth - x0; if (r < 0) r = 0;
            Rec4.Width = r;

            r = y1; if (r < 0) r = 0;
            Rec1.Height = r;
            r = SystemParameters.PrimaryScreenHeight - y1; if (r < 0) r = 0;
            Rec2.Height = r;
            r = SystemParameters.PrimaryScreenHeight - y0; if (r < 0) r = 0;
            Rec3.Height = r;
            r = y0; if (r < 0) r = 0;
            Rec4.Height = r;

            Canvas.SetLeft(Rec1, 0);
            Canvas.SetLeft(Rec2, 0);
            Canvas.SetLeft(Rec3, x1);
            Canvas.SetLeft(Rec4, x0);

            Canvas.SetTop(Rec1, 0);
            Canvas.SetTop(Rec2, y1);
            Canvas.SetTop(Rec3, y0);
            Canvas.SetTop(Rec4, 0);

            if (SystemParameters.PrimaryScreenHeight - y1 < 30)
            {
                Canvas.SetTop(label1, y0 - 26);
                Canvas.SetLeft(label1, x1 - 60);
            }
            else
            {
                Canvas.SetTop(label1, y1);
                Canvas.SetLeft(label1, x1 - 60);
            }
        }

        private void box_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mDown = true;
            oLoc = e.GetPosition(canvas);
            ms = m;
        }

        private void box_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mDown = false;
            ms = -1;
        }

        private void box_MouseMove(object sender, MouseEventArgs e)
        {
            mLoc = e.GetPosition(canvas);

            double bx0 = Canvas.GetLeft(box);
            double by0 = Canvas.GetTop(box);

            double bx1 = bx0 + box.Width;
            double by1 = by0 + box.Height;
            
            double xoffset = mLoc.X - oLoc.X;
            double yoffset = mLoc.Y - oLoc.Y;
            oLoc = mLoc;
            m = -1;

            if (ms < 0)
            {
                this.Cursor = Cursors.Arrow;
                if (Math.Abs(mLoc.X - bx0) <= 4)
                {
                    if (Math.Abs(mLoc.Y - by0) <= 4)
                    {
                        this.Cursor = Cursors.SizeNWSE;
                        m = 1;
                    }
                    else if (Math.Abs(mLoc.Y - by1) <= 4)
                    {
                        this.Cursor = Cursors.SizeNESW;
                        m = 3;
                    }
                    else if (mLoc.Y > by0 && mLoc.Y < by1)
                    {
                        this.Cursor = Cursors.SizeWE;
                        m = 2;
                    }
                }
                else if (Math.Abs(mLoc.X - bx1) <= 4)
                {
                    if (Math.Abs(mLoc.Y - by0) <= 4)
                    {
                        this.Cursor = Cursors.SizeNESW;
                        m = 6;
                    }
                    else if (Math.Abs(mLoc.Y - by1) <= 4)
                    {
                        this.Cursor = Cursors.SizeNWSE;
                        m = 8;
                    }
                    else if (mLoc.Y > by0 && mLoc.Y < by1)
                    {
                        this.Cursor = Cursors.SizeWE;
                        m = 7;
                    }
                }
                else if (mLoc.X > bx0 && mLoc.X < bx1)
                {
                    if (Math.Abs(mLoc.Y - by0) <= 4)
                    {
                        this.Cursor = Cursors.SizeNS;
                        m = 4;
                    }
                    else if (Math.Abs(mLoc.Y - by1) <= 4)
                    {
                        this.Cursor = Cursors.SizeNS;
                        m = 5;
                    }
                    else if (mLoc.Y > by0 && mLoc.Y < by1)
                    {
                        this.Cursor = Cursors.SizeAll;
                        m = 0;
                    }
                }
            }
            else
            {
                double bw = box.Width;
                double bh = box.Height;
                switch (ms)
                {
                    case 0: Canvas.SetLeft(box, bx0 + xoffset); Canvas.SetTop(box, by0 + yoffset); break;
                    case 1: bw -= xoffset; bh -= yoffset; Canvas.SetLeft(box, bx0 + xoffset); Canvas.SetTop(box, by0 + yoffset); break;
                    case 2: bw -= xoffset; Canvas.SetLeft(box, bx0 + xoffset); break;
                    case 3: bw -= xoffset; bh += yoffset; Canvas.SetLeft(box, bx0 + xoffset); break;
                    case 4: bh -= yoffset; Canvas.SetTop(box, by0 + yoffset); break;
                    case 5: bh += yoffset; break;
                    case 6: bw += xoffset; bh -= yoffset; Canvas.SetTop(box, by0 + yoffset); break;
                    case 7: bw += xoffset; break;
                    case 8: bw += xoffset; bh += yoffset; break;
                }
                if (bw < 40) bw = 40;
                if (bh < 40) bh = 40;
                box.Width = bw;
                box.Height = bh;
                RectMask(Canvas.GetLeft(box), Canvas.GetTop(box), Canvas.GetLeft(box) + bw, Canvas.GetTop(box) + bh);
            }
        }

        private void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsEnabled = false;
            ((MainWindow)Application.Current.Windows[1]).GoPrint(Canvas.GetLeft(box), Canvas.GetTop(box), box.Width, box.Height);
            this.Hide();
        }

        private void Rec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.IsEnabled = false;
            this.Hide();
        }
    }
}
