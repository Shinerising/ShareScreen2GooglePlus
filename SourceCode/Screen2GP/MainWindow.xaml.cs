﻿using Google.GData.Photos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Screen2GP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        httpclient client = new httpclient();
        userinfo user = new userinfo("", "", "");
        private delegate string ThreadDelegate();

        NotifyIcon notifyIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();
            InitnotifyIcon();
            client.url = "https://plus.google.com/app/basic/login";
            if (ReadInfo()) HttpRequest(0, false);
            else
            {
                this.WindowState = WindowState.Normal;
                BeginFade();
            };
            textbox1.IsEnabled = false;
            button1.IsEnabled = false;
            button1.Content = "Logging...";
            KeyboardHandler hotkey = new KeyboardHandler(this);
        }

        private void InitnotifyIcon()
        {
            notifyIcon.BalloonTipText = "Hello, NotifyIcon!";
            notifyIcon.Text = "Quick Share to Google+";
            notifyIcon.Icon = Resource1.gp;
            notifyIcon.Visible = true;
            notifyIcon.Click += notifyIcon_Click;
            notifyIcon.MouseMove += notifyIcon_MouseMove;
        }

        private void notifyIcon_Click(object sender, EventArgs args)
        {
            this.WindowState = WindowState.Normal;
        }

        private void notifyIcon_MouseMove(object sender, EventArgs args)
        {
        }

        private void Showstatus(String status)
        {
            client.status = status;
            notifyIcon.BalloonTipText = status;
            notifyIcon.ShowBalloonTip(500);
        }

        static private string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        static private string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            string returnValue =
               System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }

        private void SaveInfo(string str1, string str2, string str3)
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["string1"].Value = str1;
            cfa.AppSettings.Settings["string2"].Value = str2;
            cfa.AppSettings.Settings["string3"].Value = str3;
            cfa.Save(ConfigurationSaveMode.Modified);
        }

        private bool ReadInfo()
        {
            try
            {
                user.email = DecodeFrom64(ConfigurationManager.AppSettings["string1"]);
                user.password = DecodeFrom64(ConfigurationManager.AppSettings["string2"]);
                user.uid = DecodeFrom64(ConfigurationManager.AppSettings["string3"]);
            }
            catch (Exception ex) { }
            if (user.email == "")
            {
                return false;
            }
            else
            {
                textbox2.Text = user.email;
                textbox3.Password = user.password;
                textbox4.Text = user.uid;
                return true;
            }
        }

        private void resolveHTML(String content)
        {
            string str0, str1, str2;
            int start = 0, end = 0;

            client.postClear();
            str0 = getBetween(content, "<form", ">");
            str0 = getBetween(str0, "action=\"", "\"");
            client.url = "https://m.google.com" + str0.Replace("amp;", "&");
            while (start != -1)
            {
                start = content.IndexOf("<input", end);
                end = content.IndexOf(">", start + 1);
                if (start != -1 && end != -1 && end > start)
                {
                    str0 = content.Substring(start, end - start);
                    str1 = getBetween(str0, "name=\"", "\"");
                    str2 = getBetween(str0, "value=\"", "\"");
                    if (str1 == "Email") str2 = user.email;
                    else if (str1 == "Passwd") str2 = user.password;
                    if (str1 != "editcircles") client.postAdd(str1, str2);
                }
            }
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public void GoPrint()
        {
            if (client.uploading == false && user.email!="" && grid11.Visibility==Visibility.Visible )
            {
                client.uploading = true;
                label1.Opacity = 1;
                button1.IsEnabled = false;
                try
                {
                    PrintScreen();
                }
                catch (Exception ex)
                {
                    label1.Content = "Upload failed";
                }
                this.WindowState = WindowState.Normal;
                if (this.Height == 210)
                {
                    DoubleAnimation myAnimation = new DoubleAnimation(210, 470, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                    this.BeginAnimation(HeightProperty, myAnimation);
                }
            }
        }

        private void PrintScreen()
        {
            label1.Content = "Uploading...";

            Bitmap printscreen = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
            Graphics graphics = Graphics.FromImage(printscreen as System.Drawing.Image);
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            var fileStream = new FileStream("screenshot/temp.png", FileMode.Create);
            printscreen.Save(fileStream, ImageFormat.Png);
            fileStream.Close();

            MemoryStream ms = new MemoryStream();
            printscreen.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            imagebox1.Source = bi;

            ThreadDelegate backWorkDel = new ThreadDelegate(UploadPhoto);
            backWorkDel.BeginInvoke(null, null);

        }

        private string UploadPhoto()
        {
            try
            {
                PicasaService service = new PicasaService("picasaupload");
                service.setUserCredentials(user.email, user.password);
                var file = "screenshot/temp.png";
                var fileStream = new FileStream(file, FileMode.Open);
                PicasaEntry entry = (PicasaEntry)service.Insert(new Uri("https://picasaweb.google.com/data/feed/api/user/default/albumid/default"), fileStream, "image/jpeg", file);
                fileStream.Close();
                PhotoAccessor ac = new PhotoAccessor(entry);
                string albumId = ac.AlbumId;
                string photoId = ac.Id;
                string contentUrl = entry.Media.Content.Attributes["url"] as string;
                ThreadDelegate UploadFinish = delegate()
                {
                    client.pid = photoId;
                    client.uploading = false;
                    label1.Opacity = 0;
                    button1.IsEnabled = true;
                    button1.Content = "Share";
                    return "succeed";
                };
                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, UploadFinish);
                return "succeed";
            }
            catch (Exception ex)
            {
                ThreadDelegate UploadFinish = delegate()
                {
                    label1.Content = "Upload failed";
                    client.uploading = false;
                    return "fail";
                };
                this.Dispatcher.BeginInvoke(DispatcherPriority.Send, UploadFinish);
                return "fail";
            }
        }


        private void ShowAvatar()
        {
            var image = new BitmapImage();
            int BytesToRead = 100;

            WebRequest request = WebRequest.Create(new Uri(user.avatar, UriKind.Absolute));
            request.Timeout = -1;
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            BinaryReader reader = new BinaryReader(responseStream);
            MemoryStream memoryStream = new MemoryStream();

            byte[] bytebuffer = new byte[BytesToRead];
            int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

            while (bytesRead > 0)
            {
                memoryStream.Write(bytebuffer, 0, bytesRead);
                bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
            }

            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);

            image.StreamSource = memoryStream;
            image.EndInit();

            image2.Source = image;
        }

        private async void HttpRequest(int pointer, bool retry)
        {
            if (retry) await Task.Delay(5000);
            if (pointer == 3)
            {
                button1.Content = "Shared!";
                await Task.Delay(1000);
                button1.Content = "Share";
                button1.IsEnabled = true;
                client.pid = "";
                textbox1.Text = "";
                if (this.Height == 470)
                {
                    DoubleAnimation myAnimation = new DoubleAnimation(470, 210, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                    this.BeginAnimation(HeightProperty, myAnimation);
                }
                Window_GoHind();
            }
            try
            {
                string responseBodyAsText;

                if (pointer == 0)
                {
                    client.pointer = pointer;
                    Showstatus("Fetching login infomation...");
                    HttpResponseMessage response = await client.client.GetAsync(client.url);
                    response.EnsureSuccessStatusCode();

                    responseBodyAsText = await response.Content.ReadAsStringAsync();
                    //browser1.NavigateToString(responseBodyAsText);
                    resolveHTML(responseBodyAsText);
                    client.url = "https://accounts.google.com/ServiceLoginAuth";
                    HttpRequest(1, false);
                }
                else if(pointer == 1)
                {
                    client.pointer = pointer;
                    Showstatus("Log in Google+...");
                    HttpResponseMessage response = await client.client.PostAsync(client.url, client.request);
                    response.EnsureSuccessStatusCode();

                    //responseBodyAsText = await response.Content.ReadAsStringAsync();
                    //browser1.NavigateToString(responseBodyAsText);

                    textbox1.IsEnabled = true;
                    button1.IsEnabled = true;
                    button1.Content = "Share";
                    Showstatus("Log in Succeed!");
                    ShowAvatar();

                }
                else if (pointer == 2)
                {
                    client.pointer = pointer;
                    HttpResponseMessage response = await client.client.GetAsync("https://m.google.com/app/plus/?v=compose&group=m1c&hideloc=1");
                    response.EnsureSuccessStatusCode();

                    responseBodyAsText = await response.Content.ReadAsStringAsync();
                    //browser1.NavigateToString(responseBodyAsText);
                    resolveHTML(responseBodyAsText);

                    client.postAdd("cpPostMsg", user.post);
                    client.postAdd("buttonPressed", "1");

                    if (client.pid != "")
                    {
                        client.postAdd("cpPhotoId", client.pid);
                        client.postAdd("cpPhotoOwnerId", user.uid);
                    }

                    response = await client.client.PostAsync(client.url, client.request);
                    response.EnsureSuccessStatusCode();

                    responseBodyAsText = await response.Content.ReadAsStringAsync();
                    //browser1.NavigateToString(responseBodyAsText);

                    HttpRequest(3, false);
                }
            }
            catch (HttpRequestException hre)
            {
                Showstatus("Connect error!");
                //label1.Content = hre.ToString();
                if (client.pointer == 2)
                {
                    HttpRequest(0, false);
                }
                else
                {
                    HttpRequest(0, true);
                }
            }
            catch (Exception ex)
            {
                // For debugging
                //label1.Content = ex.ToString();
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Window_GoHind();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            button1.IsEnabled = false;
            button1.Content = "Sharing";
            HttpRequest(2, false);
        }

        private void textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            user.post = textbox1.Text;
        }


        private void grid1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        public void BeginFade()
        {
            DoubleAnimation myAnimation = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 0, 0, 500)));
            if (grid11.Visibility == Visibility.Hidden)
            {
                grid11.Visibility = Visibility.Visible;
                grid11.BeginAnimation(OpacityProperty, myAnimation);
                grid12.Visibility = Visibility.Hidden;
            }
            else
            {
                grid12.Visibility = Visibility.Visible;
                grid12.BeginAnimation(OpacityProperty, myAnimation);
                grid11.Visibility = Visibility.Hidden;
            }
        }
        
        private void Label1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Label2_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Window_GoHind();
        }

        private void Label3_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BeginFade();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                DoubleAnimation animation = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 0, 0, 200)));
                this.BeginAnimation(OpacityProperty, animation);
            }
        }

        private void Window_GoHind()
        {
            DoubleAnimation animation = new DoubleAnimation(1, 0, new Duration(new TimeSpan(0, 0, 0, 0, 200)));
            animation.Completed += Window_Minimize;
            this.BeginAnimation(OpacityProperty, animation);
        }

        private void Window_Minimize(object sender, EventArgs args)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Label4_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            client.pid = "";
            DoubleAnimation myAnimation = new DoubleAnimation(470, 210, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
            this.BeginAnimation(HeightProperty, myAnimation);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                button3.Content = "Saving...";
                button3.IsEnabled = false;
                SaveInfo(EncodeTo64(textbox2.Text), EncodeTo64(textbox3.Password), EncodeTo64(textbox4.Text));
                button3.Content = "Apply";
                button3.IsEnabled = true;
                BeginFade();
                user.email = textbox2.Text;
                user.password = textbox3.Password;
                user.uid = textbox4.Text;
                HttpRequest(0, false);
            }
            catch(Exception ex){}
        }
    }

    public class httpclient
    {
        private CookieContainer cookies = new CookieContainer();
        private HttpClientHandler handler = new HttpClientHandler();
        public HttpClient client;
        private List<KeyValuePair<string, string>> postContent = new List<KeyValuePair<string, string>>();
        private string useragent = "Mozilla/4.0 (compatible; MSIE 5.0; S60/3.0 NokiaN73-1/2.0(2.0617.0.0.7) Profile/MIDP-2.0 Configuration/CLDC-1.1)";

        public String url;
        public String pid = "";
        public bool uploading = false;

        public int pointer;
        public string status = "Starting...";

        public HttpContent request
        {
            set{}
            get { return new FormUrlEncodedContent(this.postContent); }
        }
        public httpclient()
        {
            this.handler.CookieContainer = this.cookies;
            this.client = new HttpClient(this.handler);
            client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("user-agent",this.useragent );
        }
        public void postAdd(string p1,string p2)
        {
            this.postContent.Add(new KeyValuePair<string, string>(p1, p2));
        }
        public void postClear()
        {
            this.postContent.Clear();
        }

    }

    public class userinfo
    {
        public string email;
        public string password;
        public string post = "";
        private string userid = "";
        public string uid
        {
            set { this.avatar = "https://plus.google.com/s2/photos/profile/" + value + "?sz=75"; this.userid = value; }
            get { return this.userid; }
        }
        public string avatar;
        public userinfo(string u1, string u2, string u3)
        {
            this.email = u1;
            this.password = u2;
            this.uid = u3;
        }
    }


    public class KeyboardHandler : IDisposable
    {

        public const int WM_HOTKEY = 0x0312;
        public const int VIRTUALKEYCODE_FOR_CAPS_LOCK = 0x14;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Window _mainWindow;
        WindowInteropHelper _host;

        public KeyboardHandler(Window mainWindow)
        {
            _mainWindow = mainWindow;
            _host = new WindowInteropHelper(_mainWindow);

            SetupHotKey(_host.Handle);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY)
            {
                _mainWindow.Activate();
                ((MainWindow)App.Current.Windows[0]).GoPrint();
            }
        }

        private void SetupHotKey(IntPtr handle)
        {
            RegisterHotKey(handle, GetType().GetHashCode(), 0x0002, 0x47);
        }

        public void Dispose()
        {
            UnregisterHotKey(_host.Handle, GetType().GetHashCode());
        }
    }
}