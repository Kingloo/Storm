using System;
using System.Runtime.Serialization;

namespace Storm.Wpf.StreamServices
{
    [Serializable]
    public class ServicesException : Exception
    {
        public Type StreamType { get; } = null;

        protected ServicesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public ServicesException()
            : base()
        { }

        public ServicesException(string message)
            : base(message)
        { }

        public ServicesException(string message, Exception exception)
            : base(message, exception)
        { }

        public ServicesException(string message, Type streamType)
            : base(message)
        {
            StreamType = streamType;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
