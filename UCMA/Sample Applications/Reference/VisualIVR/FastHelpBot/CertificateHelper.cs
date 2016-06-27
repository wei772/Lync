/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Security.Cryptography.X509Certificates;

namespace FastHelpServer
{
    static class CertificateHelper
    {
        public static X509Certificate2 GetLocalCertificate(string friendlyName)
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine);

            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = store.Certificates;
            store.Close();

            foreach (X509Certificate2 certificate in certificates)
            {
                if (certificate.FriendlyName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase))
                {
                    return certificate;
                }
            }
            return null;

        }
    }
}
