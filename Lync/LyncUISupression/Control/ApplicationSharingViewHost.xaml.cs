using Microsoft.Lync.Model.Conversation.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lync.Control
{
    /// <summary>
    /// Interaction logic for ApplicationSharingViewHost.xaml
    /// </summary>
    public partial class ApplicationSharingViewHost : UserControl
    {
        private static ILog _log = LogManager.GetLog(typeof(ApplicationSharingViewHost));



        public static DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(ApplicationSharingView), typeof(ApplicationSharingViewHost), new PropertyMetadata(OnViewPropertyChanged));
        public ApplicationSharingView View
        {
            get { return (ApplicationSharingView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }


        public static DependencyProperty SharingModalityProperty = DependencyProperty.Register("SharingModality", typeof(ApplicationSharingModality), typeof(ApplicationSharingViewHost), new PropertyMetadata(OnSharingModalityPropertyChanged));
        public ApplicationSharingModality SharingModality
        {
            get { return (ApplicationSharingModality)GetValue(SharingModalityProperty); }
            set { SetValue(SharingModalityProperty, value); }
        }


        public ApplicationSharingViewHost()
        {
            InitializeComponent();
            applicaionViewPanel.Layout += OnApplicaionViewPanelLayout;
        }



        #region UI Events

        private void OnApplicaionViewPanelLayout(object sender, System.Windows.Forms.LayoutEventArgs e)
        {
            //Microsoft.Lync.Model.LyncClientException
            if (View != null)
            {
                View.SyncRectangle();
            }
        }

        #endregion

        #region Property setter callbacks



        private static void OnViewPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var thisControl = (ApplicationSharingViewHost)sender;
            var view = (ApplicationSharingView)args.NewValue;

            if (view != null)
            {
                thisControl.ShowApplicationSharingView(thisControl.applicaionViewPanel, view);
            }
        }

        public static void OnSharingModalityPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {

        }



        private void ShowApplicationSharingView(System.Windows.Forms.Panel applicaionViewPanel, ApplicationSharingView view)
        {

            _log.Debug("ShowApplicationSharingView  applicaionViewPanel:{0}", applicaionViewPanel.Handle.ToInt32());

            try
            {
                view.PropertyChanged += OnViewPropertyChanged;
                view.SetParent(applicaionViewPanel.Handle.ToInt32());
                if (view.Properties == null)
                {
                    return;
                }



            }
            catch (Exception exception)
            {
                _log.ErrorException("", exception);
            }
        }


        private void OnViewPropertyChanged(object sender, ApplicationSharingViewPropertyChangedEventArgs e)
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
                if (SharingModality.View.DisplayMode == ApplicationSharingViewDisplayMode.FitToParent)
                {
                    return;
                }
            }
            else if (e.Property == ApplicationSharingViewProperty.ParentWindow)
            {
                if (SharingModality.View.State == ApplicationSharingViewState.Active)
                {
                    SharingModality.View.SyncRectangle();
                }

            }
        }

        #endregion


    }
}
