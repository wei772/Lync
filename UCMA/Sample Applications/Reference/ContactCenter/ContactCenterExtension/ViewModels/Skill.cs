/*=====================================================================
  File:      Skill.cs

  Summary:   Represents a skill and possible values for the skill.

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

using System.Collections.Generic;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    /// <summary>
    /// Represents a particular skill, and the possible values for the skill.
    /// </summary>
    public class Skill
    {
        #region Properties

        internal string Name { get; set; }
        internal List<string> Values { get; set; }

        #endregion

        #region Constructors
        
        internal Skill(string name)
        {
            Name = name;
        }

        #endregion

       
    }
}
