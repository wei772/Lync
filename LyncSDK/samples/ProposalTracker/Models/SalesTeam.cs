/*=====================================================================
  File:      SalesTeam.cs

  Summary:   Model class for creating a Sales Team.

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

namespace ProposalTracker.Models
{
    /// <summary>
    /// Class that models a sales team
    /// </summary>
    public static class SalesTeam
    {
        public static ObservableCollection<SalesPerson> SalesPeople { get; set; }
        public static ObservableCollection<Proposal> Proposals { get; set; }

        static SalesTeam()
        {
            SalesPeople = new ObservableCollection<SalesPerson>();
            Proposals = new ObservableCollection<Proposal>();
            LoadProposals();
            LoadSalesPeople();
        }

        public static void LoadProposals()
        {
            Proposals.Add(new Proposal
                              {
                ProjectName = "City Power and Light",
                ClosedDate = "08/01/2010",
                Description = "Discovery project with full multi-platform Widget implementation for global divisions. Estimated TTC is 9-12 months. Projected Fabrikam revenue of $1.2M. Strong client relationship, likely close.",
                SponsorUri = "sip:bob@fabrikam.com",
                Team = new List<String> { "sip:mary@fabrikam.com", "sip:kathy@fabrikam.com", "sip:martin@fabrikam.com", "sip:john@fabrikam.com", "sip:peter@fabrikam.com" }
            });
            Proposals.Add(new Proposal
                              {
                ProjectName = "Datum Corporation",
                ClosedDate = "09/20/2010",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla a interdum ligula. Quisque velit magna, rhoncus non commodo at, ultricies sed neque. Nulla quis orci quam. Phasellus nec lacus erat.",
                SponsorUri = "sip:mary@fabrikam.com",
                Team = new List<String> { "sip:bob@fabrikam.com", "sip:kathy@fabrikam.com", "sip:martin@fabrikam.com", "sip:john@fabrikam.com", "sip:peter@fabrikam.com" }
            });
            Proposals.Add(new Proposal
                              {
                ProjectName = "Northwind Traders",
                ClosedDate = "07/21/2010",
                Description = "Donec et urna nisl, vel semper neque. Donec nec erat tortor. Quisque consectetur quam eu ipsum auctor sodales. Pellentesque eget mi ut ipsum lacinia sollicitudin. Etiam dictum pulvinar ante dictum.",
                SponsorUri = "sip:kathy@fabrikam.com",
                Team = new List<String> { "sip:mary@fabrikam.com", "sip:bob@fabrikam.com", "sip:martin@fabrikam.com", "sip:john@fabrikam.com" }
            });
            Proposals.Add(new Proposal
            {
                ProjectName = "Fabrikam, Inc",
                ClosedDate = "10/13/2010",
                Description = "Phasellus pretium turpis vel felis malesuada blandit. Phasellus magna magna, vehicula sit amet dignissim a, fermentum eget ante. Pellentesque imperdiet, lectus vitae auctor eleifend, leo massa aliquet eros.",
                SponsorUri = "sip:martin@fabrikam.com",
                Team = new List<String> { "sip:mary@fabrikam.com", "sip:kathy@fabrikam.com", "sip:bob@fabrikam.com", "sip:john@fabrikam.com" }
            });
            Proposals.Add(new Proposal
            {
                ProjectName = "Proseware, Inc",
                ClosedDate = "07/17/2010",
                Description = " Proin fermentum dapibus quam eget sagittis. Cras eu iaculis lorem. Duis sit amet adipiscing tortor. Proin vel turpis gravida ipsum accumsan elementum sed vitae purus.",
                SponsorUri = "sip:john@fabrikam.com",
                Team = new List<String> { "sip:mary@fabrikam.com", "sip:kathy@fabrikam.com", "sip:martin@fabrikam.com", "sip:bob@fabrikam.com" }
            });
            Proposals.Add(new Proposal
            {
                ProjectName = "Stonecold Manufacturing",
                ClosedDate = "12/12/2012",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla a interdum ligula. Quisque velit magna, rhoncus non commodo at, ultricies sed neque. Nulla quis orci quam. Phasellus nec lacus erat.",
                SponsorUri = "sip:mary@fabrikam.com",
                Team = new List<String> { "sip:bob@fabrikam.com", "sip:kathy@fabrikam.com", "sip:martin@fabrikam.com", "sip:john@fabrikam.com" }
            });
        }

        public static void LoadSalesPeople()
        {
            SalesPeople.Add(new SalesPerson { SalesPersonUri = "sip:bob@fabrikam.com", TotalSales = 200 });
            SalesPeople.Add(new SalesPerson { SalesPersonUri = "sip:mary@fabrikam.com", TotalSales = 159 });
            SalesPeople.Add(new SalesPerson { SalesPersonUri = "sip:martin@fabrikam.com", TotalSales = 137 });
            SalesPeople.Add(new SalesPerson { SalesPersonUri = "sip:kathy@fabrikam.com", TotalSales = 146 });
            SalesPeople.Add(new SalesPerson { SalesPersonUri = "sip:john@fabrikam.com", TotalSales = 109 });
            SalesPeople.Add(new SalesPerson { SalesPersonUri = "sip:sam@fabrikam.com", TotalSales = 112 });
        }
    }
}
