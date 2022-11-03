using Godot;
using Vector3 = Godot.Vector3;
using Vector2 = Godot.Vector2;
using System.Collections.Generic;
using System.Linq;
using System;

public partial class ShipGenerator3D
{
	private readonly RandomNumberGenerator _rng;

	public ShipGenerator3D(RandomNumberGenerator rng)
	{
		_rng = rng;
	}

	private bool smoothShading = false;
	private int seed = 844483692;
	private int numHullSegmentsMin = 3;
	private int numHullSegmentsMax = 6;
	private bool createAsymmetrySegments = true;
	private int numAsymmetrySegmentsMin = 1;
	private int numAsymmetrySegmentsMax = 5;
	private bool createFaceDetail = true;
	private bool allowHorizontalSymmetry = true;
	private bool allowVerticalSymmetry = true;
	private bool applyBevelModifier = true;
	private bool assignMaterials = true;

	public void RandomizeSeed()
	{
		seed = _rng.RandiRange(0, int.MaxValue);
	}

	public IEnumerable<ShipMesh> Generate(ShipMesh ShipMesh)
	{
		var scaleFactor = _rng.RandfRange(0.75f, 2.0f);
		var scaleVector = Vector3.One * scaleFactor;

		ShipMesh.Scale(scaleVector, ShipMesh.Vertices);

		var faces = ShipMesh.Faces.ToArray();
		for (var f = 0; f < faces.Length; f++)
		{
			var face = faces[f];

			if (Mathf.Abs(face.Normal.x) > 0.5f)
			{
				var hullSegmentLength = _rng.RandfRange(0.3f, 1.0f);
				var numberOfHullSegments = _rng.RandiRange(numHullSegmentsMin, numHullSegmentsMax);

				for (var i = 0; i < numberOfHullSegments; i++)
				{
					var isLastHullSegment = i == numberOfHullSegments - 1;
					var val = _rng.Randf();

					if (val > 0.1f)
					{
						// Most of the time, extrude out the face with some random deviations
						face = this.ExtrudeFace(ShipMesh, face, hullSegmentLength);

						if (_rng.Randf() > 0.75f)
						{
							face = this.ExtrudeFace(ShipMesh, face, hullSegmentLength * 0.25f);
						}

						// Maybe apply some scaling
						if (_rng.Randf() > 0.5f)
						{
							var sy = _rng.RandfRange(1.2f, 1.5f);
							var sz = _rng.RandfRange(1.2f, 1.5f);

							if (isLastHullSegment || _rng.Randf() > 0.5f)
							{
								sy = 1f / sy;
								sz = 1f / sz;
							}

							this.ScaleFace(ShipMesh, face, 1f, sy, sz);
						}

						// Maybe apply some sideways translation
						if (_rng.Randf() > 0.5f)
						{
							var sidewaysTranslation = new Vector3(0f, 0f, _rng.RandfRange(0.1f, 0.4f) * scaleVector.z * hullSegmentLength);

							if (_rng.Randf() > 0.5f)
							{
								sidewaysTranslation = -sidewaysTranslation;
							}

							ShipMesh.Translate(sidewaysTranslation, face.Vertices);
						}

						// Maybe add some rotation around Z axis
						if (_rng.Randf() > 0.5f)
						{
							var angle = 5f;
							if (_rng.Randf() > 0.5f)
							{
								angle = -angle;
							}

							var quaterion = new Quaternion(new Vector3(0, 0, 1), angle);
							ShipMesh.Rotate(face.Vertices, new Vector3(0, 0, 0), quaterion);
						}
					}
					else
					{
						// Rarely, create a ribbed section of the hull
						var ribScale = _rng.RandfRange(0.75f, 0.95f);
						face = this.RibbedExtrudeFace(ShipMesh, face, hullSegmentLength, _rng.RandiRange(2, 4), ribScale);
					}
				}
			}
		}

		yield return ShipMesh;

		//Add some large asymmetrical sections of the hull that stick out
		if (createAsymmetrySegments)
		{
			var potentialFaces = ShipMesh.Faces.ToArray();
			for (var f = 0; f < potentialFaces.Length; f++)
			{
				var face = potentialFaces[f];

				if (face.AspectRatio > 4f)
				{
					continue;
				}

				if (_rng.Randf()> 0.85f)
				{
					var hullPieceLength = _rng.RandfRange(0.1f, 0.4f);
					var totalSegments = _rng.RandiRange(numAsymmetrySegmentsMin, numAsymmetrySegmentsMax);

					for (var i = 0; i < totalSegments; i++)
					{
						face = ExtrudeFace(ShipMesh, face, hullPieceLength);

						// Maybe apply some scaling
						if (_rng.Randf()> 0.25f)
						{
							var s = 1f / _rng.RandfRange(1.1f, 1.5f);
							ScaleFace(ShipMesh, face, s, s, s);
						}
					}
				}
			}
		}

		// Now the basic hull shape is built, let's categorize + add detail to all the faces
		if (createFaceDetail)
		{
			var engineFaces = new List<ShipMeshFace>();
			var gridFaces = new List<ShipMeshFace>();
			var antennaFaces = new List<ShipMeshFace>();
			var weaponFaces = new List<ShipMeshFace>();
			var sphereFaces = new List<ShipMeshFace>();
			var discFaces = new List<ShipMeshFace>();
			var cylinderFaces = new List<ShipMeshFace>();

			faces = ShipMesh.Faces.ToArray();
			for (var f = 0; f < faces.Length; f++)
			{
				var face = faces[f];

				// Skip any long thin faces as it'll probably look stupid
				if (face.AspectRatio > 3) continue;

				// Spin the wheel! Let's categorize + assign some materials
				var val = _rng.Randf();

				if (face.Normal.x < -0.9f)
				{
					if (!engineFaces.Any() || val > 0.75f)
						engineFaces.Add(face);
					else if (val > 0.5f)
						cylinderFaces.Add(face);
					else if (val > 0.25f)
						gridFaces.Add(face);
					else
						face.MaterialIndex = 1;//Material.hull_lights
				}
				else if (face.Normal.x > 0.9f) // front face
				{
					if (face.Normal.Dot(face.CalculateCenterBounds()) > 0 && val > 0.7f)
					{
						antennaFaces.Add(face);  // front facing antenna
						face.MaterialIndex = 1;// Material.hull_lights
					}
					else if (val > 0.4f)
						gridFaces.Add(face);
					else
						face.MaterialIndex = 1;// Material.hull_lights
				}
				else if (face.Normal.y > 0.9f) // top face
				{
					if (face.Normal.Dot(face.CalculateCenterBounds()) > 0 && val > 0.7f)
						antennaFaces.Add(face);  // top facing antenna
					else if (val > 0.6f)
						gridFaces.Add(face);
					else if (val > 0.3f)
						cylinderFaces.Add(face);
				}
				else if (face.Normal.y < -0.9f) // bottom face
				{
					if (val > 0.75f)
						discFaces.Add(face);
					else if (val > 0.5f)
						gridFaces.Add(face);
					else if (val > 0.25f)
						weaponFaces.Add(face);
				}
				else if (Mathf.Abs(face.Normal.z) > 0.9f) // side face
				{
					if (!weaponFaces.Any() || val > 0.75f)
						weaponFaces.Add(face);
					else if (val > 0.6f)
						gridFaces.Add(face);
					else if (val > 0.4f)
						sphereFaces.Add(face);
					else
						face.MaterialIndex = 1;// Material.hull_lights
				}
			}

			// Now we've categorized, let's actually add the detail
			foreach (var fac in engineFaces)
				AddExhaustToFace(ShipMesh, fac);

			foreach (var fac in gridFaces)
				AddGridToFace(ShipMesh, fac);

			foreach (var fac in antennaFaces)
				AddSurfaceAntennaToFace(ShipMesh, fac);

			foreach (var fac in weaponFaces)
				AddWeaponsToFace(ShipMesh, fac);

			foreach (var fac in sphereFaces)
				AddSphereToFace(ShipMesh, fac);

			foreach (var fac in discFaces)
				AddDiscToFace(ShipMesh, fac);

			foreach (var fac in cylinderFaces)
				AddCylindersToFace(ShipMesh, fac);
		}

		// Apply horizontal symmetry sometimes
		if (allowHorizontalSymmetry && _rng.Randf()> 0.5f)
			ShipMesh.Symmetrize(Axis.Horizontal);

		// Apply vertical symmetry sometimes - this can cause spaceship "islands", so disabled by default
		if (allowHorizontalSymmetry && _rng.Randf()> 0.5f)
			ShipMesh.Symmetrize(Axis.Vertical);

		if (applyBevelModifier)
		{
			// TODO
		}
	}

