﻿using CustomLogger;
using QuazalServer.QNetZ;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace QuazalServer.ServerProcessors
{
	public class BackendServicesServer
	{
        private readonly ConcurrentBag<UdpClient> _listeners = new();
        private readonly ConcurrentBag<bool> _shutflags = new();
        private CancellationTokenSource _cts = null!;

		public void Start(List<Tuple<int, string>> PrudpInstance, uint serverPID, CancellationToken cancellationToken)
		{
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Parallel.ForEach(PrudpInstance, tuple => { new Thread(() => HandleClient(tuple.Item1, serverPID, tuple.Item2)).Start(); });
        }

        public void StopAsync()
        {
            _cts.Cancel();
            _shutflags.ToList().ForEach(x => x = false);
            _listeners.ToList().ForEach(x => x.Dispose());
        }

        public void HandleClient(int listenPort, uint serverPID, string AccessKey)
		{
            Task serverPRUDP = Task.Run(() =>
            {
                bool _exit = false;
                object _sync = new();
                Task<UdpReceiveResult>? CurrentRecvTask = null;
                UdpClient listener = new(listenPort);
                QPacketHandlerPRUDP? packetHandler = new(listener, serverPID, listenPort, listenPort, AccessKey, "BackendServices");
                packetHandler.Updates.Add(() => NetworkPlayers.DropPlayers());
                _listeners.Add(listener);
                _shutflags.Add(_exit);

                LoggerAccessor.LogInfo($"Backend Service Server initiated on port: {listenPort}...");

                while (!_cts.Token.IsCancellationRequested)
                {
                    lock (_sync)
                    {
                        if (_exit)
                            break;
                    }

                    try
                    {
                        packetHandler.Update();

                        // use non-blocking recieve
                        if (CurrentRecvTask != null)
                        {
                            if (CurrentRecvTask.IsCompleted)
                            {
                                UdpReceiveResult result = CurrentRecvTask.Result;
                                CurrentRecvTask = null;
                                packetHandler.ProcessPacket(result.Buffer, result.RemoteEndPoint);
                            }
                            else if (CurrentRecvTask.IsCanceled || CurrentRecvTask.IsFaulted)
                                CurrentRecvTask = null;
                        }

                        if (CurrentRecvTask == null)
                            CurrentRecvTask = listener.ReceiveAsync();
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException is SocketException socketException &&
                            socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                            LoggerAccessor.LogError($"[Backend Service] - HandleClient thrown an exception : {ex}");
                        CurrentRecvTask = null;
                    }

                    Thread.Sleep(1);
                }

                CurrentRecvTask = null;
                packetHandler = null;
            }, _cts.Token);
        }
	}
}
