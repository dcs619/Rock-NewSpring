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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Represents an entity object and is the base class for all model objects to inherit from
    /// </summary>
    /// <typeparam name="T">The Type entity that is being referenced <example>Entity&lt;Person&gt;</example></typeparam>
    [DataContract]
    public abstract class Entity<T> : IEntity, DotLiquid.ILiquidizable
        where T : Entity<T>, new()
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the value of the identifier.  This value is the primary field/key for the entity object.  This value is system and database
        /// dependent, and is not guaranteed to be unique. This id should only be used to identify an object internally to a single implementation
        /// of Rock since this value has a very high probability of not being consistent in an external implementation of Rock.
        /// </summary>
        /// <value>
        /// Primary and system dependent <see cref="System.Int32" /> based identity/key of an entity object in Rock.
        /// </value>
        [Key]
        [DataMember]
        [IncludeForReporting]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="System.Guid"/> value that is a guaranteed unique identifier for the entity object.  This value 
        /// is an alternate key for the object, and should be used when interacting with external systems and when comparing and synchronizing
        /// objects across across data stores or external /implementations of Rock
        /// </summary>
        /// <remarks>
        /// A good place for a Guid to be used is when comparing or syncing data across two implementations of Rock. For example, if you 
        /// were creating a <see cref="Rock.Web.UI.RockBlock"/> with a data migration that adds/remove a new defined value object to the database. You would want to 
        /// search based on the Guid because it would be guaranteed to be unique across all implementations of Rock. 
        /// </remarks>
        /// <value>
        /// A <see cref="System.Guid"/> value that will uniquely identify the entity/object across all implementations of Rock.
        /// </value>
        [Index( IsUnique = true )]
        [DataMember]
        [IncludeForReporting]
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        private Guid _guid = Guid.NewGuid();

        /// <summary>
        /// Gets or sets an optional foreign identifier.  This can be used for importing or syncing data to a foreign system
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [HideFromReporting]
        public string ForeignId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the <see cref="Rock.Model.EntityType"/> Id for the Entity object type in Rock. If an <see cref="Rock.Model.EntityType"/> is not found
        /// for the object type it will be created
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the identifier for the current Entity object type. 
        /// </value>
        public virtual int TypeId
        {
            get
            {
                // Read should never return null since it will create entity type if it doesn't exist
                return Rock.Web.Cache.EntityTypeCache.Read( typeof( T ) ).Id;
            }
        }

        /// <summary>
        /// Gets the unique type name of the entity.  Typically this is the qualified name of the class
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        [NotMapped]
        public virtual string TypeName
        {
            get
            {
                return typeof( T ).FullName;
            }
        }

        /// <summary>
        /// Gets an URLEncoded encrypted string/key that represents the entity object
        /// </summary>
        /// <value>
        /// An encrypted <see cref="System.String"/> that represents the entity object.
        /// </value>
        [NotMapped]
        public virtual string ContextKey
        {
            get
            {
                string identifier =
                    TypeName + "|" +
                    this.Id.ToString() + ">" +
                    this.Guid.ToString();
                return System.Web.HttpUtility.UrlEncode( Rock.Security.Encryption.EncryptString( identifier ) );
            }
        }

        /// <summary>
        /// Gets the validation results for the entity
        /// </summary>
        [NotMapped]
        public virtual List<ValidationResult> ValidationResults
        {
            get { return _validationResults; }
        }
        private List<ValidationResult> _validationResults;

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> that is <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public virtual bool IsValid
        {
            get
            {
                var valContext = new ValidationContext( this, serviceProvider: null, items: null );
                _validationResults = new List<ValidationResult>();
                return Validator.TryValidateObject( this, valContext, _validationResults, true );
            }
        }

        /// <summary>
        /// Gets a publicly viewable unique key for the entity.
        /// NOTE: Will result in an empty string in a non-web app.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents a viewable version of the entity's unique key.
        /// </value>
        [NotMapped]
        public virtual string EncryptedKey
        {
            get
            {
                string identifier = this.Id.ToString() + ">" + this.Guid.ToString();
                string result = string.Empty;

                //// Non-web apps might not have the dataencryptionkey
                //// so just return empty string if we can't encrypt

                if ( Rock.Security.Encryption.TryEncryptString( identifier, out result ) )
                {
                    return result;
                }
                else
                {
                    return string.Empty; ;
                }
            }
        }

        /// <summary>
        /// Gets a URL friendly version of the EncryptedKey for the entity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents a URL friendly version of the entity's unique key.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual string UrlEncodedKey
        {
            get
            {
                string encodedKey = System.Web.HttpUtility.UrlEncode( EncryptedKey );
                return encodedKey.Replace( '%', '!' );
            }
            private set { }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets the entity object type's friendly name
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the entity object type's friendly name.
        /// </value>
        [NotMapped]
        public static string FriendlyTypeName
        {
            get
            {
                var type = typeof( T );
                return type.GetFriendlyTypeName();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a deep copy of this entity object, including all child objects.
        /// </summary>
        /// <returns>
        /// A complete copy of the entity object
        /// </returns>
        public virtual IEntity Clone()
        {
            var json = this.ToJson();
            return FromJson( json );
        }

        /// <summary>
        /// Creates a dictionary containing the majority of the entity object's properties. The only properties that are excluded
        /// are the Id, Guid and Order.  
        /// </summary>
        /// <returns>A <see cref="Dictionary{String, Object}"/> that represents the current entity object. Each <see cref="KeyValuePair{String, Object}"/> includes the property
        /// name as the key and the property value as the value.</returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();

            foreach ( var propInfo in this.GetType().GetProperties() )
            {
                if ( !propInfo.GetGetMethod().IsVirtual || propInfo.Name == "Id" || propInfo.Name == "Guid" || propInfo.Name == "Order" )
                {
                    dictionary.Add( propInfo.Name, propInfo.GetValue( this, null ) );
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Populates the current entity object with the contents of the current entity object.
        /// </summary>
        /// <param name="properties">A <see cref="System.Collections.Generic.Dictionary{String, Object}" /> that contains <see cref="System.Collections.Generic.KeyValuePair{String, Object}">KeyValuePairs</see>
        /// of representing properties.</param>
        public virtual void FromDictionary( Dictionary<string, object> properties )
        {
            Type type = this.GetType();

            foreach ( var property in properties )
            {
                var propInfo = type.GetProperty( property.Key );
                if ( propInfo != null )
                {
                    propInfo.SetValue( this, property.Value );
                }
            }
        }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object. 
        /// </summary>
        /// <returns>DotLiquid compatible dictionary.</returns>
        public object ToLiquid()
        {
            return this.ToLiquid( false );
        }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> the entire object tree will be parsed immediately.</param>
        /// <returns>
        /// DotLiquid compatible dictionary.
        /// </returns>
        public virtual object ToLiquid( bool debug )
        {
            var dictionary = new Dictionary<string, object>();

            Type entityType = this.GetType();
            if ( entityType.Namespace == "System.Data.Entity.DynamicProxies" )
                entityType = entityType.BaseType;

            foreach ( var propInfo in entityType.GetProperties() )
            {
                if ( propInfo.Name != "Attributes" &&
                    propInfo.Name != "AttributeValues" &&
                    propInfo.GetCustomAttributes( typeof( System.Runtime.Serialization.DataMemberAttribute ) ).Count() > 0 &&
                    propInfo.GetCustomAttributes( typeof( Rock.Data.IgnoreLiquidAttribute ) ).Count() <= 0 )
                {
                    object propValue = propInfo.GetValue( this, null );

                    if (propValue is Guid)
                    {
                        propValue = ( (Guid)propValue ).ToString();
                    }

                    if ( debug && propValue is IEntity )
                    {
                        dictionary.Add( propInfo.Name, ( (IEntity)propValue ).ToLiquid( true ) );
                    }
                    else if ( debug && propValue is DotLiquid.ILiquidizable )
                    {
                        dictionary.Add( propInfo.Name, ( (DotLiquid.ILiquidizable)propValue ).ToLiquid() );
                    }
                    else
                    {
                        dictionary.Add( propInfo.Name, propValue );
                    }
                }
            }

            return dictionary;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns an instance of the  entity based on a JSON represented of the entity object.
        /// </summary>
        /// <param name="json">A <see cref="System.String"/> containing a JSON formatted representation of the object.</param>
        /// <returns>An instance of the entity object based on the provided JSON string.</returns>
        public static T FromJson( string json )
        {
            return JsonConvert.DeserializeObject( json, typeof( T ) ) as T;
        }

        #endregion

    }

    #region KeyEntity

    /// <summary>
    /// Object used for current model (context) implementation 
    /// </summary>
    internal class KeyEntity
    {
        public string Key { get; set; }
        public int Id { get; set; }
        public IEntity Entity { get; set; }

        public KeyEntity( int id )
        {
            Id = id;
        }

        public KeyEntity( string key )
        {
            Key = key;
        }
    }

    #endregion

}