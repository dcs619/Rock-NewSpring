﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.ScheduleContextSetter
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Schedule Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default schedule context for the site." )]
    
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0 )]
    [SchedulesField( "Schedule Group", "Choose a schedule group to populate the dropdown", order: 1 )]
    [TextField( "Current Item Template", "Lava template for the current item. The only merge field is {{ ScheduleName }}.", true, "{{ ScheduleName }}", order: 2 )]
    [TextField( "Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.", true, "{{ ScheduleName }}", order: 2 )]
    [TextField( "No Schedule Text", "The text to show when there is no schedule in the context.", true, "All Schedules", order: 3 )]

    public partial class ScheduleContextSetter : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Request.QueryString["scheduleId"] != null )
            {
                SetScheduleContext();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
            }
        }

        private void SetContextUrlCookie()
        {
            HttpCookie cookieUrl = new HttpCookie( "Rock.Schedule.Context.Query" );
            cookieUrl["scheduleId"] = Request.QueryString["scheduleId"].ToString();
            cookieUrl.Expires = DateTime.Now.AddHours( 1 );
            Response.Cookies.Add( cookieUrl );
        }

        private void ClearRockContext( string cookieName )
        {
            var cookieKeys = Request.Cookies[ cookieName ].Value.Split( '&' ).ToArray();

            HttpCookie newRockCookie = new HttpCookie( cookieName );

            foreach ( var cookieKey in cookieKeys )
            {

                if ( !cookieKey.ToString().StartsWith( "Rock.Model.Schedule" ) )
                {
                    var cookieValue = cookieKey.Split( '=' );

                    var cookieId = cookieValue[0].ToString();
                    var cookieHash = cookieValue[1].ToString();

                    newRockCookie[cookieId] = cookieHash;
                }
            }

            newRockCookie.Expires = DateTime.Now.AddHours( 1 );
            Response.Cookies.Add( newRockCookie );
        }

        private void SetScheduleContext()
        {

           
            var scheduleContextQuery = Request.QueryString["scheduleId"];

            if ( scheduleContextQuery != null )
            {
                bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";

                var schedule = new ScheduleService( new RockContext() ).Get( scheduleContextQuery.ToString().AsInteger() );

                HttpCookie cookieUrl = Request.Cookies["Rock.Schedule.Context.Query"];

                if ( schedule != null )
                {

                    if ( cookieUrl == null || Request.QueryString["scheduleId"].ToString() != cookieUrl.Value.Replace( "scheduleId=", "" ) )
                    {
                        SetContextUrlCookie();
                        RockPage.SetContextCookie( schedule, pageScope, true );
                    }
                }
                else
                {
                    if ( cookieUrl == null || Request.QueryString["scheduleId"].ToString() != cookieUrl.Value.Replace( "scheduleId=", "" ) )
                    {
                        SetContextUrlCookie();

                        // Check for a page specific Rock Context Cookie
                        if ( Request.Cookies["Rock_Context:" + RockPage.PageId.ToString()] != null )
                        {
                            ClearRockContext("Rock_Context:" + RockPage.PageId.ToString());
                        }

                        // Check for a site specific Rock Context Cookie
                        if ( Request.Cookies["Rock_Context"] != null )
                        {
                            ClearRockContext("Rock_Context");   
                        }

                        // Refresh the page once we modify the cookies
                        Response.Redirect( Request.Url.ToString() );
                    }
                }
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();

            var scheduleEntityType = EntityTypeCache.Read( "Rock.Model.Schedule" );
            var defaultSchedule = RockPage.GetCurrentContext( scheduleEntityType ) as Schedule;
             
            if ( defaultSchedule != null )
            {
                mergeObjects.Add( "ScheduleName", defaultSchedule.Name );
                lCurrentSelection.Text = GetAttributeValue( "CurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( "NoScheduleText" );
            }

            List<ScheduleItem> schedules = new List<ScheduleItem>();
            schedules.Add( new ScheduleItem { Name = GetAttributeValue( "NoScheduleText" ), Id = Rock.Constants.All.ListItem.Value.AsInteger() } );

            if ( GetAttributeValue( "ScheduleGroup" ) != null )
            {
                var selectedSchedule = GetAttributeValue( "ScheduleGroup" );
                var selectedScheduleList = selectedSchedule.Split( ',' ).AsGuidList();

                schedules.AddRange( new ScheduleService( new RockContext() ).Queryable()
                    .Where( a => selectedScheduleList.Contains( a.Guid ) )
                    .Select( a => new ScheduleItem { Name = a.Name, Id = a.Id } )
                );
            }

            var formattedSchedule = new Dictionary<int, string>();
            // run lava on each campus
            foreach ( var schedule in schedules )
            {
                mergeObjects.Clear();
                mergeObjects.Add( "ScheduleName", schedule.Name );
                schedule.Name = GetAttributeValue( "DropdownItemTemplate" ).ResolveMergeFields( mergeObjects );
            }

            rptSchedules.DataSource = schedules;
            rptSchedules.DataBind();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptSchedules_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var schedule = new ScheduleService( new RockContext() ).Get( e.CommandArgument.ToString().AsInteger() );

            var nameValues = HttpUtility.ParseQueryString( Request.QueryString.ToString() );
            nameValues.Set( "scheduleId", e.CommandArgument.ToString() );
            string url = Request.Url.AbsolutePath;
            string updatedQueryString = "?" + nameValues.ToString();

            // Only update the Context Cookie if the Schedule is valid
            if ( schedule != null )
            {
                RockPage.SetContextCookie( schedule, pageScope, false );
            }

            Response.Redirect( url + updatedQueryString );
        }

        #endregion

        /// <summary>
        /// Schedule Item
        /// </summary>
        public class ScheduleItem
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
        }
    }
}