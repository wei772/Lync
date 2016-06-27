/*=====================================================================
  File:      BooleanToVisibility.cs

  Summary:   Converter class for converting Boolean to Visibility. 

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
using System.Windows;

namespace ProposalTracker.Converters
{
    /// <summary>
    /// NotBooleanToVisibility Converter. Implements the BooleanToVisibility Converter and 
    /// overrides the ConvertValue method to return the opposite of the implementation in the
    /// base class.
    /// </summary>
    public class NotBooleanToVisibility :
        BooleanToVisibility
    {
        /// <summary>
        /// Method that overrides the ConvertValue method of the base class. 
        /// </summary>
        /// <param name="value"></param>
        protected override Visibility ConvertValue(bool value)
        {
            return ConvertCollapsed(!value);
        }
    } 
}
