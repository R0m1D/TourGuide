using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.Users;
using TourGuide.Utilities;

namespace TourGuideTest;

public class RewardServiceTest : IClassFixture<DependencyFixture>
{
    private readonly DependencyFixture _fixture;

    public RewardServiceTest(DependencyFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UserGetRewards()
    {
        _fixture.Initialize(0);
        var user = new User(Guid.NewGuid(), "jon", "000", "jon@tourGuide.com");
        var attraction = _fixture.GpsUtil.GetAttractions().First();
        user.AddToVisitedLocations(new VisitedLocation(user.UserId, attraction, DateTime.Now));

        _fixture.TourGuideService.TrackUserLocation(user);

        // Calculez les récompenses de manière asynchrone
        await _fixture.RewardsService.CalculateRewards(user);

        // Obtenez les récompenses de l'utilisateur
        var userRewards = user.UserRewards;

        // Arrêtez le suivi
        _fixture.TourGuideService.Tracker.StopTracking();

        // Vérifiez le nombre de récompenses
        Assert.True(userRewards.Count == 1);
    }


    [Fact]
    public void IsWithinAttractionProximity()
    {
        var attraction = _fixture.GpsUtil.GetAttractions().First();
        Assert.True(_fixture.RewardsService.IsWithinAttractionProximity(attraction, attraction));
    }

    [Fact]
    public async Task NearAllAttractions()
    {
        var stopwatch = new Stopwatch();

        // Initialisation
        stopwatch.Start();
        _fixture.Initialize(1);
        stopwatch.Stop();
        Console.WriteLine($"Initialize time: {stopwatch.ElapsedMilliseconds} ms");

        // Configuration du buffer de proximité
        stopwatch.Restart();
        _fixture.RewardsService.SetProximityBuffer(int.MaxValue);
        stopwatch.Stop();
        Console.WriteLine($"SetProximityBuffer time: {stopwatch.ElapsedMilliseconds} ms");

        // Récupération de l'utilisateur
        stopwatch.Restart();
        var user = _fixture.TourGuideService.GetAllUsers().First();
        stopwatch.Stop();
        Console.WriteLine($"GetAllUsers time: {stopwatch.ElapsedMilliseconds} ms");

        // Calcul des récompenses
        stopwatch.Restart();
        await _fixture.RewardsService.CalculateRewards(user);
        stopwatch.Stop();
        Console.WriteLine($"CalculateRewards time: {stopwatch.ElapsedMilliseconds} ms");

        // Récupération des récompenses de l'utilisateur
        stopwatch.Restart();
        var userRewards = _fixture.TourGuideService.GetUserRewards(user);
        stopwatch.Stop();
        Console.WriteLine($"GetUserRewards time: {stopwatch.ElapsedMilliseconds} ms");

        // Arrêt du suivi
        stopwatch.Restart();
        _fixture.TourGuideService.Tracker.StopTracking();
        stopwatch.Stop();
        Console.WriteLine($"StopTracking time: {stopwatch.ElapsedMilliseconds} ms");

        // Vérification de l'assertion
        stopwatch.Restart();
        Assert.Equal(_fixture.GpsUtil.GetAttractions().Count, userRewards.Count);
        stopwatch.Stop();
        Console.WriteLine($"Assert.Equal time: {stopwatch.ElapsedMilliseconds} ms");
    }
}
