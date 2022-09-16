using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Universe : Node
{
    int _universeSpawnAreaWidth = 250;
    int _universeSpawnAreaHeight = 250;
    ulong _seed = 1;
    int _noiseSeed = 1;
    RandomNumberGenerator _rng;
    OpenSimplexNoise _noise;

    public MapCell _currentMapCell;
    List<MapCell> _mapCellGrid;

    public List<MapCell> MapCellGrid => _mapCellGrid;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _rng = new RandomNumberGenerator();
        _rng.Seed = _seed;
        _noise = new OpenSimplexNoise();
        _noise.Seed = _noiseSeed;
    }

    public void NewGame()
    {
        _mapCellGrid = new List<MapCell>();

        var currentPositionInUniverse = NewPlayerPosition();

        _currentMapCell = GetCurrentSector(currentPositionInUniverse);
        _mapCellGrid.Add(_currentMapCell);

        while(_mapCellGrid.Count < 33)
        {
            var count = _mapCellGrid.Count;
            for (int i = 0; i < count; i++)
            {
                MapCell mcg = _mapCellGrid[i];
                var currentMapCellNeighbors = GetSoroundingSectors(mcg.OffsetCoordFromHex.ToVector2());
                foreach(var cmcn in currentMapCellNeighbors)
                {
                    if(!_mapCellGrid.Any(x => x.Equals(cmcn)))
                    {
                        GD.Print($"Added {cmcn.HexPosition.ToVector3()}");
                        _mapCellGrid.Add(cmcn);
                    }
                    else
                    {
                        GD.Print("Skipped");
                        continue;
                    }
                }
            }
        }
    }

    public Vector2 NewPlayerPosition()
    {
        //return new Vector2(_rng.RandiRange(-_universeSpawnAreaWidth, _universeSpawnAreaWidth), _rng.RandiRange(-_universeSpawnAreaHeight, _universeSpawnAreaHeight));
        return new Vector2(0, 0);
    }

    public MapCell GetCurrentSector(Vector2 currentPosition)
    {
        var currentMapCell = new Hex((int)currentPosition.x, (int)currentPosition.y);
        return new MapCell() {
                HexPosition = currentMapCell,
                NoiseSeed = _noise.GetNoise3dv(currentMapCell.ToVector3())
            };
    }

    public MapCell[] GetSoroundingSectors(Vector2 currentPosition)
    {
        var mapCells = new List<MapCell>();
        var currentMapCell = new MapCell() {
                HexPosition = new Hex((int)currentPosition.x, (int)currentPosition.y)
            };

        var neighbors = currentMapCell.HexPosition.Neighbors();
        foreach(var sor in neighbors)
        {
            var noise = _noise.GetNoise3dv(sor.ToVector3());

            mapCells.Add(new MapCell() {
                HexPosition = sor,
                NoiseSeed = noise
            });
        }

        return mapCells.ToArray();
    }

    public MapCell GetCurrentMapCell()
    {
        return _currentMapCell;
    }

    public MapCell[] GetMap()
    {
        return _mapCellGrid.ToArray();
    }
}


/// <summary>
/// Cube storage
/// </summary>
public class Hex
{
    public int Q { get; }
    public int R { get; }
    public int S { get;}

    public Hex(int q, int r, int s)
    {
        Q = q;
        R = r;
        S = s;
    }

    public Hex(int q, int r)
    {
        Q = q;
        R = r;
        S = -Q - R;
    }

#region Arithmetic
    public Hex Add(Hex b)
    {
        return new Hex(Q + b.Q, R + b.R, S + b.S);
    }

    public Hex Subtract(Hex b)
    {
        return new Hex(Q - b.Q, R - b.R, S - b.S);
    }

    public Hex Multipy(Hex b)
    {
        return new Hex(Q * b.Q, R * b.R, S * b.S);
    }

    public static Hex Add(Hex a, Hex b)
    {
        return new Hex(a.Q + b.Q, a.R + b.R, a.S + b.S);
    }

    public static Hex Subtract(Hex a, Hex b)
    {
        return new Hex(a.Q - b.Q, a.R - b.R, a.S - b.S);
    }

    public static Hex Multipy(Hex a, Hex b)
    {
        return new Hex(a.Q * b.Q, a.R * b.R, a.S * b.S);
    }
#endregion
    
#region Distance
    public int Length(Hex hex)
    {
        return (int)((Mathf.Abs(hex.Q) + Mathf.Abs(hex.R) + Mathf.Abs(hex.S)) / 2);
    }

    public int Distance(Hex a, Hex b)
    {
        return Length(a.Subtract(b));
    }

    public int Distance(Hex b)
    {
        return Length(this.Subtract(b));
    }
#endregion
    
#region Neighbors
    public static List<Hex> directions = new List<Hex>{new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)};

    public Hex Direction(int direction)
    {
        if(!(0 <= direction && direction < 6))
            throw new Exception("Direction must be 0 to 5");

        return directions[direction];
    }

    public Hex Neighbor(Hex hex, int direction)
    {
        return hex.Add(Direction(direction));
    }

    public List<Hex> Neighbors()
    {
        return Neighbors(this);
    }

    public List<Hex> Neighbors(Hex hex)
    {
        var result = new List<Hex>();

        for (int i = 0; i < directions.Count; i++)
        {
            result.Add(hex.Add(Direction(i)));
        }

        return result;
    }
