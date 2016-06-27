using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Rtc.Collaboration.Samples.Common.Activities
{

    /// <summary>
    /// NoRecognition Exception. This excetion is thrown if there is no recognition for consecutive n times in question answer activity.
    /// </summary>
    [Serializable]
    public class NoRecognitionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NoRecognitionException class.
        /// </summary>
        public NoRecognitionException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NoRecognitionException class.
        /// </summary>
        /// <param name="message">Message string</param>
        public NoRecognitionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// This constructor is needed for serialization.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public NoRecognitionException(System.Runtime.Serialization.SerializationInfo info,
    System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Support for inherited ISerializable.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


    }

    /// <summary>
    /// Silence Timeout Exception. This excetion is thrown if there is no input from user for consecutive n times in question answer activity.
    /// </summary>
    [Serializable]
    public class SilenceTimeOutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the SilenceTimeOutException class.
        /// </summary>
        public SilenceTimeOutException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SilenceTimeOutException class.
        /// </summary>
        /// <param name="message">Message string</param>
        public SilenceTimeOutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// This constructor is needed for serialization.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public SilenceTimeOutException(System.Runtime.Serialization.SerializationInfo info,
    System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Support for inherited ISerializable.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }



}
