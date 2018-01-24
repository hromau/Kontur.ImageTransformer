using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;

        public static Regex urlGrayscale = new Regex(@"\w+(/process/grayscale/\d,\d,\d,\d/)");
        public static Regex urlSepia = new Regex(@"\w+/process/sepia/\d,\d,\d,\d/");
        public static Regex urlThreshold = new Regex(@"\w+/process/threshold(\d{100})/\d,\d,\d,\d/");

        public AsyncHttpServer()
        {
            listener = new HttpListener();
        }
        
        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();
                    
                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                        Console.WriteLine("New Request");
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    // TODO: log errors
                }
            }
        }

        public static byte CheckUrl(string url,ref byte x, ref byte y, ref byte w, ref byte h,ref byte thresholdX)
        {
            char[] sp = { ',', '/', '(', ')' };
            string[] temp0;
            if (urlGrayscale.IsMatch(url))
            {
                //grayscale
               temp0 = url.Split(sp);
                x = byte.Parse( temp0[5]);
                y = byte.Parse(temp0[6]);
                w = byte.Parse(temp0[7]);
                h = byte.Parse(temp0[8]);

                return 1;
            }

            if (urlSepia.IsMatch(url))
            {
                //sepia
                temp0 = url.Split(sp);
                x = byte.Parse(temp0[5]);
                y = byte.Parse(temp0[6]);
                w = byte.Parse(temp0[7]);
                h = byte.Parse(temp0[8]);
                return 2;
            }

            if (urlThreshold.IsMatch(url))
            {
                //threshold
                temp0 = url.Split(sp);
                x = byte.Parse(temp0[5]);
                y = byte.Parse(temp0[6]);
                w = byte.Parse(temp0[7]);
                h = byte.Parse(temp0[8]);
                thresholdX = byte.Parse(temp0[5]);
                return 3;
            }
            x = 0;
            y = 0;
            w = 0;
            h = 0;
            return 0;
        }

        public static bool CheckFileSize(long contentLength)
        {
            var fileSize = Math.Round(Convert.ToDouble(contentLength) / 1024.0, 2);
            if (fileSize <=100)
            {
                return true;
            }
            return false;
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            // TODO: implement request handling
            byte x=0;
            byte y=0;
            byte w=0;
            byte h=0;
            byte thresholdX=0;

            byte methodFilter = CheckUrl(listenerContext.Request.Url.ToString(),ref x,ref y,ref w,ref h,ref thresholdX);

            if (listenerContext.Request.HttpMethod.ToString() == "POST" && methodFilter !=0 &&
                listenerContext.Request.HasEntityBody == true && CheckFileSize(listenerContext.Request.ContentLength64))
            {
                Stream data = listenerContext.Request.InputStream;
                int asd = data.ReadByte();
                long contentLength = listenerContext.Request.ContentLength64;
                byte[] buffer = new byte[contentLength]; //BODY
                data.Read(buffer, 0, asd);

                byte[] responseBodyArray=null;
                switch (methodFilter)
                {
                    case 1:
                        {
                            //grayscale
                            responseBodyArray = ImageFiltering.GetBytesOutputArray(ImageFiltering.DrawAsGrayscale(ref buffer, x, y, w, h));
                            break;
                        }
                    case 2:
                        {
                            //sepia
                            responseBodyArray = ImageFiltering.GetBytesOutputArray(ImageFiltering.DrawAsSepiaTone(ref buffer, x, y, w, h));
                            break;
                        }
                    case 3:
                        {
                            //threshold
                            break;
                        }
                    default:break;
                }

                //listenerContext.Response.ContentLength64 = outputBodyArray.LongLength;
                listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                listenerContext.Response.OutputStream.Read(responseBodyArray, 0, responseBodyArray.Length);
               

            }
            else
            {
                return;
            }
            
            
            string body2 = listenerContext.Request.QueryString.ToString();
            string headers = listenerContext.Request.Headers.ToString();
            

           
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
            {
                if (listenerContext.Response.StatusCode == (int)HttpStatusCode.OK)
                    writer.WriteLine(HttpStatusCode.OK.ToString());
                //writer.WriteLine("Hello, world!");
                
            }
        }

        
    }
}