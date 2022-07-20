using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wire
{
    public class SimpleHTTPServer : IDisposable
    {
       
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        public SimpleHTTPServer(int port)
        {
            this.Initialize(port);
        }

        public SimpleHTTPServer(string path)
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(port);
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
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    await API.Resolve(API.FromHttpListener(context));
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
