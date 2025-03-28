//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using System.Collections.Generic;

namespace NaoRemote.Proxy
{
    public class ProxyBehaviorManager
    {
        private const string ModuleName = "ALBehaviorManager";
        private readonly NaoRemoteModule _naoModule;
        private readonly bool _isPost;

        public ProxyBehaviorManager(NaoRemoteModule naoModule, bool isPost)
        {
            _naoModule = naoModule;
            _isPost = isPost;
        }

        public void AddDefaultBehavior(string prefixedBehavior)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "addDefaultBehavior", prefixedBehavior);
        }

        public List<string> GetInstalledBehaviors()
        {
            return _naoModule.Eval<List<string>>(ModuleName, _isPost, "getInstalledBehaviors");
        }
        public List<string> GetRunningBehaviors()
        {
            return _naoModule.Eval<List<string>>(ModuleName, _isPost, "getRunningBehaviors");
        }
        public List<string> GetSystemBehaviorNames()
        {
            return _naoModule.Eval<List<string>>(ModuleName, _isPost, "getSystemBehaviorNames");
        }

        public bool InstallBehavior(string absolutePath, string localPath, bool overwrite)
        {
            return _naoModule.Eval<bool>(ModuleName, _isPost, "installBehavior", absolutePath, localPath, overwrite);
        }
        public bool InstallBehavior(string localPath)
        {
            return _naoModule.Eval<bool>(ModuleName, _isPost, "installBehavior", localPath);
        }
        public bool IsBehaviorInstalled(string name)
        {
            return _naoModule.Eval<bool>(ModuleName, _isPost, "isBehaviorInstalled", name);
        }
        public bool IsBehaviorRunning(string name)
        {
            return _naoModule.Eval<bool>(ModuleName, _isPost, "IsBehaviorRunning", name);
        }
        public bool PreloadBehavior(string name)
        {
            return _naoModule.Eval<bool>(ModuleName, _isPost, "preloadBehavior", name);
        }
        public bool RemoveBehavior(string name)
        {
            return _naoModule.Eval<bool>(ModuleName, _isPost, "removeBehavior", name);
        }
        public void RemoveDefaultBehavior(string name)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "removeDefaultBehavior");
        }
        public void RunBehavior(string name)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "runBehavior", name);
        }
        public void StopAllBehaviors()
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "stopAllBehaviors");
        }
        public void StopBehavior(string name)
        {
            _naoModule.Eval<object>(ModuleName, _isPost, "stopBehavior", name);
        }

    }
}
