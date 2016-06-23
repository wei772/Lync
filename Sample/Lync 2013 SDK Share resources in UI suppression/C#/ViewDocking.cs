/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation.Sharing;
using System;
using System.Windows.Forms;
namespace ShareResources
{
    partial class ShareResources_Form
    {


        /// <summary>
        /// Docks the application sharing view in a container control and then
        /// registers for view events
        /// </summary>
        private void DockAppShareView()
        {
            if (_sharingModality.View.Properties == null)
            {
                return;
            }

            //Set view display mode to user's choice
            if (this.autoSizeView_Checkbox.Checked == true)
            {
                _sharingModality.View.DisplayMode = ApplicationSharingViewDisplayMode.FitToParent;
            }
            else
            {
                _sharingModality.View.DisplayMode = ApplicationSharingViewDisplayMode.ActualSize;
            }

            //Register for application sharing view events.
            _sharingModality.View.PropertyChanged += View_PropertyChanged;
            _sharingModality.View.StateChanged += View_StateChanged;
            try
            {
                //Expand the width of the WinForm to contain the new view panel.
                //Width of WinForm is expanded by width of the panel plus 6 pixels
                this.Width += _windowSize._originalPanelWidth + 6;

                //Get the horizontal and vertical panel margins to preserve
                _windowSize._verticalMargin = this.Height - _windowSize._originalPanelHeight;
                _windowSize._horizontalMargin = this.Width - _windowSize._originalPanelWidth;


                //Instantiate the new panel and set various properties
                _ViewPanel = new Panel();
                _ViewPanel.SuspendLayout();
                this.SuspendLayout();

                _ViewPanel.BackColor = System.Drawing.Color.WhiteSmoke;
                _ViewPanel.Anchor = ((
                    System.Windows.Forms.AnchorStyles)(
                    (
                        (
                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Left
                        )
                    )
                ));

                _ViewPanel.Location = new System.Drawing.Point(415, 26);
                _ViewPanel.Size = new System.Drawing.Size(_windowSize._originalPanelWidth, _windowSize._originalPanelHeight);


                _ViewPanel.TabIndex = 1;


                //Add the new panel to the form
                this.Controls.Add(_ViewPanel);
                _ViewPanel.Visible = true;
                _ViewPanel.ResumeLayout(false);
                _ViewPanel.PerformLayout();
                this.ResumeLayout(false);
                this.PerformLayout();

                //Set the handle of the panel as the parent of the view
                _sharingModality.View.SetParent((int)_ViewPanel.Handle);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unauthorized access in DocAppShareView " + ex.Message);
            }

        }

        private void RemoveDockingPanel()
        {
            if (_ViewPanel != null && _ViewPanel.IsDisposed == false)
            {
                this.Controls.Remove(_ViewPanel);
                try
                {
                    _ViewPanel.Dispose();
                }
                catch (Exception) { }
                this.Width -= (_windowSize._originalPanelWidth + 6);
            }

        }


        private delegate void ResizeFormForPanelDelegate(ApplicationSharingView view);

