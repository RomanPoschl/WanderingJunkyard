using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ShipMeshFactory
{
    public static ShipMesh CreateCube(float size = 1)
    {
        var mesh = new ShipMesh();

        // create 8 vertices the notation is XYZ so for vertex that's on top face on the left top it will be LTT
        var LTT = new ShipMeshVertex(new Vector3(-size, +size, -size));
        var RTT = new ShipMeshVertex(new Vector3(+size, +size, -size));
        var RTB = new ShipMeshVertex(new Vector3(+size, +size, +size));
        var LTB = new ShipMeshVertex(new Vector3(-size, +size, +size));

        var LBT = new ShipMeshVertex(new Vector3(-size, -size, -size));
        var RBT = new ShipMeshVertex(new Vector3(+size, -size, -size));
        var RBB = new ShipMeshVertex(new Vector3(+size, -size, +size));
        var LBB = new ShipMeshVertex(new Vector3(-size, -size, +size));

        // create 6 faces

        // top face
        mesh.Faces.Add(new ShipMeshSquareFace(RTT, RTB, LTB, LTT));

        // left face
        mesh.Faces.Add(new ShipMeshSquareFace(LTB, LBB, LBT, LTT));

        // right face
        mesh.Faces.Add(new ShipMeshSquareFace(RTT, RBT, RBB, RTB));

        // front face
        mesh.Faces.Add(new ShipMeshSquareFace(RTB, RBB, LBB, LTB));

        // back face
        mesh.Faces.Add(new ShipMeshSquareFace(LTT, LBT, RBT, RTT));

        // bottom face
        mesh.Faces.Add(new ShipMeshSquareFace(LBT, LBB, RBB, RBT));

        return mesh;
    }

    public static ShipMesh CreateCylinder(int numberOfSegments, float cylinderSize1, float cylinderSize2, float cylinderDepth)
    {
        var mesh = new ShipMesh();
        var lowerCircle = new List<ShipMeshVertex>();
        var upperCircle = new List<ShipMeshVertex>();

        for (var i = 0; i < numberOfSegments; i++)
        {
            lowerCircle.Add(new ShipMeshVertex(new Vector3(
                Mathf.Cos((float)i / numberOfSegments * Mathf.Pi * 2) * cylinderSize1 / 2,
                Mathf.Sin((float)i / numberOfSegments * Mathf.Pi * 2) * cylinderSize1 / 2,
                -cylinderDepth / 2
                )));

            upperCircle.Add(new ShipMeshVertex(new Vector3(
                Mathf.Cos((float)i / numberOfSegments * Mathf.Pi * 2) * cylinderSize2 / 2,
                Mathf.Sin((float)i / numberOfSegments * Mathf.Pi * 2) * cylinderSize2 / 2,
                cylinderDepth / 2)));
        }

        for (var i = 0; i < numberOfSegments; i++)
        {
            mesh.Faces.Add(new ShipMeshSquareFace(
                upperCircle[i],
                upperCircle[(i + 1) % numberOfSegments],
                lowerCircle[(i + 1) % numberOfSegments],
                lowerCircle[i]
                ));
        }

        mesh.Faces.Add(new ShipMeshComplexFace(lowerCircle.ToArray(),
            BowyerWatson.GetTris(lowerCircle.Select(e => new Vector2(e.Coordinates.x, e.Coordinates.y) * 100))));

        mesh.Faces.Add(new ShipMeshComplexFace(upperCircle.ToArray(),
            BowyerWatson.GetTris(lowerCircle.Select(e => new Vector2(e.Coordinates.x, e.Coordinates.y) * 100), true)));

        return mesh;
    }


    // Inspired by http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
    public static ShipMesh CreateIcosphere(int numberOfSubdivisions, float size)
    {
        var t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        var verticies = new Vector3[]
        {
            new Vector3(-1, t, 0),
            new Vector3(1, t, 0),
            new Vector3(-1, -t, 0),
            new Vector3(1, -t, 0),

            new Vector3(0, -1, t),
            new Vector3(0, 1, t),
            new Vector3(0, -1, -t),
            new Vector3(0, 1, -t),

            new Vector3(t, 0, -1),
            new Vector3(t, 0, 1),
            new Vector3(-t, 0, -1),
            new Vector3(-t, 0, 1)
        }
        .Select(p => {
            var length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
            return new ShipMeshVertex(new Vector3(p.x / length, p.y / length, p.z / length));
        })
        .ToArray();

        var faces = new List<ShipMeshFace>()
        {
            new ShipMeshTriangleFace(verticies[0], verticies[11], verticies[5]),
            new ShipMeshTriangleFace(verticies[0], verticies[5], verticies[1]),
            new ShipMeshTriangleFace(verticies[0], verticies[1], verticies[7]),
            new ShipMeshTriangleFace(verticies[0], verticies[7], verticies[10]),
            new ShipMeshTriangleFace(verticies[0], verticies[10], verticies[11]),

            new ShipMeshTriangleFace(verticies[1], verticies[5], verticies[9]),
            new ShipMeshTriangleFace(verticies[5], verticies[11], verticies[4]),
            new ShipMeshTriangleFace(verticies[11], verticies[10], verticies[2]),
            new ShipMeshTriangleFace(verticies[10], verticies[7], verticies[6]),
            new ShipMeshTriangleFace(verticies[7], verticies[1], verticies[8]),

            new ShipMeshTriangleFace(verticies[3], verticies[9], verticies[4]),
            new ShipMeshTriangleFace(verticies[3], verticies[4], verticies[2]),
            new ShipMeshTriangleFace(verticies[3], verticies[2], verticies[6]),
            new ShipMeshTriangleFace(verticies[3], verticies[6], verticies[8]),
            new ShipMeshTriangleFace(verticies[3], verticies[8], verticies[9]),

            new ShipMeshTriangleFace(verticies[4], verticies[9], verticies[5]),
            new ShipMeshTriangleFace(verticies[2], verticies[4], verticies[11]),
            new ShipMeshTriangleFace(verticies[6], verticies[2], verticies[10]),
            new ShipMeshTriangleFace(verticies[8], verticies[6], verticies[7]),
            new ShipMeshTriangleFace(verticies[9], verticies[8], verticies[1])
        };

        var afterSubdivision = new List<ShipMeshFace>();

        foreach (var face in faces)
        {
            afterSubdivision.AddRange(face.Subdivide(numberOfSubdivisions));
        }

        var allVerticies = afterSubdivision.SelectMany(e => e.Vertices).Distinct();

        foreach (var v in allVerticies)
        {
            v.Coordinates = v.Coordinates * size / 2;
        }

        return new ShipMesh()
        {
            Faces = afterSubdivision
        };
    }
}
