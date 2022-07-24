using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Wire
{
    public class WireHTTPServer : IDisposable
    {
       
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        public WireHTTPServer(int port = 80)
        {
            this.Initialize(port);
        }

        public WireHTTPServer()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(port);
        }

        public void Wait()
        {
            Console.Read();
            Stop();
        }

        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }
        
        private async void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            //_listener.Prefixes.Add("https://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    await API.Resolve(API.FromHttpListener(context));
                    context.Response.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
