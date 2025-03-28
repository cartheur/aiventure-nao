//
// This autonomous intelligent system software is the property of Cartheur Research, Copyright 2018 - 2025, all rights reserved.
//
using NaoRemote.Proxy;

namespace NaoRemote
{
    public class NaoMotion : ProxyMotion
    {
        private readonly ProxyMotion _postInstance;

        public NaoMotion(NaoRemoteModule naoModule) : base(naoModule, false)
        {
            _postInstance = new ProxyMotion(naoModule, true);
        }

        public ProxyMotion Post
        {
            get
            {
                return _postInstance;
            }
        }

    }
}
