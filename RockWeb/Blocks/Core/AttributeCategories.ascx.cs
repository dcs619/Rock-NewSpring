// <copyright>
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for managing attribute categories 
    /// </summary>
    [DisplayName( "Attribute Categories" )]
    [Category( "Core" )]
    [Description( "Allows attribute categories to be managed." )]
    public partial class AttributeCategories : RockBlock
    {
        #region Fields

        bool _canConfigure = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Load Entity Type Filter
            var entityTypes = new EntityTypeService( new RockContext() ).GetEntities().OrderBy( t => t.FriendlyName ).ToList();
            entityTypeFilter.EntityTypes = entityTypes;
            entityTypePicker.EntityTypes = entityTypes;

            _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.Actions.ShowAdd = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridReorder += rGrid_GridReorder;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
            }
            else
            {
                nbMessage.Text = "You are not authorized to configure this page";
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _canConfigure )
                {
                    BindGrid();
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    modalDetails.Show();
                }
            }


            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "EntityType", entityTypeFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "EntityType":

                    if ( e.Value != "" )
                    {
                        if ( e.Value == "0" )
                        {
                            e.Value = "None (Global Attributes)";
                        }
                        else
                        {
                            e.Value = EntityTypeCache.Read( int.Parse( e.Value ) ).FriendlyName;
                        }
                    }
                    break;
            }

        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)rGrid.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CategoryService( rockContext );

            var category = service.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
            if ( category != null )
            {
                string errorMessage = string.Empty;
                if ( service.CanDelete( category, out errorMessage ) )
                {

                    service.Delete( category );

                    rockContext.SaveChanges();
                }
                else
                {
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Literal lEntityType = e.Row.FindControl( "lEntityType" ) as Literal;
                if ( lEntityType != null )
                {
                    lEntityType.Text = "None (Global Attributes)";

                    int categoryId = (int)rGrid.DataKeys[e.Row.RowIndex].Value;
                    var category = CategoryCache.Read( categoryId );

                    int entityTypeId = int.MinValue;
                    if ( category != null &&
                        !string.IsNullOrWhiteSpace( category.EntityTypeQualifierValue ) &&
                        int.TryParse( category.EntityTypeQualifierValue, out entityTypeId ) &&
                        entityTypeId > 0 )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeId );
                        if ( entityType != null )
                        {
                            lEntityType.Text = entityType.FriendlyName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var categories = GetCategories();
            if ( categories != null )
            {
                var rockContext = new RockContext();
                new CategoryService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            int categoryId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out categoryId ) )
            {
                categoryId = 0;
            }

            var rockContext = new RockContext();
            var service = new CategoryService( rockContext );
            Category category = null;

            if ( categoryId != 0 )
            {
                CategoryCache.Flush( categoryId );
                category = service.Get( categoryId );
            }

            if ( category == null )
            {
                category = new Category();
                category.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
                category.EntityTypeQualifierColumn = "EntityTypeId";

                var lastCategory = GetUnorderedCategories()
                    .OrderByDescending( c => c.Order ).FirstOrDefault();
                category.Order = lastCategory != null ? lastCategory.Order + 1 : 0;

                service.Add( category );
            }

            category.Name = tbName.Text;
            category.Description = tbDescription.Text;

            string QualifierValue = null;
            if ( ( entityTypePicker.SelectedEntityTypeId ?? 0 ) != 0 )
            {
                QualifierValue = entityTypePicker.SelectedEntityTypeId.ToString();
            }
            category.EntityTypeQualifierValue = QualifierValue;

            category.IconCssClass = tbIconCssClass.Text;
            category.HighlightColor = tbHighlightColor.Text;

            List<int> orphanedBinaryFileIdList = new List<int>();

            if ( category.IsValid )
            {
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                foreach ( int binaryFileId in orphanedBinaryFileIdList )
                {
                    var binaryFile = binaryFileService.Get( binaryFileId );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                    }
                }

                rockContext.SaveChanges();

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            entityTypeFilter.SelectedValue = rFilter.GetUserPreference( "EntityType" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            rGrid.DataSource = GetCategories().ToList();
            rGrid.DataBind();
        }

        private IQueryable<Category> GetCategories()
        {
            return GetUnorderedCategories()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name );
        }

        private IQueryable<Category> GetUnorderedCategories()
        {
            string selectedValue = rFilter.GetUserPreference( "EntityType" );

            var attributeEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
            var queryable = new CategoryService( new RockContext() ).Queryable()
                .Where( c => c.EntityTypeId == attributeEntityTypeId );

            if ( !string.IsNullOrWhiteSpace( selectedValue ) )
            {
                if ( selectedValue == "0" )
                {
                    queryable = queryable
                        .Where( c =>
                            c.EntityTypeQualifierColumn == "EntityTypeId" &&
                            c.EntityTypeQualifierValue == null );
                }
                else
                {
                    queryable = queryable
                        .Where( c =>
                            c.EntityTypeQualifierColumn == "EntityTypeId" &&
                            c.EntityTypeQualifierValue == selectedValue );
                }
            }

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int categoryId )
        {
            var category = new CategoryService( new RockContext() ).Get( categoryId );

            if ( category != null )
            {
                tbName.Text = category.Name;
                tbDescription.Text = category.Description;
                tbIconCssClass.Text = category.IconCssClass;
                tbHighlightColor.Text = category.HighlightColor;
            }
            else
            {
                tbName.Text = string.Empty;
                tbDescription.Text = string.Empty;
                tbIconCssClass.Text = string.Empty;
            }

            int entityTypeId = 0;
            if ( category == null || category.EntityTypeQualifierValue == null ||
                !int.TryParse( category.EntityTypeQualifierValue, out entityTypeId ) )
            {
                entityTypeId = 0;
            }

            if ( entityTypeId == 0 )
            {
                var filterValue = rFilter.GetUserPreference( "EntityType" );
                if ( !string.IsNullOrWhiteSpace( filterValue ) )
                {
                    if ( !int.TryParse( filterValue, out entityTypeId ) )
                    {
                        entityTypeId = 0;
                    }
                }
            }

            entityTypePicker.SelectedEntityTypeId = entityTypeId;

            hfIdValue.Value = categoryId.ToString();
            modalDetails.Show();
        }

        #endregion
    }
}