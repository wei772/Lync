/*=====================================================================
  File:      DataStructures.cs
 
  Summary:   Represents Pair, Triple and Quardruple data structures.
 

/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/



using System;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{

    #region internal class Pair
 
    /// <summary>
    /// This class stores two related items together.
    /// This is useful, for example, to store name,value collection in a list.
    /// NameValueCollection in framework might be expensive (need to investigate in future).
    /// </summary>
    /// <typeparam name="FirstType">The first item type</typeparam>
    /// <typeparam name="SecondType">The second item type</typeparam>
    internal class Pair<FirstType, SecondType> : IEquatable<Pair<FirstType, SecondType>>
    {
        /// <summary>
        /// First item
        /// </summary>
        private FirstType m_firstItem;

        /// <summary>
        /// Second item
        /// </summary>
        private SecondType m_secondItem;

        public Pair(FirstType firstItem, SecondType secondItem)
        {
            m_firstItem = firstItem;
            m_secondItem = secondItem;
        }

        /// <summary>
        /// Get the first item.
        /// </summary>
        public FirstType First
        {
            get
            {
                return m_firstItem;
            }
        }

        /// <summary>
        /// Get the second item.
        /// </summary>
        public SecondType Second
        {
            get
            {
                return m_secondItem;
            }
        }

        #region IEquatable<Pair<FirstType, SecondType>> Members

        public bool Equals(Pair<FirstType, SecondType> other)
        {
            bool isFirstEqual = false;
            bool isSecondEqual = false;

            if (null != other)
            {
                string otherFirstAsString = other.m_firstItem as string;
                string otherSecondAsString = other.m_secondItem as string;

                if (null != otherFirstAsString)
                {
                    string thisFirstAsString = this.m_firstItem as string;
                    isFirstEqual = otherFirstAsString.Equals(thisFirstAsString, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isFirstEqual = other.m_firstItem.Equals(this.m_firstItem);
                }

                if (null != otherSecondAsString)
                {
                    string thisSecondAsString = this.m_secondItem as string;
                    isSecondEqual = otherSecondAsString.Equals(thisSecondAsString, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isSecondEqual = other.m_secondItem.Equals(this.m_secondItem);
                }
            }

            return isFirstEqual && isSecondEqual;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Pair<FirstType, SecondType>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }


    #endregion


    #region internal class Triple

    /// <summary>
    /// This class stores three related items together.
    /// This is useful, for example, to store values that can be passed as the "state" of an async operation.
    /// </summary>
    /// <typeparam name="T1">The first item type</typeparam>
    /// <typeparam name="T2">The second item type</typeparam>
    /// <typeparam name="T3">The thrid item type</typeparam>
    internal class Triple<T1, T2, T3> : Pair<T1, T2>, IEquatable<Triple<T1, T2, T3>>
    {
        /// <summary>
        /// Third item
        /// </summary>
        private T3 m_thirdItem;

        public Triple(T1 firstItem, T2 secondItem, T3 thirdItem)
            : base(firstItem, secondItem)
        {
            m_thirdItem = thirdItem;
        }

        /// <summary>
        /// Get the third item.
        /// </summary>
        public T3 Third
        {
            get
            {
                return m_thirdItem;
            }
        }

        #region IEquatable<Triple<T1, T2, T3> Members

        public bool Equals(Triple<T1, T2, T3> other)
        {
            bool isPairEqual = false;
            bool isThirdEqual = false;

            if (null != other)
            {
                isPairEqual = base.Equals(other);

                if (isPairEqual)
                {
                    string otherThirdAsString = other.m_thirdItem as string;

                    if (null != otherThirdAsString)
                    {
                        string thisThirdAsString = this.m_thirdItem as string;
                        isThirdEqual = otherThirdAsString.Equals(thisThirdAsString, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        isThirdEqual = other.m_thirdItem.Equals(this.m_thirdItem);
                    }
                }
            }

            return isPairEqual && isThirdEqual;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Triple<T1, T2, T3>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }

    #endregion
}
