//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data Transfer Object for Site object
    /// </summary>
    [Serializable]
    [DataContract]
    public partial class SiteDto : IDto, DotLiquid.ILiquidizable
    {
        /// <summary />
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary />
        [DataMember]
        public string Name { get; set; }

        /// <summary />
        [DataMember]
        public string Description { get; set; }

        /// <summary />
        [DataMember]
        public string Theme { get; set; }

        /// <summary />
        [DataMember]
        public int? DefaultPageId { get; set; }

        /// <summary />
        [DataMember]
        public string FaviconUrl { get; set; }

        /// <summary />
        [DataMember]
        public string AppleTouchIconUrl { get; set; }

        /// <summary />
        [DataMember]
        public string FacebookAppId { get; set; }

        /// <summary />
        [DataMember]
        public string FacebookAppSecret { get; set; }

        /// <summary />
        [DataMember]
        public string LoginPageReference { get; set; }

        /// <summary />
        [DataMember]
        public string RegistrationPageReference { get; set; }

        /// <summary />
        [DataMember]
        public string ErrorPage { get; set; }

        /// <summary />
        [DataMember]
        public int Id { get; set; }

        /// <summary />
        [DataMember]
        public Guid Guid { get; set; }

        /// <summary>
        /// Instantiates a new DTO object
        /// </summary>
        public SiteDto ()
        {
        }

        /// <summary>
        /// Instantiates a new DTO object from the entity
        /// </summary>
        /// <param name="site"></param>
        public SiteDto ( Site site )
        {
            CopyFromModel( site );
        }

        /// <summary>
        /// Creates a dictionary object.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "IsSystem", this.IsSystem );
            dictionary.Add( "Name", this.Name );
            dictionary.Add( "Description", this.Description );
            dictionary.Add( "Theme", this.Theme );
            dictionary.Add( "DefaultPageId", this.DefaultPageId );
            dictionary.Add( "FaviconUrl", this.FaviconUrl );
            dictionary.Add( "AppleTouchIconUrl", this.AppleTouchIconUrl );
            dictionary.Add( "FacebookAppId", this.FacebookAppId );
            dictionary.Add( "FacebookAppSecret", this.FacebookAppSecret );
            dictionary.Add( "LoginPageReference", this.LoginPageReference );
            dictionary.Add( "RegistrationPageReference", this.RegistrationPageReference );
            dictionary.Add( "ErrorPage", this.ErrorPage );
            dictionary.Add( "Id", this.Id );
            dictionary.Add( "Guid", this.Guid );
            return dictionary;
        }

        /// <summary>
        /// Creates a dynamic object.
        /// </summary>
        /// <returns></returns>
        public virtual dynamic ToDynamic()
        {
            dynamic expando = new ExpandoObject();
            expando.IsSystem = this.IsSystem;
            expando.Name = this.Name;
            expando.Description = this.Description;
            expando.Theme = this.Theme;
            expando.DefaultPageId = this.DefaultPageId;
            expando.FaviconUrl = this.FaviconUrl;
            expando.AppleTouchIconUrl = this.AppleTouchIconUrl;
            expando.FacebookAppId = this.FacebookAppId;
            expando.FacebookAppSecret = this.FacebookAppSecret;
            expando.LoginPageReference = this.LoginPageReference;
            expando.RegistrationPageReference = this.RegistrationPageReference;
            expando.ErrorPage = this.ErrorPage;
            expando.Id = this.Id;
            expando.Guid = this.Guid;
            return expando;
        }

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public void CopyFromModel( IEntity model )
        {
            if ( model is Site )
            {
                var site = (Site)model;
                this.IsSystem = site.IsSystem;
                this.Name = site.Name;
                this.Description = site.Description;
                this.Theme = site.Theme;
                this.DefaultPageId = site.DefaultPageId;
                this.FaviconUrl = site.FaviconUrl;
                this.AppleTouchIconUrl = site.AppleTouchIconUrl;
                this.FacebookAppId = site.FacebookAppId;
                this.FacebookAppSecret = site.FacebookAppSecret;
                this.LoginPageReference = site.LoginPageReference;
                this.RegistrationPageReference = site.RegistrationPageReference;
                this.ErrorPage = site.ErrorPage;
                this.Id = site.Id;
                this.Guid = site.Guid;
            }
        }

        /// <summary>
        /// Copies the DTO property values to the entity properties
        /// </summary>
        /// <param name="model">The model.</param>
        public void CopyToModel ( IEntity model )
        {
            if ( model is Site )
            {
                var site = (Site)model;
                site.IsSystem = this.IsSystem;
                site.Name = this.Name;
                site.Description = this.Description;
                site.Theme = this.Theme;
                site.DefaultPageId = this.DefaultPageId;
                site.FaviconUrl = this.FaviconUrl;
                site.AppleTouchIconUrl = this.AppleTouchIconUrl;
                site.FacebookAppId = this.FacebookAppId;
                site.FacebookAppSecret = this.FacebookAppSecret;
                site.LoginPageReference = this.LoginPageReference;
                site.RegistrationPageReference = this.RegistrationPageReference;
                site.ErrorPage = this.ErrorPage;
                site.Id = this.Id;
                site.Guid = this.Guid;
            }
        }

        /// <summary>
        /// Converts to liquidizable object for dotLiquid templating
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this.ToDictionary();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public static class SiteDtoExtension
    {
        /// <summary>
        /// To the model.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static Site ToModel( this SiteDto value )
        {
            Site result = new Site();
            value.CopyToModel( result );
            return result;
        }

        /// <summary>
        /// To the model.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static List<Site> ToModel( this List<SiteDto> value )
        {
            List<Site> result = new List<Site>();
            value.ForEach( a => result.Add( a.ToModel() ) );
            return result;
        }

        /// <summary>
        /// To the dto.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static List<SiteDto> ToDto( this List<Site> value )
        {
            List<SiteDto> result = new List<SiteDto>();
            value.ForEach( a => result.Add( a.ToDto() ) );
            return result;
        }

        /// <summary>
        /// To the dto.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static SiteDto ToDto( this Site value )
        {
            return new SiteDto( value );
        }

    }
}