	private System.Numerics.Matrix4x4 GetFaceMatrix(ShipMeshFace face, Vector3? position = null)
	{
		var xAxis = (face.RightTop.Coordinates - face.LeftTop.Coordinates).Normalized();
		var zAxis = -face.Normal;
		var yAxis = zAxis.Cross(xAxis);

		if (!position.HasValue)
		{
			position = face.CalculateCenterBounds();
		}

		var mat = new System.Numerics.Matrix4x4();
		mat.M11 = xAxis.x;
		mat.M21 = xAxis.y;
		mat.M31 = xAxis.z;
		mat.M41 = 0;
		mat.M12 = yAxis.x;
		mat.M22 = yAxis.y;
		mat.M32 = yAxis.z;
		mat.M42 = 0;
		mat.M13 = zAxis.x;
		mat.M23 = zAxis.y;
		mat.M33 = zAxis.z;
		mat.M43 = 0;
		mat.M14 = position.Value.x;
		mat.M24 = position.Value.y;
		mat.M34 = position.Value.z;
		mat.M44 = 1;

		return mat;
	}

	private void AddCylindersToFace(ShipMesh ShipMesh, ShipMeshFace fac)
	{
		var horizontalStep = _rng.RandiRange(1, 3);
		var verticalStep = _rng.RandiRange(1, 3);
		var numberOfSegments = _rng.RandiRange(6, 12);
		var faceWidth = fac.Width;
		var faceHeight = fac.Height;
		var cylinderDepth = 1.3f * Mathf.Min(faceWidth / (horizontalStep + 2), faceHeight / (verticalStep + 2));
		var cylinderSize = cylinderDepth * 0.5f;

		for (var h = 0; h < horizontalStep; h++)
		{
			var top = fac.LeftTop.Coordinates.Lerp(fac.RightTop.Coordinates, ((float)h + 1) / (horizontalStep + 1));
			var bottom = fac.LeftBottom.Coordinates.Lerp(fac.RightBottom.Coordinates, ((float)h + 1) / (horizontalStep + 1));

			for (var v = 0; v < verticalStep; v++)
			{
				var pos = top.Lerp(bottom, ((float)v + 1) / (verticalStep + 1));
				var cylinderMatrix = GetFaceMatrix(fac, pos) * GetRotationMatrix4x4(new Quaternion(new Vector3(0, 1, 0), 90));

				ShipMesh.CreateCylinder(numberOfSegments, cylinderSize, cylinderSize, cylinderDepth, cylinderMatrix);
			}
		}

	}

