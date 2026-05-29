using Library.Models.OpenBadges;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class GeoCoordinatesTests
{
    [Fact]
    public void GeoCoordinates_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates"
        };

        // Assert
        Assert.Equal("GeoCoordinates", geo.Type);
    }

    [Fact]
    public void GeoCoordinates_Latitude_CanBeSet()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = 39.7817
        };

        // Assert
        Assert.Equal(39.7817, geo.Latitude);
    }

    [Fact]
    public void GeoCoordinates_Longitude_CanBeSet()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Longitude = -89.6501
        };

        // Assert
        Assert.Equal(-89.6501, geo.Longitude);
    }

    [Fact]
    public void GeoCoordinates_BothCoordinates_CanBeSet()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        // Assert - New York City coordinates
        Assert.Equal(40.7128, geo.Latitude);
        Assert.Equal(-74.0060, geo.Longitude);
    }

    [Fact]
    public void GeoCoordinates_NegativeCoordinates_Supported()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = -33.8688,  // Sydney, Australia
            Longitude = 151.2093
        };

        // Assert
        Assert.Equal(-33.8688, geo.Latitude);
        Assert.Equal(151.2093, geo.Longitude);
    }

    [Fact]
    public void GeoCoordinates_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = 39.7817,
            Longitude = -89.6501
        };
        geo.AdditionalProperties["elevation"] = 180;

        // Assert
        Assert.Contains("elevation", geo.AdditionalProperties.Keys);
    }

    [Fact]
    public void GeoCoordinates_Type_MustBeGeoCoordinates()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates"
        };

        // Assert
        Assert.Equal("GeoCoordinates", geo.Type);
    }

    [Fact]
    public void GeoCoordinates_Serialization_Roundtrip()
    {
        // Arrange
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = 39.7817,
            Longitude = -89.6501
        };

        // Act
        var json = JsonSerializer.Serialize(geo);
        var deserialized = JsonSerializer.Deserialize<GeoCoordinates>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(geo.Type, deserialized.Type);
        Assert.Equal(geo.Latitude, deserialized.Latitude);
        Assert.Equal(geo.Longitude, deserialized.Longitude);
    }

    [Fact]
    public void GeoCoordinates_OptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var geo = new GeoCoordinates
        {
            Type = "GeoCoordinates"
        };

        // Assert
        Assert.Null(geo.Latitude);
        Assert.Null(geo.Longitude);
    }

    [Fact]
    public void GeoCoordinates_WGS84_Format()
    {
        // WGS84 is the standard for GPS coordinates
        // Latitude range: -90 to +90
        // Longitude range: -180 to +180

        // Arrange & Act
        var northPole = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = 90.0,
            Longitude = 0.0
        };

        var southPole = new GeoCoordinates
        {
            Type = "GeoCoordinates",
            Latitude = -90.0,
            Longitude = 0.0
        };

        // Assert
        Assert.InRange(northPole.Latitude.Value, -90, 90);
        Assert.InRange(northPole.Longitude.Value, -180, 180);
        Assert.InRange(southPole.Latitude.Value, -90, 90);
        Assert.InRange(southPole.Longitude.Value, -180, 180);
    }
}