#endregion
    
    public Hex RotateLeft()
    {
        return new Hex(-S, -Q, -R);
    }


    public Hex RotateRight()
    {
        return new Hex(-R, -S, -Q);
    }

    internal Vector3 ToVector3()
    {
        return new Vector3(R, S, Q);
    }
}

public class Layout
{
    public readonly HexOrientation orientation;
    public readonly Point size;
    public readonly Point origin;

    public Layout(HexOrientation orientation, Point size, Point origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }

    public Point HexToPixel(Hex h)
    {
        HexOrientation M = orientation;
        double x = (M.f0 * h.Q + M.f1 * h.R) * size.x;
        double y = (M.f2 * h.Q + M.f3 * h.R) * size.y;
        return new Point(x + origin.x, y + origin.y);
    }

    public FractionalHex PixelToHex(Point p)
    {
        HexOrientation M = orientation;
        Point pt = new Point((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        double q = M.b0 * pt.x + M.b1 * pt.y;
        double r = M.b2 * pt.x + M.b3 * pt.y;
        return new FractionalHex(q, r, -q - r);
    }

    public Point HexCornerOffset(int corner)
    {
        HexOrientation M = orientation;
        double angle = 2.0 * Math.PI * (M._startAngle - corner) / 6.0;
        return new Point(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
    }

    public List<Point> PolygonCorners(Hex h)
    {
        List<Point> corners = new List<Point>{};
        Point center = HexToPixel(h);
        for (int i = 0; i < 6; i++)
        {
            Point offset = HexCornerOffset(i);
            corners.Add(new Point(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }

    static public HexOrientation pointy = new HexOrientation(Math.Sqrt(3.0), Math.Sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.Sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);
    static public HexOrientation flat = new HexOrientation(3.0 / 2.0, 0.0, Math.Sqrt(3.0) / 2.0, Math.Sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.Sqrt(3.0) / 3.0, 0.0);
}

public class Point
{
    public Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
    public readonly double x;
    public readonly double y;

    public Vector2 ToVector2()
    {
        return new Vector2((float)x, (float)y);
    }

    internal static Point FromVector2(Vector2 v)
    {
        return new Point(v.x, v.y);
    }
}

public class HexOrientation
{
    public readonly double f0, f1, f2, f3, b0, b1, b2, b3, _startAngle;

    public HexOrientation(double f0, double f1, double f2, double f3, double b0, double b1, double b2, double b3, double startAngle)
    {
        this.f0 = f0;
        this.f1 = f1;
        this.f2 = f2;
        this.f3 = f3;
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
        this.b3 = b3;
        _startAngle = startAngle;
    }
}

public class FractionalHex
{
    public FractionalHex(double q, double r, double s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public readonly double q;
    public readonly double r;
    public readonly double s;

    public Hex HexRound()
    {
        int qi = (int)(Math.Round(q));
        int ri = (int)(Math.Round(r));
        int si = (int)(Math.Round(s));
        double q_diff = Math.Abs(qi - q);
        double r_diff = Math.Abs(ri - r);
        double s_diff = Math.Abs(si - s);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            qi = -ri - si;
        }
        else
            if (r_diff > s_diff)
            {
                ri = -qi - si;
            }
            else
            {
                si = -qi - ri;
            }
        return new Hex(qi, ri, si);
    }

    public FractionalHex HexLerp(FractionalHex b, double t)
    {
        return new FractionalHex(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
    }

    static public List<Hex> HexLinedraw(Hex a, Hex b)
    {
        int N = a.Distance(b);
        FractionalHex a_nudge = new FractionalHex(a.Q + 1e-06, a.R + 1e-06, a.S - 2e-06);
        FractionalHex b_nudge = new FractionalHex(b.Q + 1e-06, b.R + 1e-06, b.S - 2e-06);
        List<Hex> results = new List<Hex>{};
        double step = 1.0 / Math.Max(N, 1);
        for (int i = 0; i <= N; i++)
        {
            results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
        }
        return results;
    }
}

public class OffsetCoord
{
    public OffsetCoord(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
    public readonly int col;
    public readonly int row;
    public const int EVEN = 1;
    public const int ODD = -1;

    static public OffsetCoord QoffsetFromCube(int offset, Hex h)
    {
        int col = h.Q;
        int row = h.R + (int)((h.Q + offset * (h.Q & 1)) / 2);
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new OffsetCoord(col, row);
    }


    static public Hex QoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col;
        int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
        int s = -q - r;
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new Hex(q, r, s);
    }


    static public OffsetCoord RoffsetFromCube(int offset, Hex h)
    {
        int col = h.Q + (int)((h.R + offset * (h.R & 1)) / 2);
        int row = h.R;
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new OffsetCoord(col, row);
    }


    static public Hex RoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col - (int)((h.row + offset * (h.row & 1)) / 2);
        int r = h.row;
        int s = -q - r;
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new Hex(q, r, s);
    }

    public Vector2 ToVector2()
    {
        return new Vector2(col, row);
    }
}