	private void AddDiscToFace(ShipMesh ShipMesh, ShipMeshFace fac)
	{
		var faceWidth = fac.Width;
		var faceHeight = fac.Height;
		var depth = 0.125f * Mathf.Min(faceWidth, faceHeight);

		ShipMesh.CreateCylinder(
			32,
			depth * 3,
			depth * 4,
			depth,
			GetFaceMatrix(fac, fac.CalculateCenterBounds() + fac.Normal * depth * 0.5f));

		ShipMesh.CreateCylinder(
			32,
			depth * 1.25f,
			depth * 2.25f,
			0.0f,
			GetFaceMatrix(fac, fac.CalculateCenterBounds() + fac.Normal * depth * 1.05f));
	}

	private void AddSphereToFace(ShipMesh ShipMesh, ShipMeshFace fac)
	{
		var sphereSize = _rng.RandfRange(0.4f, 1f) * Mathf.Min(fac.Width, fac.Height);
		var sphereMatrix = GetFaceMatrix(fac, fac.CalculateCenterBounds() - fac.Normal * _rng.RandfRange(0f, sphereSize * 0.5f));
		ShipMesh.CreateIcosphere(3, sphereSize, sphereMatrix);
	}

	private void AddWeaponsToFace(ShipMesh ShipMesh, ShipMeshFace fac)
	{
		var horizontalStep = _rng.RandiRange(1, 3);
		var verticalStep = _rng.RandiRange(1, 3);
		var numSegments = 16;

		var weaponSize = 0.5f * Mathf.Min(fac.Width / (horizontalStep + 2), fac.Height / (verticalStep + 2));
		var weaponDepth = weaponSize * 0.2f;

		for (var h = 0; h < horizontalStep; h++)
		{
			var top = fac.LeftTop.Coordinates.Lerp(fac.RightTop.Coordinates, (float)(h + 1) / (horizontalStep + 1));
			var bottom = fac.LeftBottom.Coordinates.Lerp(fac.RightBottom.Coordinates, (float)(h + 1) / (horizontalStep + 1));

			for (var v = 0; v < verticalStep; v++)
			{
				var pos = top.Lerp(bottom, (float)(v + 1) / (verticalStep + 1));
				var faceMatrix = GetFaceMatrix(fac, pos + fac.Normal * weaponDepth * 0.5f) *
					GetRotationMatrix4x4(new Quaternion(new Vector3(0, 0, 1), _rng.RandiRange(0, 90)));


				// Turret foundation
				ShipMesh.CreateCylinder(numSegments, weaponSize * 0.9f, weaponSize, weaponDepth, faceMatrix);

				// Turret left guard
				var leftGuardMat = faceMatrix *
					GetRotationMatrix4x4(new Quaternion(new Vector3(0, 1, 0), 90)) *
					GetTranslationMatrix4x4(new Vector3(0, 0, weaponSize * 0.6f));
				ShipMesh.CreateCylinder(numSegments, weaponSize * 0.6f, weaponSize * 0.5f, weaponDepth * 2, leftGuardMat);

				// Turret right guard
				var rightGuardMat = faceMatrix *
					GetRotationMatrix4x4(new Quaternion(new Vector3(0, 1, 0), 90)) *
					GetTranslationMatrix4x4(new Vector3(0, 0, weaponSize * -0.6f));
				ShipMesh.CreateCylinder(numSegments, weaponSize * 0.5f, weaponSize * 0.6f, weaponDepth * 2, rightGuardMat);

				// Turret housing
				var upwardAngle = _rng.RandiRange(0, 45);
				var turretHouseMat = faceMatrix *
					GetRotationMatrix4x4(new Quaternion(new Vector3(1, 0, 0), upwardAngle)) *
					GetTranslationMatrix4x4(new Vector3(0, weaponSize * -0.4f, 0));
				ShipMesh.CreateCylinder(8, weaponSize * 0.4f, weaponSize * 0.4f, weaponDepth * 5, turretHouseMat);

				// Turret barrels L + R
				ShipMesh.CreateCylinder(8, weaponSize * 0.1f, weaponSize * 0.1f, weaponDepth * 6, turretHouseMat *
					GetTranslationMatrix4x4(new Vector3(weaponSize * 0.2f, 0, -weaponSize)));
				ShipMesh.CreateCylinder(8, weaponSize * 0.1f, weaponSize * 0.1f, weaponDepth * 6, turretHouseMat *
					GetTranslationMatrix4x4(new Vector3(weaponSize * -0.2f, 0, -weaponSize)));
			}
		}
	}

