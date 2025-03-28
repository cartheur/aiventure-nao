using System;

namespace NaoRemote
{
    public class NaoRobotException : Exception
    {
        public NaoRobotException(string message) : base(message) { }
    }
}
