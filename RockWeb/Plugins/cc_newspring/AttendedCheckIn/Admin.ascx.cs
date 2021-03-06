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
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.AttendedCheckin
{
    /// <summary>
    /// Admin block for Attended Check-in
    /// </summary>
    [DisplayName( "Check-in Administration" )]
    [Category( "Check-in > Attended" )]
    [Description( "Check-In Administration block" )]
    [BooleanField( "Enable Location Sharing", "If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.", false, "Geo Location", 0 )]
    [IntegerField( "Time to Cache Kiosk GeoLocation", "Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.", false, 20, "Geo Location", 1 )]
    public partial class Admin : CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // Set the check-in state from values passed on query string
                CurrentKioskId = PageParameter( "KioskId" ).AsIntegerOrNull();

                CurrentGroupTypeIds = ( PageParameter( "GroupTypeIds" ) ?? "" )
                    .Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .ToList()
                    .Select( s => s.AsInteger() )
                    .ToList();

                if ( CurrentKioskId.HasValue && CurrentGroupTypeIds != null && CurrentGroupTypeIds.Any() && !UserBackedUp )
                {
                    // Save the check-in state
                    SaveState();

                    // Navigate to the next page
                    NavigateToNextPage();
                }
                else
                {
                    RockPage.AddScriptLink( "~/Blocks/CheckIn/Scripts/geo-min.js" );

                    AttemptKioskMatchByIpOrName();

                    string script = string.Format( @"
                <script>
                    $(document).ready(function (e) {{
                        if (localStorage) {{
                            if (localStorage.checkInKiosk) {{
                                $('[id$=""hfKiosk""]').val(localStorage.checkInKiosk);
                                if (localStorage.checkInGroupTypes) {{
                                    $('[id$=""hfGroupTypes""]').val(localStorage.checkInGroupTypes);
                                }}
                            }}
                            {0};
                        }}
                    }});
                </script>
                ", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                    phScript.Controls.Add( new LiteralControl( script ) );

                    // Initiate the check-in variables
                    lbOk.Focus();
                    SaveState();
                }
            }
            else
            {
                phScript.Controls.Clear();
            }
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        private void AttemptKioskMatchByIpOrName()
        {
            // match kiosk by ip/name.
            string hostIp = Request.ServerVariables["REMOTE_ADDR"];
            string forwardedIp = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string ipAddress = forwardedIp ?? hostIp;
            bool skipDeviceNameLookup = false;

            var rockContext = new RockContext();
            var checkInDeviceTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            var device = new DeviceService( rockContext ).GetByIPAddress( ipAddress, checkInDeviceTypeId, skipDeviceNameLookup );

            var checkInDeviceTypeGuid = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Guid;
            var deviceList = new DeviceService( rockContext ).GetByDeviceTypeGuid( checkInDeviceTypeGuid ).AsNoTracking().ToList();

            string hostName = string.Empty;
            try
            {
                hostName = System.Net.Dns.GetHostEntry( ipAddress ).HostName ?? ipAddress;
            }
            catch ( System.Net.Sockets.SocketException )
            {
                hostName = "Unknown";
            }

            lblInfo.Text = string.Format( "Device IP: {0}     Name: {1}", ipAddress, hostName );

            if ( device != null )
            {
                ClearMobileCookie();
                CurrentKioskId = device.Id;
            }
            else
            {
                maAlert.Show( "This device has not been set up for check-in.", ModalAlertType.Alert );
                lbOk.Text = @"<span class='fa fa-refresh' />";
                lbOk.Enabled = false;
                pnlHeader.Update();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                maAlert.Show( "Check-in state timed out.  Please refresh the page.", ModalAlertType.Warning );
                pnlContent.Update();
                return;
            }

            if ( CurrentKioskId == null || CurrentKioskId == 0 )
            {
                CurrentKioskId = hfKiosk.ValueAsInt();
            }

            var selectedGroupTypes = hfGroupTypes.Value.SplitDelimitedValues().Select( int.Parse ).Distinct().ToList();
            if ( !selectedGroupTypes.Any() )
            {
                foreach ( DataListItem item in dlMinistry.Items )
                {
                    ( (Button)item.FindControl( "lbMinistry" ) ).RemoveCssClass( "active" );
                }

                maAlert.Show( "Please select at least one check-in type.", ModalAlertType.Warning );
                pnlContent.Update();
                return;
            }

            // return if kiosk isn't active
            if ( !CurrentCheckInState.Kiosk.HasActiveLocations( selectedGroupTypes ) )
            {
                maAlert.Show( "There are no active schedules for the selected grouptypes.", ModalAlertType.Information );
                pnlContent.Update();
                return;
            }

            ClearMobileCookie();
            CurrentGroupTypeIds = selectedGroupTypes;
            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();
            NavigateToNextPage();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the dlMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void dlMinistry_ItemDataBound( object sender, DataListItemEventArgs e )
        {
            var selectedGroupTypes = hfGroupTypes.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
            if ( selectedGroupTypes.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( selectedGroupTypes.Contains( ( (GroupType)e.Item.DataItem ).Id ) )
                    {
                        ( (Button)e.Item.FindControl( "lbMinistry" ) ).AddCssClass( "active" );
                    }
                }
            }
        }

        #endregion Events

        #region GeoLocation related

        /// <summary>
        /// Adds GeoLocation script and calls its init() to get client's latitude/longitude before firing
        /// the server side lbCheckGeoLocation_Click click event. Puts the two values into the two corresponding
        /// hidden varialbles, hfLatitude and hfLongitude.
        /// </summary>
        private void AddGeoLocationScript()
        {
            string geoScript = string.Format( @"
            <script>
                $(document).ready(function (e) {{
                    tryGeoLocation();

                    function tryGeoLocation() {{
                        if ( geo_position_js.init() ) {{
                            geo_position_js.getCurrentPosition(success_callback, error_callback, {{ enableHighAccuracy: true }});
                        }}
                        else {{
                            $(""div.checkin-header h1"").html( ""We're Sorry!"" );
                            $(""div.checkin-header h1"").after( ""<p>We don't support that kind of device yet. Please check-in using the on-site kiosks.</p>"" );
                            alert(""We We don't support that kind of device yet. Please check-in using the on-site kiosks."");
                        }}
                    }}

                    function success_callback( p ) {{
                        var latitude = p.coords.latitude.toFixed(4);
                        var longitude = p.coords.longitude.toFixed(4);
                        $(""input[id$='hfLatitude']"").val( latitude );
                        $(""input[id$='hfLongitude']"").val( longitude );
                        $(""div.checkin-header h1"").html( 'Checking Your Location...' );
                        $(""div.checkin-header"").append( ""<p class='muted'>"" + latitude + "" "" + longitude + ""</p>"" );
                        // now perform a postback to fire the check geo location
                        {0};
                    }}

                    function error_callback( p ) {{
                        // TODO: decide what to do in this situation...
                        alert( 'error=' + p.message );
                    }}
                }});
            </script>
            ", this.Page.ClientScript.GetPostBackEventReference( lbCheckGeoLocation, "" ) );
            phScript.Controls.Add( new LiteralControl( geoScript ) );
        }

        /// <summary>
        /// Used by the local storage script to rebind the group types if they were previously
        /// saved via local storage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            BindGroupTypes();
        }

        /// <summary>
        /// Handles attempting to find a registered Device kiosk by it's latitude and longitude.
        /// This event method is called automatically when the GeoLocation script get's the client's location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbCheckGeoLocation_Click( object sender, EventArgs e )
        {
            var lat = hfLatitude.Value;
            var lon = hfLongitude.Value;
            Device kiosk = null;

            if ( !string.IsNullOrEmpty( lat ) && !string.IsNullOrEmpty( lon ) )
            {
                kiosk = GetCurrentKioskByGeoFencing( lat, lon );
            }

            if ( kiosk != null )
            {
                SetDeviceIdCookie( kiosk );
                CurrentKioskId = kiosk.Id;
            }
        }

        /// <summary>
        /// Returns a kiosk based on finding a geo location match for the given latitude and longitude.
        /// </summary>
        /// <param name="sLatitude">latitude as string</param>
        /// <param name="sLongitude">longitude as string</param>
        /// <returns></returns>
        public static Device GetCurrentKioskByGeoFencing( string sLatitude, string sLongitude )
        {
            var rockContext = new RockContext();
            double latitude = double.Parse( sLatitude );
            double longitude = double.Parse( sLongitude );
            var checkInDeviceType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ), rockContext );

            // We need to use the DeviceService until we can get the GeoFence to JSON Serialize/Deserialize.
            Device kiosk = new DeviceService( rockContext ).GetByGeocode( latitude, longitude, checkInDeviceType.Id );

            return kiosk;
        }

        #endregion GeoLocation related

        #region Storage Methods

        /// <summary>
        /// Sets the "DeviceId" cookie to expire after TimeToCacheKioskGeoLocation minutes
        /// if IsMobile is set.
        /// </summary>
        /// <param name="kiosk"></param>
        private void SetDeviceIdCookie( Device kiosk )
        {
            // set an expiration cookie for these coordinates.
            double timeCacheMinutes = double.Parse( GetAttributeValue( "TimetoCacheKioskGeoLocation" ) ?? "0" );

            HttpCookie deviceCookie = Request.Cookies[CheckInCookie.DEVICEID];
            if ( deviceCookie == null )
            {
                deviceCookie = new HttpCookie( CheckInCookie.DEVICEID, kiosk.Id.ToString() );
            }

            deviceCookie.Expires = ( timeCacheMinutes == 0 ) ? DateTime.MaxValue : RockDateTime.Now.AddMinutes( timeCacheMinutes );
            Response.Cookies.Set( deviceCookie );

            HttpCookie isMobileCookie = new HttpCookie( CheckInCookie.ISMOBILE, "true" );
            Response.Cookies.Set( isMobileCookie );
        }

        /// <summary>
        /// Clears the flag cookie that indicates this is a "mobile" device kiosk.
        /// </summary>
        private void ClearMobileCookie()
        {
            HttpCookie isMobileCookie = new HttpCookie( CheckInCookie.ISMOBILE );
            isMobileCookie.Expires = RockDateTime.Now.AddDays( -1d );
            Response.Cookies.Set( isMobileCookie );
        }

        #endregion Storage Methods

        #region Internal Methods

        /// <summary>
        /// Binds the group types.
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void BindGroupTypes( RockContext rockContext = null )
        {
            if ( CurrentKioskId > 0 )
            {
                dlMinistry.DataSource = GetDeviceGroupTypes( (int)CurrentKioskId, rockContext );
                dlMinistry.DataBind();
                lblHeader.Visible = true;
            }
        }

        /// <summary>
        /// Gets the device group types.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        private List<GroupType> GetDeviceGroupTypes( int deviceId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var groupTypes = new Dictionary<int, GroupType>();

            var locationService = new LocationService( rockContext );

            // Get all locations (and their children) associated with device
            var locationIds = locationService
                .GetByDevice( deviceId, true )
                .Select( l => l.Id )
                .ToList();

            // Requery using EF
            foreach ( var groupType in locationService
                .Queryable().AsNoTracking()
                .Where( l => locationIds.Contains( l.Id ) )
                .SelectMany( l => l.GroupLocations )
                .Where( gl => gl.Group.GroupType.TakesAttendance )
                .Select( gl => gl.Group.GroupType )
                .ToList() )
            {
                groupTypes.AddOrIgnore( groupType.Id, groupType );
            }

            return groupTypes
                .Select( g => g.Value )
                .OrderBy( g => g.Order )
                .ToList();
        }

        #endregion Internal Methods
    }
}