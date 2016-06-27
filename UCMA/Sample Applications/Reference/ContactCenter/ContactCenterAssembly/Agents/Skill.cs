/*=====================================================================
  File:      Skill.cs

  Summary:   Represents a skill and possible values for the skill.


/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    /// <summary>
    /// Represents a particular skill, and the possible values for the skill.
    /// </summary>
	public class Skill
	{
        internal Skill()
        { }

        internal Skill(string name)
        {
            this.Name = name;
        }

        internal string Name { get; set; }
        internal List<string> Values { get; set; }
        

        /// <summary>
        /// The string displayed to the user when asking the user to specify a specific value for the skill.
        /// </summary>
        internal string MainPrompt{ get; set; }
       

        internal string NoRecoPrompt { get; set; }


        internal string SilencePrompt { get; set; }

        internal string RecognizedSkillPrompt { get; set; }


        internal bool IsValidValue(string value)
        {
            foreach (string val in this.Values)
            {
                if (val.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return true;  
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            Skill skill = obj as Skill;
            if (skill == null)
            {
                return false;
            }

            if (!string.Equals(this.Name, skill.Name))
            {
                return false;
            }

            if (this.Values.Count != skill.Values.Count)
            {
                return false;
            }

            foreach (string value in this.Values)
            {
                if (!skill.Values.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = this.Name.GetHashCode();
            foreach (string value in this.Values)
            {
                hashCode += value.GetHashCode();
            }

            return hashCode;
        }

        private static string _skillName = "";
        private static object syncRoot  = new object();
        internal static Skill FindSkill(string skillName, List<Skill> skillsList)
        {
            lock (syncRoot)
            {
                _skillName = skillName;
                var skill = skillsList.FirstOrDefault(MatchSkill);
                return skill;
            }
        
        }
        private static bool MatchSkill(Skill skill)
        {
            if (skill.Name == _skillName)
                return true;
            else
                return false;
        }
        public override string ToString()
        {
            return this.Name;
        }

        public static skillType Convert(Skill skill)
        {
            skillType st = new skillType();
            st.name = skill.Name;
            st.skillValues = new string[skill.Values.Count];
            int i = 0;
            skill.Values.ForEach( sv =>
            {
              st.skillValues[i++]=sv;
            });
            return st;
        }
	}
}
