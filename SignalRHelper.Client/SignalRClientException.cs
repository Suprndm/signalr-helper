using System;

namespace SignalRHelper.Client
{
    public class SignalRClientException : Exception
    {
        public SignalRClientException(string message, Exception e) : base(message, e)
        {

        }

        public SignalRClientException(string message) : base(message)
        {

        }
    }
}
