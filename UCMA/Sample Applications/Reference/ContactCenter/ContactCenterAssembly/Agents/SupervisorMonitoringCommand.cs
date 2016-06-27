
/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Agents
{
	public class SupervisorMonitoringCommand
	{
        private string _name;
        private string _helpDefinition;
        private string _value;

        public SupervisorMonitoringCommand()
        { 
        
        }

        public string Key
        {
            get { return _name; }
            set { _name = value; }
        }

        public string HelpDefinition
        {
            get { return _helpDefinition; }
            set { _helpDefinition = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
          
        }
	}
}
