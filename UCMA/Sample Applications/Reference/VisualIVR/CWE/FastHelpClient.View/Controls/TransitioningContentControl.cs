/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpClient.View.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
    /// A ContentControl that animates the transition when its content is changed.
    /// </summary>
    [TemplatePart(Name = TransitioningContentControl.PARTCONTAINER, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = TransitioningContentControl.PARTPREVIOUSCONTENTSITE, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = TransitioningContentControl.PARTCURRENTCONTENTSITE, Type = typeof(ContentPresenter))]
    [TemplateVisualState(Name = NormalState, GroupName = PresentationGroup)]
    public class TransitioningContentControl : ContentControl
    {
        /// <summary>
        /// PART Container
        /// </summary>
        public const string PARTCONTAINER = "PART_Container";
        
        /// <summary>
        /// Previous Content
        /// </summary>
        public const string PARTPREVIOUSCONTENTSITE = "PART_PreviousContentPresentationSite";
        
        /// <summary>
        /// Current Content
        /// </summary>
        public const string PARTCURRENTCONTENTSITE = "PART_CurrentContentPresentationSite";

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="Transition"/> property.
        /// </summary>
        public static readonly DependencyProperty TransitionProperty = DependencyProperty.RegisterAttached("Transition", typeof(TransitionType), typeof(TransitioningContentControl), new PropertyMetadata(TransitionType.Fade, OnTransitionChanged));

        /// <summary>
        /// <see cref="DependencyProperty"/> for the <see cref="TransitionPart"/> property.
        /// </summary>
        public static readonly DependencyProperty TransitionPartProperty = DependencyProperty.RegisterAttached("TransitionPart", typeof(TransitionPartType), typeof(TransitioningContentControl), new PropertyMetadata(TransitionPartType.OutIn, OnTransitionPartChanged));

        /// <summary>
        /// Presentation Group
        /// </summary>
        private const string PresentationGroup = "PresentationStates";

        /// <summary>
        /// Normal State
        /// </summary>
        private const string NormalState = "Normal";

        /// <summary>
        /// isTransitioning - to check whether page is in transition
        /// </summary>
        private bool isTransitioning;

        /// <summary>
        /// bool canSplitTransition
        /// </summary>
        private bool canSplitTransition;

        /// <summary>
        /// Storyboard startingTransition
        /// </summary>
        private Storyboard startingTransition;

        /// <summary>
        /// Storyboard completingTransition
        /// </summary>
        private Storyboard completingTransition;

        /// <summary>
        /// Grid container
        /// </summary>
        private Grid container;

        /// <summary>
        /// ContentPresenter previousContentPresentationSite
        /// </summary>
        private ContentPresenter previousContentPresentationSite;

        /// <summary>
        /// ContentPresenter currentContentPresentationSite
        /// </summary>
        private ContentPresenter currentContentPresentationSite;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitioningContentControl"/> class.
        /// </summary>
        public TransitioningContentControl()
        {
            this.DefaultStyleKey = typeof(TransitioningContentControl);
            this.Loaded += this.TransitioningContentControlLoaded;
        }

        /// <summary>
        /// Occurs when a transition has completed.
        /// </summary>
        public event RoutedEventHandler TransitionCompleted;

        /// <summary>
        /// Occurs when a transition has started.
        /// </summary>
        public event RoutedEventHandler TransitionStarted;

        /// <summary>
        /// Represents the type of transition that a TransitioningContentControl will perform.
        /// </summary>
        public enum TransitionType
        {
            /// <summary>
            /// A simple fading transition.
            /// </summary>
            Fade,

            /// <summary>
            /// A transition that fades the new element in from the top.
            /// </summary>
            FadeDown,

            /// <summary>
            /// A transition that slides old content left and out of view, then slides new content back in from the same direction.
            /// </summary>
            SlideLeft
        }

        /// <summary>
        /// Represents the part of the transition that the developer would like the TransitioningContentControl to perform
        /// </summary>
        /// <remarks>This only applies to certain TransitionTypes. An InvalidOperationException will be thrown if the TransitionType does not support the TransitionPartType. Default is OutIn.</remarks>
        public enum TransitionPartType
        {
            /// <summary>
            /// Transitions out only.
            /// </summary>
            Out,

            /// <summary>
            /// Transitions in only.
            /// </summary>
            In,

            /// <summary>
            /// Transitions in and out.
            /// </summary>
            OutIn
        }

        /// <summary>
        /// Gets or sets the transition.
        /// </summary>
        /// <value>The transition.</value>
        public TransitionType Transition
        {
            get { return (TransitionType)this.GetValue(TransitionProperty); }
            set { this.SetValue(TransitionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the transition part.
        /// </summary>
        /// <value>The transition part.</value>
        public TransitionPartType TransitionPart
        {
            get { return (TransitionPartType)this.GetValue(TransitionPartProperty); }
            set { this.SetValue(TransitionPartProperty, value); }
        }

        /// <summary>
        /// Gets or sets the starting transition.
        /// </summary>
        /// <value>
        /// The starting transition.
        /// </value>
        private Storyboard StartingTransition
        {
            get
            {
                return this.startingTransition;
            }
            
            set
            {
                this.startingTransition = value;
                if (this.startingTransition != null)
                {
                    this.SetTransitionDefaultValues();
                }
            }
        }

        /// <summary>
        /// Gets or sets the completing transition.
        /// </summary>
        /// <value>
        /// The completing transition.
        /// </value>
        private Storyboard CompletingTransition
        {
            get
            {
                return this.completingTransition;
            }

            set
            {
                // Decouple transition.
                if (this.completingTransition != null)
                {
                    this.completingTransition.Completed -= this.OnTransitionCompleted;
                }

                this.completingTransition = value;

                if (this.completingTransition != null)
                {
                    this.completingTransition.Completed += this.OnTransitionCompleted;
                    this.SetTransitionDefaultValues();
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:System.Windows.Controls.Control.ApplyTemplate"/>. In simplest terms, this means the method is called just before a UI element displays in an application. For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Wire up all of the various control parts.
            this.container = (Grid)GetTemplateChild("PART_Container");
            if (this.container == null)
            {
                throw new ArgumentException("PART_Container not found.");
            }

            this.currentContentPresentationSite = (ContentPresenter)GetTemplateChild("PART_CurrentContentPresentationSite");
            if (this.currentContentPresentationSite == null)
            {
                throw new ArgumentException("PART_CurrentContentPresentationSite not found.");
            }

            this.previousContentPresentationSite = (ContentPresenter)GetTemplateChild("PART_PreviousContentPresentationSite");
            if (this.previousContentPresentationSite == null)
            {
                throw new ArgumentException("PART_PreviousContentPresentationSite not found.");
            }

            // Set the current content site to the first piece of content.
            this.currentContentPresentationSite.Content = Content;
            VisualStateManager.GoToState(this, NormalState, false);
        }

        /// <summary>
        /// Called when the value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        /// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            this.QueueTransition(oldContent, newContent);
            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// Called when [transition changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnTransitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transitioningContentControl = (TransitioningContentControl)d;
            var transition = (TransitionType)e.NewValue;

            transitioningContentControl.canSplitTransition = VerifyCanSplitTransition(transition, transitioningContentControl.TransitionPart);
        }

        /// <summary>
        /// Called when [transition part changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnTransitionPartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transitioningContentControl = (TransitioningContentControl)d;
            var transitionPart = (TransitionPartType)e.NewValue;

            transitioningContentControl.canSplitTransition = VerifyCanSplitTransition(transitioningContentControl.Transition, transitionPart);
        }

        /// <summary>
        /// Verifies the can split transition.
        /// </summary>
        /// <param name="transition">The transition.</param>
        /// <param name="transitionPart">The transition part.</param>
        /// <returns>bool whether split transition is possible</returns>
        private static bool VerifyCanSplitTransition(TransitionType transition, TransitionPartType transitionPart)
        {
            // Check whether the TransitionPart is compatible with the current transition.
            var canSplitTransition = true;
            if (transition == TransitionType.Fade || transition == TransitionType.FadeDown)
            {
                if (transitionPart != TransitionPartType.OutIn)
                {
                    throw new InvalidOperationException("Cannot split this transition.");
                }

                canSplitTransition = false;
            }

            return canSplitTransition;
        }

        /// <summary>
        /// Transitionings the content control loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void TransitioningContentControlLoaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Aborts the transition.
        /// </summary>
        private void AbortTransition()
        {
            // Go to a normal state and release our hold on the old content.
            VisualStateManager.GoToState(this, NormalState, false);
            this.isTransitioning = false;
            if (this.previousContentPresentationSite != null)
            {
                this.previousContentPresentationSite.Content = null;
            }
        }

        /// <summary>
        /// Called when [transition completed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            this.AbortTransition();

            var handler = this.TransitionCompleted;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Raises the transition started.
        /// </summary>
        private void RaiseTransitionStarted()
        {
            var handler = this.TransitionStarted;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Queues the transition.
        /// </summary>
        /// <param name="oldContent">The old content.</param>
        /// <param name="newContent">The new content.</param>
        private void QueueTransition(object oldContent, object newContent)
        {
            // Both ContentPresenters must be available, otherwise a transition is useless.
            if (this.currentContentPresentationSite != null && this.previousContentPresentationSite != null)
            {
                this.currentContentPresentationSite.Content = newContent;
                this.previousContentPresentationSite.Content = oldContent;

                if (!this.isTransitioning)
                {
                    // Determine the TransitionPart that is associated with this transition and either set up a single part transition, or a queued transition.
                    string startingTransitionName;
                    if (this.TransitionPart == TransitionPartType.OutIn && this.canSplitTransition)
                    {
                        // Wire up the completion transition.
                        var transitionInName = this.Transition + "Transition_" + TransitionPartType.In;
                        var transitionIn = this.GetTransitionStoryboardByName(transitionInName);
                        this.CompletingTransition = transitionIn;

                        // Wire up the first transition to start the second transition when it's complete.
                        startingTransitionName = this.Transition + "Transition_" + TransitionPartType.Out;
                        var transitionOut = this.GetTransitionStoryboardByName(startingTransitionName);
                        transitionOut.Completed += delegate
                                                    {
                                                        VisualStateManager.GoToState(this, transitionInName, false);
                                                    };
                        this.StartingTransition = transitionOut;
                    }
                    else
                    {
                        startingTransitionName = this.Transition + "Transition_" + this.TransitionPart;
                        var transitionIn = this.GetTransitionStoryboardByName(startingTransitionName);
                        this.CompletingTransition = transitionIn;
                    }

                    // Start the transition.
                    this.isTransitioning = true;
                    this.RaiseTransitionStarted();
                    VisualStateManager.GoToState(this, startingTransitionName, false);  
                }
            }
        }

        /// <summary>
        /// Gets the name of the transition storyboard by.
        /// </summary>
        /// <param name="transitionName">Name of the transition.</param>
        /// <returns>Storyboard object</returns>
        private Storyboard GetTransitionStoryboardByName(string transitionName)
        {
            // Hook up the CurrentTransition.
            var presentationGroup = ((IEnumerable<VisualStateGroup>)VisualStateManager.GetVisualStateGroups(this.container))
                                    .Where(o => o.Name == PresentationGroup)
                                    .FirstOrDefault();
            if (presentationGroup == null)
            {
                throw new ArgumentException("Invalid VisualStateGroup.");
            }
            
            var transition = ((IEnumerable<VisualState>)presentationGroup.States).Where(o => o.Name == transitionName).Select(o => o.Storyboard).FirstOrDefault();
            if (transition == null)
            {
                throw new ArgumentException("Invalid transition");
            }

            return transition;
        }

        /// <summary>
        /// Sets default values for certain transition types.
        /// </summary>
        private void SetTransitionDefaultValues()
        {
            if (this.Transition == TransitionType.SlideLeft)
            {
                if (this.CompletingTransition != null)
                {
                    var completingDoubleAnimation = (DoubleAnimationUsingKeyFrames)this.CompletingTransition.Children[0];
                    completingDoubleAnimation.KeyFrames[1].Value = -this.ActualWidth;
                }

                if (this.StartingTransition != null)
                {
                    var startingDoubleAnimation = (DoubleAnimation)this.StartingTransition.Children[0];
                    startingDoubleAnimation.To = -this.ActualWidth;
                }

                return;
            }
        }
    }
}