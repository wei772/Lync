

/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common
{
    internal static class CommonHelper
    {
        /// <summary>
        /// Method return the deserialized object from raw data.
        /// </summary>
        /// <param name="data">The raw data to deserialize.</param>
        /// <param name="serializer">XmlSerializer which should be used for deserialization.</param>        
        /// <returns>
        /// Returns the deserialized object or throw XmlException, if deserialization failed.         
        /// </returns>
        /// <exception cref="XmlException">Throws XmlException if deserialization failed.</exception>
        internal static object DeserializeObjectFragment(byte[] data, XmlSerializer serializer)
        {
            Object o = null;
            Debug.Assert(serializer != null);
            try
            {
                using (MemoryStream bodyStream = new MemoryStream(data))
                {
                    o = serializer.Deserialize(bodyStream);
                }
            }
            catch (XmlException)
            {
                throw;
            }
            catch (InvalidOperationException ioe)
            {
                throw new XmlException("deserialization failure", ioe);
            }
            catch (ArithmeticException arithmeticException)
            {
                throw new XmlException("deserialization failure", arithmeticException);
            }
            catch (FormatException formatException)
            {
                throw new XmlException("deserialization failure", formatException);
            }

            return o;
        }

        /// <summary>
        /// Method return the serialized string for an object using a given serializer object.
        /// </summary>
        /// <param name="objectToSerialize">Object which is being serialized.</param>
        /// <param name="serializer">XmlSerializer which should be used for serialization.</param>
        /// <returns></returns>
        internal static byte[] SerializeObjectToByteArray(Object objectToSerialize, XmlSerializer serializer)
        {
            Debug.Assert(objectToSerialize != null);
            Debug.Assert(serializer != null);
            byte[] retVal = null;
            try
            {
                using (MemoryStream bodyStream = new MemoryStream(256))
                {
                    serializer.Serialize(bodyStream, objectToSerialize);
                    retVal = bodyStream.GetBuffer();
                }
            }
            catch (InvalidOperationException ioe)
            {
                throw new XmlException("deserialization failure", ioe);
            }
            return retVal;
        }

    }
}
