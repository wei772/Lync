/*=====================================================================
  File:      AgentSkill.cs

  Summary:   Represents a particular skill with a particular value.


/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    /// <summary>
    /// Represents a particular skill and a particular value of that skill, i.e. Language=English.
    /// </summary>
	public class AgentSkill
	{

        internal AgentSkill(Skill skill, string value)
        {
            if (skill == null)
            {
                throw new ArgumentNullException("skill == null");
            }

            if (value == null)
            {
                throw new ArgumentException("value == null");
            }

            if (!skill.IsValidValue(value))
            {
                throw new ArgumentException("Invalid value: [" + value + "]");
            }

            this.Skill = skill;
            this.Value = value;
        }

        internal Skill Skill{ get; set; }
        
        internal string Value { get; set; }

        public override bool Equals(object obj)
        {
            AgentSkill agentSkill = obj as AgentSkill;
            if (agentSkill == null)
            {
                return false;
            }

            return this.Skill.Equals(agentSkill.Skill) && string.Equals(this.Value, agentSkill.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.Skill.GetHashCode() & this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Skill, this.Value);
        }

	}
}
