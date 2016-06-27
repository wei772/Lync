/*=====================================================================
  File:      SalesPerson.cs

  Summary:   Model class for creating a Sales Person.

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/
using System;

namespace ProposalTracker.Models
{
    /// <summary>
    /// Class that models a Sales Person
    /// </summary>
    public class SalesPerson
    {
        public String SalesPersonUri { get; set; }
        public Double TotalSales { get; set; }
    }
}
