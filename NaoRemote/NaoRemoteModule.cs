//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NaoRemote.Json;

namespace NaoRemote
{
    public class NaoRemoteModule
    {
        private static bool _active;
        private static Socket _sender;
        private static byte[] _bytes;
        private IPAddress _ipAddress;
        private IPEndPoint _remoteEp;
        private Thread _sockThread;

        private readonly ManualResetEvent[] _poolWaiter;
        private int _poolWaiterIndex;

        private int _callCounter;
        private readonly Dictionary<int, ManualResetEvent> _pendingWaiter;
        private readonly Dictionary<int, string> _pendingResults;
        private readonly Dictionary<int, string> _pendingErrors;

        public delegate void NaoEventHandler(string eventName, NaoEventData data);
        private readonly Dictionary<string, List<NaoEventHandler>> _eventHandlers;

        public NaoRemoteModule()
        {
            _active = false;
            _bytes = new byte[1024];
            _poolWaiter = new ManualResetEvent[50];
            for (var i = 0; i < 50; i++)
            {
                _poolWaiter[i] = new ManualResetEvent(false);
            }
            _poolWaiterIndex = 0;
            _callCounter = 0;
            _pendingWaiter = new Dictionary<int, ManualResetEvent>();
            _pendingResults = new Dictionary<int, string>();
            _pendingErrors = new Dictionary<int, string>();
            _eventHandlers = new Dictionary<string, List<NaoEventHandler>>();
        }

