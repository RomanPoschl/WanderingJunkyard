using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using Vector3 = Godot.Vector3;

public enum Axis
{
	Horizontal,
	Vertical
}

public partial class ShipMesh
{
	public ShipMeshVertex[] Vertices { get { return Faces.SelectMany(e => e.Vertices).Distinct().ToArray(); } }
	public List<ShipMeshFace> Faces { get; set; }

	public ShipMesh()
	{
		this.Faces = new List<ShipMeshFace>();
	}

	public void Scale(Vector3 scaleVector, ShipMeshVertex[] vertices)
	{
		var center = Vector3.Zero;

		foreach (var vert in vertices)
		{
			center += vert.Coordinates;
		}

		center = center / vertices.Count();

		foreach (var vert in vertices)
		{
			var centered = vert.Coordinates - center;
			vert.Coordinates = new Vector3(centered.x * scaleVector.x, centered.y * scaleVector.y, centered.z * scaleVector.z) + center;
		}
	}

	public void Translate(Vector3 translation, ShipMeshVertex[] vertices)
	{
		foreach (var vert in vertices)
		{
			vert.Coordinates += translation;
		}
	}

	public void Rotate(ShipMeshVertex[] vertices, Vector3 center, Quaternion quaterion)
	{
		foreach (var vert in vertices)
		{
			var centered = vert.Coordinates - center;
			vert.Coordinates = quaterion * vert.Coordinates + center;
		}
	}

	public ShipMeshFace[] ExtrudeDiscreetFace(ShipMeshFace face)
	{
		var faces = face.Extrude();

		foreach (var addedFace in faces)
		{
			this.Faces.Add(addedFace);
		}

		this.Faces.RemoveAt(this.Faces.IndexOf(face));

		return faces;
	}

	public void Scale(Vector3 scaleVector, System.Numerics.Matrix4x4 faceSpace, ShipMeshVertex[] vertices)
	{
		// Not sure what Blender does with the space matrix, so I'm gonna skip it for now
		var center = Vector3.Zero;

		foreach (var vert in vertices)
		{
			center += vert.Coordinates;
		}

		center = center / vertices.Count();

		foreach (var vert in vertices)
		{
			var transformedSpace = (vert.Coordinates - center);
			vert.Coordinates = (new Vector3(transformedSpace.x * scaleVector.x, transformedSpace.y * scaleVector.y, transformedSpace.z * scaleVector.z)) + center;
		}
	}

	public Mesh ToGodotMesh(bool smooth = false)
	{
		var mesh = new SurfaceTool();
		mesh.Begin(Mesh.PrimitiveType.Triangles);

		var faces = smooth
			? this.Faces.ToArray()
			: this.Faces.Select(e => e.Clone()).ToArray();

		var vertices = faces.SelectMany(f => f.Vertices);

		this.IndexAllVertexes(vertices);
		var coordinates = vertices.Select(e => e.Coordinates).ToArray();

		var centerOfMass = Vector3.Zero;
		foreach (var vertex in coordinates) centerOfMass += vertex;
		centerOfMass /= coordinates.Length;

		foreach (var face in faces)
		{
			var faceIndexes = face.GetTriangles();
			var faceVertices = face.Vertices;

			foreach (var item in faceVertices)
			{
				mesh.AddVertex(item.Coordinates);
			}

			foreach(var item in faceIndexes)
			{
				mesh.AddIndex(item);
			}
		}

		GD.Print("Vertex count: " + vertices.Count());

		return mesh.Commit();
	}

	private void IndexAllVertexes(IEnumerable<ShipMeshVertex> vertices)
	{
		var index = 0;

		foreach (var vertex in vertices)
		{
			vertex.Index = index;
			index++;
		}
	}

	internal void Symmetrize(Axis axis)
	{
		// TODO
	}

	public ShipMeshFace[] Subdivide(ShipMeshFace face, int numberOfCuts)
	{
		var result = face.Subdivide(numberOfCuts);

		this.Faces.RemoveAt(this.Faces.IndexOf(face));
		this.Faces.AddRange(result);

		return result;
	}

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = lineVec1.Cross(lineVec2);
		Vector3 crossVec3and2 = lineVec3.Cross(lineVec2);

		float planarFactor = lineVec3.Dot(crossVec1and2);

		//is coplanar, and not parrallel
		if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.LengthSquared() > 0.0001f)
		{
			float s = crossVec3and2.Dot(crossVec1and2) / crossVec1and2.LengthSquared();
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.Zero;
			return false;
		}
	}

	public void CreateCylinder(int numberOfSegments, float cylinderSize1, float cylinderSize2, float cylinderDepth, System.Numerics.Matrix4x4 cylinderMatrix)
	{
		var cylinderMesh = ShipMeshFactory.CreateCylinder(numberOfSegments, cylinderSize1, cylinderSize2, cylinderDepth);

		foreach (var vertex in cylinderMesh.Vertices)
		{
			vertex.Coordinates = ShipGenerator3D.MultiplyPoint3x4(cylinderMatrix, vertex.Coordinates);
		}

		foreach (var face in cylinderMesh.Faces)
		{
			this.Faces.Add(face);
		}
	}

	internal void CreateIcosphere(int subdivisions, float sphereSize, System.Numerics.Matrix4x4 sphereMatrix)
	{
		var icosphereMesh = ShipMeshFactory.CreateIcosphere(subdivisions, sphereSize);

		foreach (var vertex in icosphereMesh.Vertices)
		{
			vertex.Coordinates = ShipGenerator3D.MultiplyPoint3x4(sphereMatrix, vertex.Coordinates);
		}

		foreach (var face in icosphereMesh.Faces)
		{
			this.Faces.Add(face);
		}
	}
}
