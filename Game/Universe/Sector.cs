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
        if (NoiseSeed > 0 && NoiseSeed < .2f)
            return SectorType.SingleStar;
        
        if (NoiseSeed == 0)
            return SectorType.BlackHole;
        
        if (NoiseSeed< 0 && NoiseSeed>= -.2f)
            return SectorType.BinaryStar;

        if (NoiseSeed< .2f && NoiseSeed>= -.4f)
            return SectorType.TriStar;

        if (NoiseSeed< .4f && NoiseSeed>= -.6f)
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

