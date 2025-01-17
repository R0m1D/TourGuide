using GpsUtil.Location;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;

namespace TourGuide.Services;

public class RewardsService : IRewardsService
{
    private const double StatuteMilesPerNauticalMile = 1.15077945;
    private readonly int _defaultProximityBuffer = 10;
    private int _proximityBuffer;
    private readonly int _attractionProximityRange = 20000;
    private readonly IGpsUtil _gpsUtil;
    private readonly IRewardCentral _rewardsCentral;
    private static int count = 0;
    public Dictionary<Attraction, double> closestAttraction = new Dictionary<Attraction, double>();


    public RewardsService(IGpsUtil gpsUtil, IRewardCentral rewardCentral)
    {
        _gpsUtil = gpsUtil;
        _rewardsCentral =rewardCentral;
        _proximityBuffer = _defaultProximityBuffer;
    }

    public void SetProximityBuffer(int proximityBuffer)
    {
        _proximityBuffer = proximityBuffer;
    }

    public void SetDefaultProximityBuffer()
    {
        _proximityBuffer = _defaultProximityBuffer;
    }

    public async Task CalculateRewards(User user)
    {
        count++;
        List<VisitedLocation> userLocations = user.VisitedLocations;
        List<Attraction> attractions =  await _gpsUtil.GetAttractionsAsync();

        var tasks = new List<Task>(); // Liste pour stocker les tâches

        foreach (var visitedLocation in userLocations.ToList())
        {
            foreach (var attraction in attractions)
            {
                if (!user.UserRewards.Any(r => r.Attraction.AttractionName == attraction.AttractionName))
                {
                    if (NearAttraction(visitedLocation, attraction))
                    {
                        // Crée une tâche pour chaque itération et ajoute-la à la liste

                        Console.WriteLine("markpoint");
                        var rewardPoints = GetRewardPoints(attraction, user);
                        user.AddUserReward(new UserReward(visitedLocation, attraction, rewardPoints));
                    } ;
                    }
                }
            }
   

        // Attendre que toutes les tâches soient terminées
    }

    public bool IsWithinAttractionProximity(Attraction attraction, Locations location)
    {
        double attractionDistance = GetDistance(attraction, location);
        closestAttraction[attraction] = attractionDistance;
        return true;
    }


    private bool NearAttraction(VisitedLocation visitedLocation, Attraction attraction)
    {
        return GetDistance(attraction, visitedLocation.Location) <= _proximityBuffer;
    }

    private int GetRewardPoints(Attraction attraction, User user)
    {
        return _rewardsCentral.GetAttractionRewardPoints(attraction.AttractionId, user.UserId);
    }

    public double GetDistance(Locations loc1, Locations loc2)
    {
        double lat1 = Math.PI * loc1.Latitude / 180.0;
        double lon1 = Math.PI * loc1.Longitude / 180.0;
        double lat2 = Math.PI * loc2.Latitude / 180.0;
        double lon2 = Math.PI * loc2.Longitude / 180.0;

        double angle = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2)
                                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));

        double nauticalMiles = 60.0 * angle * 180.0 / Math.PI;
        return StatuteMilesPerNauticalMile * nauticalMiles;
    }
}
