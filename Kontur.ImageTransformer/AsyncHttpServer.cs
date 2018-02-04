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

        public static Regex urlGrayscale = new Regex(@"\w+/process/grayscale/\d{0,4},\d{0,4},\d{0,4},\d{0,4}/");
        public static Regex urlSepia = new Regex(@"\w+/process/sepia/\d{0,4},\d{0,4},\d{0,4},\d{0,4}/");
        public static Regex urlThreshold = new Regex(@"\w+/process/threshold\(\d{0,3}\)/\d{0,4},\d{0,4},\d{0,4},\d{0,4}/");

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
                x = byte.Parse(temp0[7]);
                y = byte.Parse(temp0[8]);
                w = byte.Parse(temp0[9]);
                h = byte.Parse(temp0[10]);
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

            w -= x;
            h -= y;

            if (w<=0 || h<=0)
            {
                listenerContext.Response.StatusCode =(int) HttpStatusCode.NoContent;
                listenerContext.Response.StatusDescription = HttpStatusCode.NoContent.ToString();
                listenerContext.Response.Close();
                return;
            }
            if (listenerContext.Request.HttpMethod == "POST" && methodFilter !=0 &&
                listenerContext.Request.HasEntityBody == true && CheckFileSize(listenerContext.Request.ContentLength64))
            {
                
                Stream data = listenerContext.Request.InputStream;
                long contentLength = listenerContext.Request.ContentLength64;
                byte[] buffer = new byte[contentLength]; //BODY
                
                data.Read(buffer, 0,(int) contentLength);
                
                Stream str = new MemoryStream(buffer);
                byte[] responseBodyArray=null;
                switch (methodFilter)
                {
                    case 1:
                        {
                            //grayscale
                            responseBodyArray = NewFilter.GetByteOutputArray(NewFilter.DrawAsGrayscale(NewFilter.ArrayToImage(buffer), w, h));
                            //Bitmap temp = ImageFiltering.MakeBitmap(buffer, w, h);
                            //Bitmap resultFilters = Filter2.DrawAsGrayscale(buffer);
                            //Image a = Filter2.ByteArraToImage(buffer);
                            //responseBodyArray = ImageFiltering.GetBytesOutputArray(resultFilters);

                            listenerContext.Response.ContentType = "text/html";
                            if (responseBodyArray==null || responseBodyArray.Length==0)
                            {
                                
                                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                                {
                                    listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    listenerContext.Response.ContentType = "text/html";
                                    listenerContext.Response.StatusDescription = HttpStatusCode.BadRequest.ToString();
                                    //listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                                    writer.WriteLine(listenerContext.Response.StatusCode + " " + listenerContext.Response.StatusDescription);
                                  
                                }
                                break;
                            }
                            listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                            listenerContext.Response.OutputStream.Read(responseBodyArray, 0, responseBodyArray.Length);


                            
                           
                            break;
                        }
                    case 2:
                        {
                            //sepia
                            //Bitmap resultFilters = Filter2.DrawAsSepiaTone(listenerContext.Request.InputStream);
                            //Image image = Filter2.ByteArraToImage(buffer);
                            //Bitmap a = Filter2.DrawAsSepiaTone(image);
                            responseBodyArray = NewFilter.GetByteOutputArray(NewFilter.DrawAsSepiaTone(NewFilter.ArrayToImage(buffer), w, h));

                            //responseBodyArray = ImageFiltering.GetBytesOutputArray(ImageFiltering.DrawAsSepiaTone(ref buffer, x, y, w, h));

                            listenerContext.Response.ContentType = "text/html";
                            if (responseBodyArray == null)
                            {

                                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                                {
                                    listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    listenerContext.Response.ContentType = "text/html";
                                    listenerContext.Response.StatusDescription = HttpStatusCode.BadRequest.ToString();
                                    //listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                                    writer.WriteLine(listenerContext.Response.StatusCode + " " + listenerContext.Response.StatusDescription);

                                }
                                break;
                            }
                            listenerContext.Response.ContentEncoding = listenerContext.Request.ContentEncoding;
                            listenerContext.Response.ContentLength64 = responseBodyArray.LongLength;
                            //listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                            listenerContext.Response.OutputStream.Write(responseBodyArray, 0, responseBodyArray.Length);
                            //listener.Stop();

                            break;
                        }
                    case 3:
                        {
                            //threshold
                            responseBodyArray = NewFilter.DrawAsThreshold(buffer, thresholdX,w, h);

                            if (responseBodyArray == null)
                            {
                                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                                {
                                    listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    listenerContext.Response.ContentType = "text/html";
                                    listenerContext.Response.StatusDescription = HttpStatusCode.BadRequest.ToString();
                                    //listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                                    writer.WriteLine(listenerContext.Response.StatusCode + " " + listenerContext.Response.StatusDescription);

                                }
                            }
                            listenerContext.Response.ContentEncoding = listenerContext.Request.ContentEncoding;
                            try
                            {
                                listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                            }
                            catch (Exception ex)
                            {
                               Console.WriteLine( ex.Message);
                            }
                            finally
                            {
                                try
                                {
                                    listenerContext.Response.OutputStream.Write(responseBodyArray, 0, responseBodyArray.Length);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                          
                            break;
                        }
                    default:break;
                }

                //listenerContext.Response.ContentLength64 = outputBodyArray.LongLength;
                //listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                //listenerContext.Response.OutputStream.Read(responseBodyArray, 0, responseBodyArray.Length);
               

            }
            else
            {

                using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    listenerContext.Response.ContentType = "text/html";
                    listenerContext.Response.StatusDescription = HttpStatusCode.BadRequest.ToString();
                    //listenerContext.Response.OutputStream.SetLength(responseBodyArray.LongLength);
                    writer.WriteLine(listenerContext.Response.StatusCode+" "+ listenerContext.Response.StatusDescription);
                }
                return;
            }
        }

        
    }
}