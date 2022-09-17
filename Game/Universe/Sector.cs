using Godot;
using System.Collections.Generic;


public class Sector
{
    public Vector2 Position { get; set; }
    public float NoiseSeed { get; set; }
    
    public List<Vector2> Seeds { get; set; }
    public Planet Planet { get; set; }
    public List<Moon> Moons { get; }
    public List<TravelLane> TravelLines { get; }
    public List<Asteroid> Asteroids { get; }

    public Sector()
    {
        Seeds = new List<Vector2>();
        Planet = new Planet();
        Moons = new List<Moon>();
        TravelLines = new List<TravelLane>();
        Asteroids = new List<Asteroid>();
    }

    public SectorType GetSectorType()
    {
        return GetSectorType(NoiseSeed);
    }

    public static SectorType GetSectorType(float noiseSeed)
    {
        if (noiseSeed > 0 && noiseSeed < .2f)
            return SectorType.SingleStar;
        
        if (noiseSeed == 0)
            return SectorType.BlackHole;
        
        if (noiseSeed< 0 && noiseSeed>= -.2f)
            return SectorType.BinaryStar;

        if (noiseSeed< .2f && noiseSeed>= -.4f)
            return SectorType.TriStar;

        if (noiseSeed< .4f && noiseSeed>= -.6f)
            return SectorType.Cloud;
        
        return SectorType.Void;
    }

    public enum SectorType
    {
        SingleStar,
        BinaryStar,
        TriStar,
        BlackHole,
        Cloud,
        Void
    }
}