	private void AddSurfaceAntennaToFace(ShipMesh ShipMesh, ShipMeshFace fac)
	{
		var horizontalStep = _rng.RandiRange(4, 10);
		var verticalStep = _rng.RandiRange(4, 10);

		for (var h = 0; h < horizontalStep; h++)
		{
			var top = fac.LeftTop.Coordinates.Lerp(fac.RightTop.Coordinates, ((float)h + 1) / (horizontalStep + 1));
			var bottom = fac.LeftBottom.Coordinates.Lerp(fac.RightBottom.Coordinates, ((float)h + 1) / (horizontalStep + 1));

			for (var v = 0; v < verticalStep; v++)
			{
				if (_rng.Randf()> 0.9f)
				{
					var pos = top.Lerp(bottom, ((float)v + 1) / (verticalStep + 1));
					var faceSize = Mathf.Sqrt(fac.Area3D());
					var depth = _rng.RandfRange(0.1f, 1.5f) * faceSize;
					var depthShort = depth * _rng.RandfRange(0.02f, 0.15f);
					var baseDiameter = _rng.RandfRange(0.005f, 0.05f);
					var materialIndex = _rng.Randf()> 0.5f ? 0 /*Material.hull*/ : 1;/*Material.hull_dark*/

					// Spire
					var numSegments = _rng.RandiRange(3, 6);
					ShipMesh.CreateCylinder(numSegments, 0, baseDiameter, depth, GetFaceMatrix(fac, pos + fac.Normal * depth * 0.5f));

					// Base
					ShipMesh.CreateCylinder(
						numSegments,
						baseDiameter * _rng.RandfRange(1f, 1.5f),
						baseDiameter * _rng.RandfRange(1.5f, 2f),
						depthShort,
						GetFaceMatrix(fac, pos + fac.Normal * depthShort * 0.45f));
				}
			}
		}
	}

