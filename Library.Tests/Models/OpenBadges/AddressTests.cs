using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class AddressTests
{
    [Fact]
    public void Address_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" }
        };

        // Assert
        Assert.Contains("Address", address.Type);
    }

    [Fact]
    public void Address_FullAddress_CanBeSet()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" },
            StreetAddress = "123 Main Street",
            AddressLocality = "Springfield",
            AddressRegion = "IL",
            PostalCode = "62701",
            AddressCountry = "United States",
            AddressCountryCode = "US"
        };

        // Assert
        Assert.Equal("123 Main Street", address.StreetAddress);
        Assert.Equal("Springfield", address.AddressLocality);
        Assert.Equal("IL", address.AddressRegion);
        Assert.Equal("62701", address.PostalCode);
        Assert.Equal("United States", address.AddressCountry);
        Assert.Equal("US", address.AddressCountryCode);
    }

    [Fact]
    public void Address_PostOfficeBox_CanBeSet()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" },
            PostOfficeBoxNumber = "PO Box 123",
            AddressLocality = "Springfield",
            PostalCode = "62701"
        };

        // Assert
        Assert.Equal("PO Box 123", address.PostOfficeBoxNumber);
    }

    [Fact]
    public void Address_GeoCoordinates_CanBeSet()
    {
        // Arrange
        var geo = new GeoCoordinates
        {
            Latitude = 39.7817,
            Longitude = -89.6501,
            Type = "GeoCoordinates"
        };

        // Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" },
            StreetAddress = "123 Main Street",
            Geo = geo
        };

        // Assert
        Assert.NotNull(address.Geo);
        Assert.Equal(39.7817, address.Geo.Latitude);
        Assert.Equal(-89.6501, address.Geo.Longitude);
    }

    [Fact]
    public void Address_CountryCode_ISO31661()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" },
            AddressCountryCode = "US"
        };

        // Assert
        Assert.Equal("US", address.AddressCountryCode);
        Assert.Equal(2, address.AddressCountryCode.Length); // ISO 3166-1 alpha-2
    }

    [Fact]
    public void Address_AllOptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" }
        };

        // Assert
        Assert.Null(address.StreetAddress);
        Assert.Null(address.AddressLocality);
        Assert.Null(address.AddressRegion);
        Assert.Null(address.PostalCode);
        Assert.Null(address.AddressCountry);
        Assert.Null(address.AddressCountryCode);
        Assert.Null(address.PostOfficeBoxNumber);
        Assert.Null(address.Geo);
    }

    [Fact]
    public void Address_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address" }
        };
        address.AdditionalProperties["building"] = "Building A";

        // Assert
        Assert.Contains("building", address.AdditionalProperties.Keys);
        Assert.Equal("Building A", address.AdditionalProperties["building"]);
    }

    [Fact]
    public void Address_Type_SupportsMultipleValues()
    {
        // Arrange & Act
        var address = new Address
        {
            Type = new Collection<string> { "Address", "PostalAddress" }
        };

        // Assert
        Assert.Equal(2, address.Type.Count);
        Assert.Contains("Address", address.Type);
        Assert.Contains("PostalAddress", address.Type);
    }

    [Fact]
    public void Address_Serialization_Roundtrip()
    {
        // Arrange
        var address = new Address
        {
            Type = new Collection<string> { "Address" },
            StreetAddress = "123 Main Street",
            AddressLocality = "Springfield",
            AddressRegion = "IL",
            PostalCode = "62701",
            AddressCountry = "United States",
            AddressCountryCode = "US"
        };

        // Act
        var json = JsonSerializer.Serialize(address);
        var deserialized = JsonSerializer.Deserialize<Address>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(address.StreetAddress, deserialized.StreetAddress);
        Assert.Equal(address.AddressLocality, deserialized.AddressLocality);
        Assert.Equal(address.AddressRegion, deserialized.AddressRegion);
        Assert.Equal(address.PostalCode, deserialized.PostalCode);
        Assert.Equal(address.AddressCountry, deserialized.AddressCountry);
        Assert.Equal(address.AddressCountryCode, deserialized.AddressCountryCode);
    }

    [Fact]
    public void Address_Serialization_WithGeoCoordinates()
    {
        // Arrange
        var address = new Address
        {
            Type = new Collection<string> { "Address" },
            StreetAddress = "123 Main Street",
            Geo = new GeoCoordinates
            {
                Latitude = 39.7817,
                Longitude = -89.6501,
                Type = "GeoCoordinates"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(address);
        var deserialized = JsonSerializer.Deserialize<Address>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Geo);
        Assert.Equal(39.7817, deserialized.Geo.Latitude);
        Assert.Equal(-89.6501, deserialized.Geo.Longitude);
    }
}