        public bool Connect(string ip)
        {
            if (_active)
                return true;

            _active = true;

            _ipAddress = IPAddress.Parse(ip);
            _remoteEp = new IPEndPoint(_ipAddress, 9559);
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _sender.Connect(_remoteEp);
                _sockThread = new Thread(new ThreadStart(SockLoop));
                _sockThread.Start();
                return true;
            }
            catch(Exception ex)
            {
                //Logging.WriteLog(ex.Message, Logging.LogType.Error, Logging.LogCaller.ExternalRobotConnection);
                throw new NaoRobotException(ex.Message + ". The selection to use the external robot is set in the config file.");
            }
            
        }
        public bool Disconnect()
        {
            if (!_active)
                return false;

            _active = false;
            _sockThread.Abort();
            _sender.Disconnect(false);
            return true;
        }
        public T Eval<T>(string moduleName, bool usePost, string methodName, params object[] args)
        {
            int c;
            ManualResetEvent waiter;
            int bytesSent;

            lock (_poolWaiter)
            {
                c = _callCounter;
                _callCounter++;
                waiter = _poolWaiter[_poolWaiterIndex];
                _poolWaiterIndex = (_poolWaiterIndex + 1) % 50;
                _pendingWaiter[c] = waiter;
            }

            var requestDictionary = new Dictionary<string, object>
            {
                {"type", "eval"}, {"id", c}, {"modu", moduleName}, {"post", ((usePost) ? "1" : "0")}, {"meth", methodName}, {"args", JsonMapper.ToJson(args)}
            };

            var request = JsonMapper.ToJson(requestDictionary);
            var message = Encoding.UTF8.GetBytes(request);
            //Console.WriteLine("Sent : {0}", s);
            bytesSent = _sender.Send(message);
            waiter.WaitOne();
            waiter.Reset();
            if (_pendingResults.ContainsKey(c))
            {
                var result = _pendingResults[c];
                _pendingResults.Remove(c);
                if (result == "null")
                {
                    return default(T);
                }
                return JsonMapper.ToObject<List<T>>(result)[0];
            }
            var error = _pendingErrors[c];
            _pendingErrors.Remove(c);
            var e = new Exception(error);
            throw e;
        }
        public void SubscribeToEvent(string eventName, NaoEventHandler action)
        {
            lock (_eventHandlers)
            {
                List<NaoEventHandler> lActions;
                if (_eventHandlers.ContainsKey(eventName))
                {
                    lActions = _eventHandlers[eventName];
                    if (!lActions.Contains(action))
                    {
                        lActions.Add(action);
                    }
                }
                else
                {
                    lActions = new List<NaoEventHandler>();

                    int c;
                    ManualResetEvent waiter;
                    Dictionary<string, object> request;
                    string s;
                    byte[] msg;
                    int bytesSent;
                    string result;

                    lock (_poolWaiter)
                    {
                        c = _callCounter;
                        _callCounter++;
                        waiter = _poolWaiter[_poolWaiterIndex];
                        _poolWaiterIndex = (_poolWaiterIndex + 1) % 50;
                        _pendingWaiter[c] = waiter;
                    }

                    request = new Dictionary<string, object>()
                    {
                        {"type", "subs"}, {"id", c}, {"evt", eventName}
                    };
                    s = JsonMapper.ToJson(request);

                    msg = Encoding.UTF8.GetBytes(s);
                    //Console.WriteLine("Sent : {0}", s);
                    bytesSent = _sender.Send(msg);
                    waiter.WaitOne();
                    waiter.Reset();
                    result = _pendingResults[c];
                    _pendingResults.Remove(c);

                    lActions.Add(action);
                    _eventHandlers[eventName] = lActions;
                }
            }
        }
        public void UnsubscribeToEvent(string eventName, NaoEventHandler action)
        {
            lock (_eventHandlers)
            {
                List<NaoEventHandler> lActions;
                if (_eventHandlers.ContainsKey(eventName))
                {
                    lActions = _eventHandlers[eventName];
                    if (lActions.Contains(action))
                    {
                        lActions.Remove(action);
                        if (lActions.Count == 0)
                        {
                            _eventHandlers.Remove(eventName);

                            int c;
                            ManualResetEvent waiter;
                            Dictionary<string, object> request;
                            string s;
                            byte[] msg;
                            int bytesSent;
                            string result;

                            lock (_poolWaiter)
                            {
                                c = _callCounter;
                                _callCounter++;
                                waiter = _poolWaiter[_poolWaiterIndex];
                                _poolWaiterIndex = (_poolWaiterIndex + 1) % 50;
                                _pendingWaiter[c] = waiter;
                            }

                            request = new Dictionary<string, object>()
                            {
                                {"type", "unsu"}, {"id", c}, {"evt", eventName}
                            };
                            s = JsonMapper.ToJson(request);

                            msg = Encoding.UTF8.GetBytes(s);
                            //Console.WriteLine("Sent : {0}", s);
                            bytesSent = _sender.Send(msg);
                            waiter.WaitOne();
                            waiter.Reset();
                            result = _pendingResults[c];
                            _pendingResults.Remove(c);
                        }
                    }
                }
            }
        }
        public void SockLoop()
        {
            while (_active)
            {
                try
                {
                    int bytesRec;
                    string recv;
                    string[] responsesArray;

                    bytesRec = _sender.Receive(_bytes);
                    recv = Encoding.ASCII.GetString(_bytes, 0, bytesRec);
                    //Console.WriteLine("Received : {0}", recv);
                    string[] splitter = { ";;;" };
                    responsesArray = recv.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < responsesArray.Length; i++)
                    {
                        var response = JsonMapper.ToObject(responsesArray[i]);
                        if (((string)response["type"]) == "resu")
                        {
                            var c = (int)response["id"];
                            if ((string)response["stat"] == "1")
                                _pendingResults[c] = (string)response["res"];
                            else
                                _pendingErrors[c] = (string)response["err"];
                            _pendingWaiter[c].Set();
                            _pendingWaiter.Remove(c);
                        }
                        else if (((string)response["type"]) == "evnt")
                        {
                            var eventName = (string)response["name"];
                            var value = (string)response["val"];
                            lock (_eventHandlers)
                            {
                                if (_eventHandlers.ContainsKey(eventName))
                                {
                                    var lActions = _eventHandlers[eventName];
                                    for (var j = 0; j < lActions.Count; j++)
                                    {
                                        lActions[j](eventName, new NaoEventData(value));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

    }
}
