﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core.Testing;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Common;
using Azure.Storage.Test;
using Azure.Storage.Test.Shared;
using NUnit.Framework;

namespace Azure.Storage.Blobs.Test
{
    [TestFixture]
    public class ServiceClientTests : BlobTestBase
    {
        public ServiceClientTests()
            : base(/* Use RecordedTestMode.Record here to re-record just these tests */)
        {
        }

        [Test]
        public void Ctor_ConnectionString()
        {
            var accountName = "accountName";
            var accountKey = Convert.ToBase64String(new byte[] { 0, 1, 2, 3, 4, 5 });

            var credentials = new SharedKeyCredentials(accountName, accountKey);
            var blobEndpoint = new Uri("http://127.0.0.1/" + accountName);
            var blobSecondaryEndpoint = new Uri("http://127.0.0.1/" + accountName + "-secondary");

            var connectionString = new StorageConnectionString(credentials, (blobEndpoint, blobSecondaryEndpoint), (default, default), (default, default), (default, default));

            var service = new BlobServiceClient(connectionString.ToString(true));

            var builder = new BlobUriBuilder(service.Uri);

            Assert.AreEqual("", builder.ContainerName);
            Assert.AreEqual("", builder.BlobName);
            Assert.AreEqual("accountName", builder.AccountName);
        }

        [Test]
        public async Task ListContainersSegmentAsync()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();
            // Ensure at least one container
            using (this.GetNewContainer(out _, service: service))
            {
                // Act
                var response = await service.ListContainersSegmentAsync();

                // Assert
                Assert.IsTrue(response.Value.ContainerItems.Count() >= 1);
            }
        }

        [Test]
        public async Task ListContainersSegmentAsync_Marker()
        {
            var service = this.GetServiceClient_SharedKey();
            // Ensure at least one container
            using (this.GetNewContainer(out var container, service: service))
            {
                var marker = default(string);
                ContainersSegment containersSegment;

                var containers = new List<ContainerItem>();

                do
                {
                    containersSegment = await service.ListContainersSegmentAsync(marker: marker);

                    containers.AddRange(containersSegment.ContainerItems);

                    marker = containersSegment.NextMarker;
                }
                while (!String.IsNullOrWhiteSpace(marker));

                Assert.AreNotEqual(0, containers.Count);
                Assert.AreEqual(containers.Count, containers.Select(c => c.Name).Distinct().Count());
                Assert.IsTrue(containers.Any(c => container.Uri == this.InstrumentClient(service.GetBlobContainerClient(c.Name)).Uri));
            }
        }

        [Test]
        public async Task ListContainersSegmentAsync_MaxResults()
        {
            var service = this.GetServiceClient_SharedKey();
            // Ensure at least one container
            using (this.GetNewContainer(out _, service: service))
            using (this.GetNewContainer(out var container, service: service))
            {
                // Act
                var response = await service.ListContainersSegmentAsync(options: new ContainersSegmentOptions
                {
                    MaxResults = 1
                });

                // Assert
                Assert.AreEqual(1, response.Value.ContainerItems.Count());
            }
        }

        [Test]
        public async Task ListContainersSegmentAsync_Prefix()
        {
            var service = this.GetServiceClient_SharedKey();
            var prefix = "aaa";
            var containerName = prefix + this.GetNewContainerName();
            // Ensure at least one container
            using (this.GetNewContainer(out var container, service: service, containerName: containerName))
            {
                // Act
                var response = await service.ListContainersSegmentAsync(options: new ContainersSegmentOptions
                {
                    Prefix = prefix
                });

                // Assert
                Assert.AreNotEqual(0, response.Value.ContainerItems.Count());
                Assert.IsTrue(response.Value.ContainerItems.All(c => c.Name.StartsWith(prefix)));
                Assert.IsNotNull(response.Value.ContainerItems.Single(c => c.Name == containerName));
            }
        }
        
        [Test]
        public async Task ListContainersSegmentAsync_Metadata()
        {
            var service = this.GetServiceClient_SharedKey();
            // Ensure at least one container
            using (this.GetNewContainer(out var container, service: service))
            {
                // Arrange
                var metadata = this.BuildMetadata();
                await container.SetMetadataAsync(metadata);

                // Act
                var response = await service.ListContainersSegmentAsync(options: new ContainersSegmentOptions
                {
                    Details = new ContainerListingDetails { Metadata = true }
                });

                // Assert
                Assert.IsNotNull(response.Value.ContainerItems.First().Metadata);
            }
        }

        [Test]
        public async Task ListContainersSegmentAsync_Error()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();

