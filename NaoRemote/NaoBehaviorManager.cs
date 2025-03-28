//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using NaoRemote.Proxy;

namespace NaoRemote
{
    public class NaoBehaviorManager : ProxyBehaviorManager
    {
        private readonly ProxyBehaviorManager _postInstance;

        public NaoBehaviorManager(NaoRemoteModule naoModule) : base(naoModule, false)
        {
            _postInstance = new ProxyBehaviorManager(naoModule, true);
        }

        public ProxyBehaviorManager Post
        {
            get
            {
                return _postInstance;
            }
        }
    }
}
