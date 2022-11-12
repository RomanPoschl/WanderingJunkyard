using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class PlanetMeshFace : MeshInstance3D
{
	[Export]
	public Vector3 normal;
	
	public void RegenerateMesh(Resource pCResource)
	{
		if(pCResource is null)
			return;

		var pC = pCResource as PlanetConfiguration;

		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		var axisA = new Vector3(normal.y, normal.z, normal.x);
		var axisB = normal.Cross(axisA);

		for(var y = 0; y < pC.Resolution; y++)
		{
			for(var x = 0; x < pC.Resolution; x++)
			{
				var i = x + y * pC.Resolution;
				var percent = new Vector2(x, y) / (pC.Resolution - 1);
				var pointOnUnitCube = (normal + (percent.x - .5f) * 2f * axisA + (percent.y - .5f) * 2f * axisB);
				var pointOnUnitSphere = pointOnUnitCube.Normalized();

				var elevation = 0f;
				foreach(var noiseMap in pC.PlanetNoises)
				{
					if (noiseMap is null)
						continue;

					var nM = noiseMap as PlanetNoise;
					var ele = nM.NoiseMap.GetNoise3dv(pointOnUnitSphere * 100);
					ele += 1 / 2f * nM.Amplitude;
					ele = Mathf.Max(0f, ele - nM.MinHeight);
					elevation += ele;
				}
				var point = pointOnUnitSphere * pC.Radius * (elevation + 1);

				st.AddVertex(point);
				if (x != pC.Resolution - 1 && y != pC.Resolution - 1)
				{
					st.AddIndex(i + pC.Resolution);
					st.AddIndex(i + pC.Resolution + 1);
					st.AddIndex(i);
					st.AddIndex(i + pC.Resolution + 1);
					st.AddIndex(i + 1);
					st.AddIndex(i);
				}
			}
		}

		st.GenerateNormals();
		this.Mesh = st.Commit();
	}
}
