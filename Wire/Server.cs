using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Wire
{
    public class WireHTTPServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly ManualResetEvent _stop;

        public int Port { get; private set; }

        public WireHTTPServer(int port = 80, int maxThreads = 8)
        {
            _stop = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(() => {
                while (_listener.IsListening) {
                    var context = _listener.BeginGetContext((x) => {
                        try {
                            var _context = _listener.EndGetContext(x);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(async x => {
                                await API.Resolve(API.FromHttpListener(_context));
                                _context.Response.Close();
                            }));
                        }
                        catch { return; }
                    }, null);
                    if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle })) return;
                }
            });
            Port = port;
            _listener.Prefixes.Add(String.Format(@"http://+:{0}/", Port));
            _listener.Start();
            _listenerThread.Start();
        }

        public void Wait()
        {
            Console.Read();
            Stop();
        }

        public void Stop()
        {
            _stop.Set();
            _listenerThread.Join();
            _listener.Stop();
        }

        public void Dispose() => Stop();
    }
}