	private void AddGridToFace(ShipMesh ShipMesh, ShipMeshFace fac)
	{
		var result = ShipMesh.Subdivide(fac, _rng.RandiRange(2, 4));
		var gridLength = _rng.RandfRange(0.025f, 0.15f);
		var scale = 0.8f;

		for (var i = 0; i < result.Length; i++)
		{
			var face = result[i];
			var materialIndex = _rng.Randf()> 0.5f ? 1/*Material.hull_lights*/ : 4 /*Material.hull*/;
			var extrudedFaceList = new List<ShipMeshFace>();

			face = ExtrudeFace(ShipMesh, face, gridLength, extrudedFaceList);

			foreach (var f in extrudedFaceList)
			{
				if (Mathf.Abs(face.Normal.z) < 0.707) // # side face
					f.MaterialIndex = materialIndex;
			}

			ScaleFace(ShipMesh, face, scale, scale, scale);
		}
	}

	// Given a face, splits it into a uniform grid and extrudes each grid face
	// out and back in again, making an exhaust shape.
	private void AddExhaustToFace(ShipMesh ShipMesh, ShipMeshFace faceForExhaust)
	{
		// The more square the face is, the more grid divisions it might have
		var num_cuts = _rng.RandiRange(1, (int)(4 - faceForExhaust.AspectRatio));
		var result = ShipMesh.Subdivide(faceForExhaust, num_cuts);

		var exhaust_length = _rng.RandfRange(0.1f, 0.2f);
		var scaleOuter = 1f / _rng.RandfRange(1.3f, 1.6f);
		var scale_inner = 1f / _rng.RandfRange(1.05f, 1.1f);

		for (var i = 0; i < result.Count(); i++)
		{
			var face = result[i];
			face.MaterialIndex = 2;// Material.hull_dark;

			face = ExtrudeFace(ShipMesh, face, exhaust_length);
			ScaleFace(ShipMesh, face, scaleOuter, scaleOuter, scaleOuter);

			face = ExtrudeFace(ShipMesh, face, 0);
			ScaleFace(ShipMesh, face, scaleOuter * 0.9f, scaleOuter * 0.9f, scaleOuter * 0.9f);

			var extruded_face_list = new List<ShipMeshFace>();
			face = ExtrudeFace(ShipMesh, face, -exhaust_length * 0.9f, extruded_face_list);

			foreach (var extruded_face in extruded_face_list)
				extruded_face.MaterialIndex = 3;// Material.exhaust_burn

			ScaleFace(ShipMesh, face, scale_inner, scale_inner, scale_inner);
		}
	}

