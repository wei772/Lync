/*=====================================================================
  File:      ViewModel.cs

  Summary:   Generic Base class for view models

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
namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    public abstract class ViewModel<T> : ViewModelBase
    {
        #region Properties

        protected internal T Model { get; private set; }

        #endregion

        #region Constructors

        protected ViewModel(T model)
        {
            Model = model;
            OnInitializeModel();
        }

        #endregion
        
        #region Methods

        protected virtual void OnInitializeModel()
        {
        }

        #endregion

    }
}
