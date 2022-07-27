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
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private Queue<HttpListenerContext> _queue;

        public int Port { get; private set; }

        public WireHTTPServer(int port = 80, int maxThreads = 8)
        {
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(() => {
                while (_listener.IsListening) {
                    var context = _listener.BeginGetContext((x) => {
                        try {
                            lock (_queue) {
                                _queue.Enqueue(_listener.EndGetContext(x));
                                _ready.Set();
                            }
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
            for (int i = 0; i < _workers.Length; i++) {
                _workers[i] = new Thread(async () => {
                    WaitHandle[] wait = new[] { _ready, _stop };
                    while (0 == WaitHandle.WaitAny(wait)) {
                        HttpListenerContext context;
                        lock (_queue) {
                            if (_queue.Count > 0)
                                context = _queue.Dequeue();
                            else {
                                _ready.Reset();
                                continue;
                            }
                        }
                        try {
                            await API.Resolve(API.FromHttpListener(context));
                            context.Response.Close();
                        }
                        catch (Exception e) { Console.Error.WriteLine(e); }
                    }
                });
                _workers[i].Start();
            }
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
            foreach (Thread worker in _workers)
                worker.Join();
            _listener.Stop();
        }

        public void Dispose() => Stop();
    }
}
