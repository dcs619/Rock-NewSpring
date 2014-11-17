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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DatePicker : DataTextBox
    {
        private CheckBox _cbCurrent;

        /// <summary>
        /// Gets or sets a value indicating whether [display current option].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display current option]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayCurrentOption
        {
            get
            {
                return ViewState["DisplayCurrentOption"] as bool? ?? false;
            }

            set
            {
                ViewState["DisplayCurrentOption"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [current date].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current date]; otherwise, <c>false</c>.
        /// </value>
        public bool CurrentDate
        {
            get
            {
                if ( DisplayCurrentOption )
                {
                    EnsureChildControls();
                    return _cbCurrent.Checked;
                }

                return false;
            }

            set
            {
                EnsureChildControls();
                _cbCurrent.Checked = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.AddCssClass( "input-width-md" );
            this.AppendText = "<i class='fa fa-calendar'></i>";

            if ( string.IsNullOrWhiteSpace( this.SourceTypeName ) )
            {
                this.LabelTextFromPropertyName = false;
                this.SourceTypeName = "Rock.Web.UI.Controls.DatePicker, Rock";
                this.PropertyName = "SelectedDate";
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            RegisterJavascript();
            base.RenderControl( writer );
        }

        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            // Get current date format and make sure it has double-lower-case month and day designators for the js date picker to use
            var dateFormat = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern;
            dateFormat = dateFormat.Replace( "M", "m" ).Replace( "m", "mm" ).Replace( "mmmm", "mm" );
            dateFormat = dateFormat.Replace( "d", "dd" ).Replace( "dddd", "dd" );

            var script = string.Format( @"Rock.controls.datePicker.initialize({{ id: '{0}', startView: {1}, format: '{2}' }});", 
                this.ClientID, this.StartView.ConvertToInt(), dateFormat );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "date_picker-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Gets or sets the start view.
        /// </summary>
        /// <value>
        /// The start view.
        /// </value>
        public StartViewOption StartView
        {
            get
            {
                return ViewState["StartView"] as StartViewOption? ?? StartViewOption.month;
            }

            set
            {
                ViewState["StartView"] = value;
            }
        }

        /// <summary>
        /// The mode to start in when first displaying selection window
        /// </summary>
        public enum StartViewOption
        {
            /// <summary>
            /// Month
            /// </summary>
            month = 0,

            /// <summary>
            /// Year
            /// </summary>
            year = 1,

            /// <summary>
            /// Decade
            /// </summary>
            decade = 2
        }

        /// <summary>
        /// Gets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime? SelectedDate
        {
            get
            {
                if ( !string.IsNullOrWhiteSpace( this.Text ) )
                {
                    DateTime result;
                    if ( DateTime.TryParse( this.Text, out result ) )
                    {
                        return result.Date;
                    }
                    else
                    {
                        ShowErrorMessage( Rock.Constants.WarningMessage.DateTimeFormatInvalid( this.PropertyName ) );
                    }
                }

                return null;
            }

            set
            {
                if ( ( value ?? DateTime.MinValue ) != DateTime.MinValue )
                {
                    this.Text = value.Value.ToShortDateString();
                }
                else
                {
                    this.Text = string.Empty;
                }

                this.Attributes["value"] = this.Text;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _cbCurrent = new CheckBox();
            _cbCurrent.ID = this.ID + "_cbCurrent";
            _cbCurrent.Text = "Current Date";
            Controls.Add( _cbCurrent );
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( DisplayCurrentOption)
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if (CurrentDate)
                {
                    base.Enabled = false;
                }
            }

            base.RenderBaseControl( writer );

            if ( DisplayCurrentOption )
            {
                _cbCurrent.RenderControl( writer );
                writer.RenderEndTag();
            }
        }

    }
}