            // Act
            await TestHelper.AssertExpectedExceptionAsync<StorageRequestFailedException>(
                service.ListContainersSegmentAsync(marker: "garbage"),
                e => Assert.AreEqual("OutOfRangeInput", e.ErrorCode));
        }

        [Test]
        public async Task GetAccountInfoAsync()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();

            // Act
            var response = await service.GetAccountInfoAsync();

            // Assert
            Assert.IsNotNull(response.GetRawResponse().Headers.RequestId);
        }

        [Test]
        public async Task GetAccountInfoAsync_Error()
        {
            // Arrange
            var service = this.InstrumentClient(
                new BlobServiceClient(
                    this.GetServiceClient_SharedKey().Uri,
                    this.GetOptions()));

            // Act
            await TestHelper.AssertExpectedExceptionAsync<StorageRequestFailedException>(
                service.GetAccountInfoAsync(),
                e => Assert.AreEqual("ResourceNotFound", e.ErrorCode));
        }

        [Test]
        public async Task GetPropertiesAsync()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();

            // Act
            var response = await service.GetPropertiesAsync();

            // Assert
            Assert.IsFalse(String.IsNullOrWhiteSpace(response.Value.DefaultServiceVersion));
        }

        [Test]
        public async Task GetPropertiesAsync_Error()
        {
            // Arrange
            var service = this.InstrumentClient(
                new BlobServiceClient(
                    this.GetServiceClient_SharedKey().Uri,
                    this.GetOptions()));

            // Act
            await TestHelper.AssertExpectedExceptionAsync<StorageRequestFailedException>(
                service.GetPropertiesAsync(),
                e => Assert.AreEqual("ResourceNotFound", e.ErrorCode));
        }

        [Test]
        [NonParallelizable]
        public async Task SetPropertiesAsync()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();
            var properties = (await service.GetPropertiesAsync()).Value;
            var originalCors = properties.Cors.ToArray();
            properties.Cors =
                new[]
                {
                    new CorsRule
                    {
                        MaxAgeInSeconds = 1000,
                        AllowedHeaders = "x-ms-meta-data*,x-ms-meta-target*,x-ms-meta-abc",
                        AllowedMethods = "PUT,GET",
                        AllowedOrigins = "*",
                        ExposedHeaders = "x-ms-meta-*"
                    }
                };

            // Act
            await service.SetPropertiesAsync(properties);

            // Assert
            properties = (await service.GetPropertiesAsync()).Value;
            Assert.AreEqual(1, properties.Cors.Count());
            Assert.IsTrue(properties.Cors[0].MaxAgeInSeconds == 1000);

            // Cleanup
            properties.Cors = originalCors;
            await service.SetPropertiesAsync(properties);
            properties = await service.GetPropertiesAsync();
            Assert.AreEqual(originalCors.Count(), properties.Cors.Count());
        }

        [Test]
        public async Task SetPropertiesAsync_Error()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();
            var properties = (await service.GetPropertiesAsync()).Value;
            var invalidService = this.InstrumentClient(
                new BlobServiceClient(
                    this.GetServiceClient_SharedKey().Uri,
                    this.GetOptions()));

            // Act
            await TestHelper.AssertExpectedExceptionAsync<StorageRequestFailedException>(
                invalidService.SetPropertiesAsync(properties),
                e => Assert.AreEqual("ResourceNotFound", e.ErrorCode));
        }

        // Note: read-access geo-redundant replication must be enabled for test account, or this test will fail.
        [Test]
        public async Task GetStatisticsAsync()
        {
            // Arrange
            // "-secondary" is required by the server
            var service = this.InstrumentClient(
                new BlobServiceClient(
                    new Uri(TestConfigurations.DefaultTargetTenant.BlobServiceSecondaryEndpoint),
                    this.GetOptions(this.GetNewSharedKeyCredentials())));

            // Act
            var response = await service.GetStatisticsAsync();

            // Assert
            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GetUserDelegationKey()
        {
            // Arrange
            var service = await this.GetServiceClient_OauthAccount();

            // Act
            var response = await service.GetUserDelegationKeyAsync(start: null, expiry: this.Recording.UtcNow.AddHours(1));

            // Assert
            Assert.IsNotNull(response.Value);
        }

        [Test]
        public async Task GetUserDelegationKey_Error()
        {
            // Arrange
            var service = this.GetServiceClient_SharedKey();

            // Act
            await TestHelper.AssertExpectedExceptionAsync<StorageRequestFailedException>(
                service.GetUserDelegationKeyAsync(start: null, expiry: this.Recording.UtcNow.AddHours(1)),
                e => Assert.AreEqual("AuthenticationFailed", e.ErrorCode.Split('\n')[0]));
        }

        [Test]
        public async Task GetUserDelegationKey_ArgumentException()
        {
            // Arrange
            var service = await this.GetServiceClient_OauthAccount();

            // Act
            await TestHelper.AssertExpectedExceptionAsync<ArgumentException>(
                service.GetUserDelegationKeyAsync(start: null, expiry: this.Recording.Now.AddHours(1)),
                e => Assert.AreEqual("expiry must be UTC", e.Message));
        }
    }
}