        /// <summary>
        /// Changes the dimensions of the main form and container control to fit
        /// the new size of the viewer. The maximum dimensions of the form are 
        /// constrained by the dimensions of the screen work area.
        /// </summary>
        /// <param name="newHeight">int. The new height of the viewer</param>
        /// <param name="newWidth">int. The new width of the viewer</param>
        private void ResizeFormForPanel(ApplicationSharingView view)
        {

            //Add 4 display units to prevent scroll bars from appearing on container.
            int newViewWidth = (int)view.Properties[ApplicationSharingViewProperty.Width] + 4;
            int newViewHeight = (int)view.Properties[ApplicationSharingViewProperty.Height] + 4;

            //Get the maximum possible dimensions as constrained by the screen
            int screenHeight = SystemInformation.WorkingArea.Height;
            int screenWidth = SystemInformation.WorkingArea.Width;


            //If the original width of the form is less than the 
            //proposed new width, then widen the form to fit the new
            //panel dimensions
            if (_windowSize._originalFormWidth < (newViewWidth + _windowSize._horizontalMargin))
            {
                //If the proposed width is less than or equal to the
                //working area of the desktop, set the width to the 
                //proposed width
                if ((newViewWidth + _windowSize._horizontalMargin) <= screenWidth)
                {
                    _ViewPanel.Width = newViewWidth;
                    this.Width = newViewWidth + _windowSize._horizontalMargin;
                }
                else
                {
                    //Set the form to maximum width
                    _ViewPanel.Width = screenWidth - _windowSize._horizontalMargin;
                    this.Width = screenWidth;
                }
            }
            //Otherwise, set the form to it's original width.
            else
            {
                this.Width = _windowSize._originalFormWidth;
                _ViewPanel.Width = _windowSize._originalPanelWidth;
            }


            //If the original Height of the form is less than the 
            //proposed new height, then set the height of the form to
            //the new height
            if (_windowSize._originalFormHeight < (newViewHeight + _windowSize._verticalMargin))
            {
                //If the proposed height is less than or equal to the
                //working area of the desktop, set the height to the 
                //proposed height
                if ((newViewHeight + _windowSize._verticalMargin) <= screenHeight)
                {
                    _ViewPanel.Height = newViewHeight;
                    this.Height = newViewHeight + _windowSize._verticalMargin;
                }
                else
                {
                    _ViewPanel.Height = screenHeight - _windowSize._verticalMargin;
                    this.Height = screenHeight;
                }

            }
            else
            {
                this.Height = _windowSize._originalFormHeight;
                _ViewPanel.Height = _windowSize._originalPanelHeight;
            }
            //Resynch the view if it is docked in a parent container
            if (_sharingModality.View.Properties[ApplicationSharingViewProperty.ParentWindow] != null)
            {
                _sharingModality.View.SyncRectangle();
            }
        }

        /// <summary>
        /// Resets the main window to the original dimensions for displayed
        /// view panel
        /// </summary>
        private void ResetFormToOriginalDimensions()
        {
            this.Width = _windowSize._originalFormWidth;
            _ViewPanel.Width = _windowSize._originalPanelWidth;
            this.Width += _windowSize._originalPanelWidth + 6;
            this.Height = _windowSize._originalFormHeight;
            _ViewPanel.Height = _windowSize._originalPanelHeight;

        }
        void View_StateChanged(object sender, ApplicationSharingViewStateChangedEventArgs e)
        {
            ApplicationSharingView view = (ApplicationSharingView)sender;
            if (view.State == ApplicationSharingViewState.Active)
            {
                System.Diagnostics.Debug.WriteLine("Sync rectangle in View_StateChanged event");
                try
                {
                    view.SyncRectangle();
                }
                catch (InvalidStateException ise)
                {
                    System.Diagnostics.Debug.WriteLine("View state is invalid on call to SyncRectangle: " + ise.Message);
                }
            }
            else if (e.NewState == ApplicationSharingViewState.Inactive || e.NewState == ApplicationSharingViewState.Minimized)
            {
                this.Invoke(new NoParamDelegate(ResetFormToOriginalDimensions));
            }
        }

        /// <summary>
        /// Sets dimensions for container control parent of application sharing view when the 
        /// dimensions of the view change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void View_PropertyChanged(object sender, ApplicationSharingViewPropertyChangedEventArgs e)
        {
            ApplicationSharingView view = (ApplicationSharingView)sender;
            if (view.Properties == null)
            {
                return;
            }

            //If the changed viewer property is a dimension property then resize parent container control
            if (e.Property == ApplicationSharingViewProperty.Height || e.Property == ApplicationSharingViewProperty.Width)
            {
                //If user chose FitToParent, the parent container control is not resized. 
                if (_sharingModality.View.DisplayMode == ApplicationSharingViewDisplayMode.FitToParent)
                {
                    this.Invoke(new NoParamDelegate(ResetFormToOriginalDimensions));

                    return;
                }


                this.Invoke(
                    new ResizeFormForPanelDelegate(ResizeFormForPanel),
                    new object[] { view });
            }
            else if (e.Property == ApplicationSharingViewProperty.ParentWindow)
            {
                if (_sharingModality.View.State == ApplicationSharingViewState.Active)
                {
                    _sharingModality.View.SyncRectangle();
                }

            }
        }

    }
}