	private ShipMeshFace RibbedExtrudeFace(ShipMesh ShipMesh, ShipMeshFace face, float distance, int numberOfRibs = 3, float ribScale = 0.9f)
	{
		var distancePerRib = distance / numberOfRibs;
		var newFace = face;

		for (var i = 0; i < numberOfRibs; i++)
		{
			newFace = ExtrudeFace(ShipMesh, newFace, distancePerRib * 0.25f);
			newFace = ExtrudeFace(ShipMesh, newFace, 0.0f);
			ScaleFace(ShipMesh, newFace, ribScale, ribScale, ribScale);
			newFace = ExtrudeFace(ShipMesh, newFace, distancePerRib * 0.5f);
			newFace = ExtrudeFace(ShipMesh, newFace, 0.0f);
			ScaleFace(ShipMesh, newFace, 1 / ribScale, 1 / ribScale, 1 / ribScale);
			newFace = ExtrudeFace(ShipMesh, newFace, distancePerRib * 0.25f);
		}

		return newFace;
	}

	private void ScaleFace(ShipMesh ShipMesh, ShipMeshFace face, float sx, float sy, float sz)
	{
		System.Numerics.Matrix4x4.Invert(GetFaceMatrix(face), out var invertedMatrix);
		ShipMesh.Scale(new Vector3(sx, sy, sz), invertedMatrix, face.Vertices);
	}

	private ShipMeshFace ExtrudeFace(ShipMesh ShipMesh, ShipMeshFace face, float distance, List<ShipMeshFace> extrudedFaces = null)
	{
		var newFaces = ShipMesh.ExtrudeDiscreetFace(face);

		if (extrudedFaces != null)
		{
			extrudedFaces.AddRange(newFaces);
		}

		var newFace = newFaces[0];
		ShipMesh.Translate(newFace.Normal * distance, newFace.Vertices);

		return newFace;
	}

	public static System.Numerics.Matrix4x4 GetTranslationMatrix4x4(Vector3 vector)
	{
		return new System.Numerics.Matrix4x4()
		{
			M11 = 1f,
			M12 = 0f,
			M13 = 0f,
			M14 = vector.x,
			M21 = 0f,
			M22 = 1f,
			M23 = 0f,
			M24 = vector.y,
			M31 = 0f,
			M32 = 0f,
			M33 = 1f,
			M34 = vector.z,
			M41 = 0f,
			M42 = 0f,
			M43 = 0f,
			M44 = 1f
		};
	}

	public static System.Numerics.Matrix4x4 GetRotationMatrix4x4(Quaternion quat)
	{
		float x = quat.x * 2.0F;
		float y = quat.y * 2.0F;
		float z = quat.z * 2.0F;
		float xx = quat.x * x;
		float yy = quat.y * y;
		float zz = quat.z * z;
		float xy = quat.x * y;
		float xz = quat.x * z;
		float yz = quat.y * z;
		float wx = quat.w * x;
		float wy = quat.w * y;
		float wz = quat.w * z;

		// Calculate 3x3 matrix from orthonormal basis
		return new System.Numerics.Matrix4x4() {
			M11 = 1f - (yy + zz),M21 = xy + wz, M31 = xz - wy, M41 = .0f,
			M12 = xy - wz, M22 = 1f - (xx + zz), M32 = yz + wx, M42 = .0f,
			M13 = xz + wy, M23 = yz - wx, M33 = 1f - (xx + yy), M43 = .0f,
			M14 = .0f, M24 = .0f, M34 = .0f, M44 = 1f,
		};
	}

	public static Vector3 MultiplyPoint3x4(System.Numerics.Matrix4x4 m, Vector3 point)
	{
		Vector3 res;
		res.x = m.M11 * point.x + m.M12 * point.y + m.M13 * point.z + m.M14;
		res.y = m.M21 * point.x + m.M22 * point.y + m.M23 * point.z + m.M24;
		res.z = m.M31 * point.x + m.M32 * point.y + m.M33 * point.z + m.M34;
		return res;
	}
}
