// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.DataFactory.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// The settings that will be leveraged for oracle source partitioning.
    /// </summary>
    public partial class OraclePartitionSettings
    {
        /// <summary>
        /// Initializes a new instance of the OraclePartitionSettings class.
        /// </summary>
        public OraclePartitionSettings()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OraclePartitionSettings class.
        /// </summary>
        /// <param name="partitionNames">Names of the physical partitions of
        /// oracle table. </param>
        /// <param name="partitionColumnName">The name of the column in integer
        /// type that will be used for proceeding range partitioning. Type:
        /// string (or Expression with resultType string).</param>
        /// <param name="partitionUpperBound">The maximum value of column
        /// specified in partitionColumnName that will be used for proceeding
        /// range partitioning. Type: string (or Expression with resultType
        /// string).</param>
        /// <param name="partitionLowerBound">The minimum value of column
        /// specified in partitionColumnName that will be used for proceeding
        /// range partitioning. Type: string (or Expression with resultType
        /// string).</param>
        public OraclePartitionSettings(object partitionNames = default(object), object partitionColumnName = default(object), object partitionUpperBound = default(object), object partitionLowerBound = default(object))
        {
            PartitionNames = partitionNames;
            PartitionColumnName = partitionColumnName;
            PartitionUpperBound = partitionUpperBound;
            PartitionLowerBound = partitionLowerBound;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets names of the physical partitions of oracle table.
        /// </summary>
        [JsonProperty(PropertyName = "partitionNames")]
        public object PartitionNames { get; set; }

        /// <summary>
        /// Gets or sets the name of the column in integer type that will be
        /// used for proceeding range partitioning. Type: string (or Expression
        /// with resultType string).
        /// </summary>
        [JsonProperty(PropertyName = "partitionColumnName")]
        public object PartitionColumnName { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of column specified in
        /// partitionColumnName that will be used for proceeding range
        /// partitioning. Type: string (or Expression with resultType string).
        /// </summary>
        [JsonProperty(PropertyName = "partitionUpperBound")]
        public object PartitionUpperBound { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of column specified in
        /// partitionColumnName that will be used for proceeding range
        /// partitioning. Type: string (or Expression with resultType string).
        /// </summary>
        [JsonProperty(PropertyName = "partitionLowerBound")]
        public object PartitionLowerBound { get; set; }

    }
}
