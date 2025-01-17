using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;

namespace TourGuide.LibrairiesWrappers;

public class GpsUtilWrapper : IGpsUtil
{
    private readonly GpsUtil.GpsUtil _gpsUtil;

    public GpsUtilWrapper()
    {
        _gpsUtil = new();
    }

    public VisitedLocation GetUserLocation(Guid userId)
    {
        return _gpsUtil.GetUserLocation(userId);
    }
    public async Task<VisitedLocation> GetUserLocationAsync(Guid userId)
    {
        return await Task.Run(() => _gpsUtil.GetUserLocation(userId));
    }

    public async Task<List<Attraction>> GetAttractionsAsync()
    {
        return await Task.Run(() => _gpsUtil.GetAttractions());
    }
    public List<Attraction> GetAttractions()
    {
        return _gpsUtil.GetAttractions();
    }
}
