using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Wire
{
	public class AsyncHttpListener : IDisposable
	{
		private readonly object _startStopLock = new object();
		private readonly HttpListener _listener;

		private volatile bool _IsRunning = false;
		public bool IsRunning => _IsRunning;

		public bool Disposed { get; private set; }

		private readonly uint _port;

		public event EventHandler<HttpListenerContext>? OnClientConnected;

		public AsyncHttpListener(uint port)
		{
			if (port == 0)
				throw new ArgumentException("Port must be greater than zero!");

			_port = port;
			_listener = new HttpListener();
			_listener.Prefixes.Add(String.Format(@"http://+:{0}/", _port));
		}

		~AsyncHttpListener() => Dispose(false);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			lock (_startStopLock)
			{
				if (Disposed)
					return;

				try
				{
					Stop();
					_listener?.Close();
				}
				catch (Exception) { }
				Disposed = true;
			}
		}

		public void Start()
		{
			lock (_startStopLock)
			{
				if (_IsRunning)
					return;

				_listener.Start();
				_listener.BeginGetContext(IncommingRequest, null);
				_IsRunning = true;
			}
		}

		public void Stop()
		{
			lock (_startStopLock)
			{
				if (!_IsRunning)
					return;

				_listener.Stop();
				_IsRunning = false;
			}
		}

		private void IncommingRequest(IAsyncResult result)
		{
			try
			{
				HttpListenerContext clientContext = _listener.EndGetContext(result);
				if (clientContext != null)
					Task.Run(() => HandleIncomingRequest(clientContext));
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
			}
			finally
			{
				if (_IsRunning)
					_listener.BeginGetContext(IncommingRequest, null);
			}
		}

		private void HandleIncomingRequest(HttpListenerContext clientContext)
		{
			try
			{
				if (_IsRunning)
					OnClientConnected?.Invoke(this, clientContext);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
			}
			finally
			{
				clientContext.Response.Close();
			}
		}

		public void Wait()
		{
			Console.Read();
			Stop();
		}
	}

	public class WireHTTPServer : IDisposable
	{
		private readonly AsyncHttpListener _listener;

		public WireHTTPServer(uint port = 80)
		{
			_listener = new AsyncHttpListener(port);
			_listener.OnClientConnected += _listener_OnClientConnected;
			_listener.Start();
		}

		private async void _listener_OnClientConnected(object sender, HttpListenerContext e)
		{
			await API.Resolve(API.FromHttpListener(e));
		}

		public void Wait()
		{
			Console.Read();
			Stop();
		}

		public void Stop()
		{
			_listener?.Stop();
		}

		public void Dispose() => _listener?.Dispose();
	}
}
