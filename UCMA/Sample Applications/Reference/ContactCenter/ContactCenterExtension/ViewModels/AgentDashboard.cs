/*=====================================================================
  File:      AgentDashboard.cs

  Summary:   View Model for AgentDashboadView.

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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Lync.Samples.ContactCenterExtension.Models;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region IAgentDashboard Interface

    /// <summary>
    /// AgentDashboard View-Model Contract
    /// </summary>
    public interface IAgentDashboard
    {
        Uri ProductImage { get; }

        String ProductTitle { get; }

        String ProductDescription { get; }

        String ProductPrice { get; }

        Int32 ProductQuantity { get; }

        Double ProductId { get; }

        Boolean HasContext { get; }

        String Status { get; }

        ObservableCollection<ISkillViewModel> Skills { get; }

        Boolean IsOnHold { get; }

        ICommand HoldRetrieveCommand { get; }

        ICommand EscalateToExpertCommand { get; }

        ICommand ClearSkillsCommand { get; }
    }

    #endregion

    /// <summary>
    /// Represents the  View Model for AgentDashboadView.
    /// </summary>
    public class AgentDashboard : ViewModel<AgentDashboardChannel>, IAgentDashboard
    {
        #region Fields

        private const string ApplicationGuid = "{63D37F02-47B3-4B9E-AA8E-FEF3665298DC}";
        private readonly Command _holdRetrieveCommand;
        private readonly Command _escalateToExpertCommand;
        private readonly Command _clearSkillsCommand;

        private bool _isOnHold;
        private bool _isHoldRetrieveInProgress;
        private readonly ObservableCollection<ISkillViewModel> _skills;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Agent Dashboard Properties
        /// </summary>
        public enum Properties
        {
            ProductDescription,
            Skills,
            IsOnHold,
            IsEscalateInProgress
        }

        public Uri ProductImage
        {
            get
            {
                if (Model.ProductInfo != null && !String.IsNullOrEmpty(Model.ProductInfo.productImage))
                {
                    string[] tokens = Model.ProductInfo.productImage.Split(new[] { '/' });

                    string file = tokens[tokens.Length - 1];

                    return new Uri("/ContactCenterExtension;component/Images/" + file, UriKind.RelativeOrAbsolute);
                }
                return null;
            }
        }

        public String ProductTitle
        {
            get { return Model.ProductInfo.productTitle; }
        }

        public String ProductDescription
        {
            get { return Model.ProductInfo.productDescription; }
        }

        public String ProductPrice
        {
            get
            {
                if (Model.ProductInfo != null && Model.ProductInfo.productPrice != null)
                {
                    return String.Format("{0:C}", Double.Parse(Model.ProductInfo.productPrice));
                }
                return null;
            }
        }

        public Int32 ProductQuantity
        {
            get { return 2; } //Sample data
        }

        public Double ProductId
        {
            get { return 1629; } //Sample data
        }

        public Boolean HasContext
        {
            get
            {
                return (Model.ProductInfo != null && !String.IsNullOrWhiteSpace(Model.ProductInfo.productGuid));
            }
        }

        public String Status
        {
            get { return "In Call"; } //Sample data
        }

        private bool IsHoldRetrieveInProgress
        {
            get { return _isHoldRetrieveInProgress; }
            set
            {
                _isHoldRetrieveInProgress = value;
                ((Command)HoldRetrieveCommand).NotifyCanExecuteChanged();
            }
        }

        public List<Order> Orders
        {
            get
            {
                return new List<Order> //Sample data
                           {
                            new Order
                                {
                                Date = "08/29/2012",
                                OrderNumber = "8777",
                                Product = "NOKIA LUMIA 900 (CYAN)",
                                Status = "Pending"
                                },
                            new Order
                                {
                                Date = "05/25/2010",
                                OrderNumber = "4734",
                                Product = "SCREEN PROTECTOR",
                                Status = "Returned"
                                },
                            new Order
                                {
                                Date = "05/15/2010",
                                OrderNumber = "4168",
                                Product = "CAR CHARGER",
                                Status = "Returned"
                                },
                            new Order
                                {
                                Date = "04/14/2010",
                                OrderNumber = "9856",
                                Product = "HTC TOUCH PRO2",
                                Status = "Delivered"
                                }
                           };
            }
        }

        public ObservableCollection<ISkillViewModel> Skills
        {
            get { return _skills; }
        }

        public bool IsOnHold
        {
            get { return _isOnHold; }
            private set
            {
                _isOnHold = value;
                NotifyPropertyChanged(Properties.IsOnHold.ToString());
            }
        }


        #endregion Properties

        #region Commands

        public ICommand HoldRetrieveCommand
        {
            get
            {
                return _holdRetrieveCommand;
            }
        }

        public ICommand EscalateToExpertCommand
        {
            get
            {
                return _escalateToExpertCommand;
            }
        }

        /// <summary>
        /// Clears all selected skills.
        /// </summary>
        public ICommand ClearSkillsCommand
        {
            get
            {
                return _clearSkillsCommand;
            }
        }

        #endregion Commands

        #region Constructors

        public AgentDashboard()
            : base(new AgentDashboardChannel(ApplicationGuid))
        {
            _holdRetrieveCommand = new Command { CanExecute = CanExecuteHoldRetrieve, Execute = ExecuteHoldRetrieve };
            _escalateToExpertCommand = new Command { CanExecute = CanExecuteEscalateToExpert, Execute = ExecuteEscalateToExpert };
            _clearSkillsCommand = new Command { CanExecute = CanExecuteClearSkills, Execute = ExecuteClearSkills };

            var skills = new List<ISkillViewModel>();

            foreach (skillType s in Model.Skills)
            {
                Skill skill = new Skill(s.name);
                skill.Values = new List<string>(s.skillValues);
                var skillVm = new SkillViewModel(skill);
                skillVm.PropertyChanged += SkillVmPropertyChanged;
                skills.Add(skillVm);
            }
            _skills = new ObservableCollection<ISkillViewModel>(skills);
        }

        #endregion Constructors

        #region Commands Handlers

        private bool CanExecuteHoldRetrieve(object param)
        {
            return !IsHoldRetrieveInProgress;
        }

        private void ExecuteHoldRetrieve(object param)
        {
            if (IsOnHold)
            {
                ExecuteRetrieve(param);
            }
            else
            {
                ExecuteHold(param);
            }
        }

        private bool CanExecuteEscalateToExpert(object param)
        {
            return Skills.Any(s => s.HasSelectedTopic);
        }

        private void ExecuteEscalateToExpert(object param)
        {
            try
            {
                List<agentSkillType> agentSkills = new List<agentSkillType>();
                foreach (SkillViewModel svm in _skills)
                {
                    if (svm.HasSelectedTopic)
                    {
                        agentSkillType agentSkill = new agentSkillType();
                        agentSkill.name = svm.Category;
                        agentSkill.Value = svm.SelectedTopic.DisplayName;
                        agentSkills.Add(agentSkill);
                    }
                }


                if (agentSkills.Count > 0)
                {
                    Model.BeginEscalate(agentSkills, ar =>
                    {
                        try
                        {
                            Model.EndEscalate(ar);
                        }
                        catch (Exception)
                        {

                        }
                    }, null);
                }
            }
            catch (Exception)
            {

            }
        }

        private bool CanExecuteClearSkills(object param)
        {
            return true;
        }

        private void ExecuteClearSkills(object param)
        {
            foreach (SkillViewModel svm in _skills)
            {
                svm.ClearSelectedTopicCommand.Execute(null);
            }
        }

        private void ExecuteHold(object param)
        {
            try
            {
                Model.BeginHold(ar =>
                {
                    try
                    {
                        Model.EndHold(ar);
                        IsOnHold = true;
                    }
                    catch (Exception)
                    {
                        IsOnHold = false;
                    }
                    finally
                    {
                        IsHoldRetrieveInProgress = false;
                    }
                }, null);

                IsHoldRetrieveInProgress = true;
            }
            catch (Exception)
            {
                IsOnHold = false;
                IsHoldRetrieveInProgress = false;
            }
        }

        private void ExecuteRetrieve(object param)
        {
            try
            {
                Model.BeginRetrieve(ar =>
                {
                    try
                    {
                        Model.EndRetrieve(ar);
                        IsOnHold = false;
                    }
                    catch (Exception)
                    {
                        IsOnHold = true;
                    }
                    finally
                    {
                        IsHoldRetrieveInProgress = false;
                    }
                }, null);

                IsHoldRetrieveInProgress = true;
            }
            catch (Exception)
            {
                IsOnHold = true;
                IsHoldRetrieveInProgress = false;
            }
        }
        #endregion Commands Handlers

        #region Event Handlers

        void SkillVmPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ((Command)EscalateToExpertCommand).NotifyCanExecuteChanged();
        }

        #endregion
    }
}
