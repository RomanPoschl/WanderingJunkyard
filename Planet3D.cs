using Godot;
using System;

public partial class Planet3D : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var st = new SurfaceTool();

		st.Begin(Godot.Mesh.PrimitiveType.Triangles);

		// Prepare attributes for add_vertex.
		st.SetNormal(new Vector3(0, 0, 1));
		st.SetUv(new Vector2(0, 0));
		// Call last for each vertex, adds the above attributes.
		st.AddVertex(new Vector3(-1, -1, 0));

		st.SetNormal(new Vector3(0, 0, 1));
		st.SetUv(new Vector2(0, 1));
		st.AddVertex(new Vector3(-1, 1, 0));

		st.SetNormal(new Vector3(0, 0, 1));
		st.SetUv(new Vector2(1, 1));
		st.AddVertex(new Vector3(1, 1, 0));

		// Commit to a mesh.
		var mesh = st.Commit();

		//var body = GetNode<MeshInstance3D>("Body");
		//body.Mesh = mesh;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
