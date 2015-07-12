//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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
using System.Collections.Generic;


namespace Rock.Client
{
    /// <summary>
    /// Base client model for WorkflowActivity that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class WorkflowActivityEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public int? ActivatedByActivityId { get; set; }

        /// <summary />
        public DateTime? ActivatedDateTime { get; set; }

        /// <summary />
        public int ActivityTypeId { get; set; }

        /// <summary />
        public int? AssignedGroupId { get; set; }

        /// <summary />
        public int? AssignedPersonAliasId { get; set; }

        /// <summary />
        public DateTime? CompletedDateTime { get; set; }

        /// <summary />
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary />
        public int WorkflowId { get; set; }

        /// <summary />
        public Guid Guid { get; set; }

        /// <summary />
        public string ForeignId { get; set; }

        /// <summary>
        /// Copies the base properties from a source WorkflowActivity object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( WorkflowActivity source )
        {
            this.Id = source.Id;
            this.ActivatedByActivityId = source.ActivatedByActivityId;
            this.ActivatedDateTime = source.ActivatedDateTime;
            this.ActivityTypeId = source.ActivityTypeId;
            this.AssignedGroupId = source.AssignedGroupId;
            this.AssignedPersonAliasId = source.AssignedPersonAliasId;
            this.CompletedDateTime = source.CompletedDateTime;
            this.LastProcessedDateTime = source.LastProcessedDateTime;
            this.WorkflowId = source.WorkflowId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for WorkflowActivity that includes all the fields that are available for GETs. Use this for GETs (use WorkflowActivityEntity for POST/PUTs)
    /// </summary>
    public partial class WorkflowActivity : WorkflowActivityEntity
    {
        /// <summary />
        public ICollection<WorkflowAction> Actions { get; set; }

        /// <summary />
        public DateTime? CreatedDateTime { get; set; }

        /// <summary />
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary />
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary />
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary>
        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }

        /// <summary>
        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }
    }
}
