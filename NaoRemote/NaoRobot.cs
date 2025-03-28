//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
namespace NaoRemote
{
    public class NaoRobot
    {
        private readonly NaoRemoteModule _naoModule;
        private readonly NaoBehaviorManager _naoBehaviorManager;
        private readonly NaoMotion _naoMotion;
        public NaoBehaviorManager NaoBehaviorManager { get { return _naoBehaviorManager; } }
        public NaoMotion NaoMotion { get { return _naoMotion; } }
        public bool Connected { get; set; }

        public NaoRobot()
        {
            _naoModule = new NaoRemoteModule();
            _naoBehaviorManager = new NaoBehaviorManager(_naoModule);
            _naoMotion = new NaoMotion(_naoModule);
        }
        public void Connect(string ipAddress)
        {
            Connected = _naoModule.Connect(ipAddress);
        }
        public void Disconnect()
        {
            _naoModule.Disconnect();
        }
        public T Eval<T>(string moduleName, bool usePost, string methodName, params object[] args)
        {
            return _naoModule.Eval<T>(moduleName, usePost, methodName, args);
        }
        public void SubscribeToEvent(string eventName, NaoRemoteModule.NaoEventHandler action)
        {
            _naoModule.SubscribeToEvent(eventName, action);
        }
        public void UnsubscribeToEvent(string eventName, NaoRemoteModule.NaoEventHandler action)
        {
            _naoModule.UnsubscribeToEvent(eventName, action);
        }

    }
}
