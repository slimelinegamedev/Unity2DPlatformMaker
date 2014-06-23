using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CreateMesh2D),true),CanEditMultipleObjects]
public class CreateMesh2DEditor : Editor {
	
	CreateMesh2D createMesh;
	MeshFilter _meshFiler;
	Vector3[] _vertices;
	
	void Awake()
	{
		createMesh = (CreateMesh2D)target;
		_meshFiler = createMesh.GetComponent<MeshFilter>();
	}
	
	[MenuItem("GameObject/Create Mesh2D")]
	static void initMesh()
	{
		GameObject mesh = new GameObject();
		mesh.AddComponent<CreateMesh2D>();
	}
	
	public override void OnInspectorGUI()
	{
		if(!createMesh.EditingVertices)
			DrawDefaultInspector();
		
		EditorGUILayout.Space();
		if(createMesh.enabled)EditorGUILayout.HelpBox("Don't forget to save mesh and disable before building your game!",MessageType.Warning);
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Save Mesh"))
		{
			save();
		}
		GUI.color = Color.green;
		if(GUILayout.Button("Save Mesh and Disable"))
		{
			save();
			createMesh.enabled = false;
		}
		GUI.color = Color.white;
		GUILayout.EndHorizontal();
	

		if(!createMesh.EditingVertices)
		{
			if(GUILayout.Button("Edit Vertices"))
			{
				createMesh.EditingVertices = true;
			}
		}
		else 
		{
			if(GUILayout.Button("Revert"))
			{
				createMesh.EditingVertices = false;
			}
		}
	}
	void OnSceneGUI()
	{
		if(createMesh.EditingVertices)
		{
			_vertices = createMesh.Vertices;
			for(int i = 0; i< _vertices.Length; i++)
			{
				//				Vector3 pos =	Handles.DoPositionHandle(_vertices[i] + createMesh.transform.position, Quaternion.identity);
				Vector3 pos = Handles.FreeMoveHandle(_vertices[i] + createMesh.transform.position, Quaternion.identity, .25f, Vector3.one *.1f ,
				                                     Handles.CircleCap);
				
				if(pos != _vertices[i] + createMesh.transform.position) _vertices[i] = pos -createMesh.transform.position;

				if(GUI.changed) EditorUtility.SetDirty(createMesh);

			}
		}
		
	}
	
	void save()
	{
		if(Directory.Exists("Assets/Meshes/"))
		{
			var name = string.Concat("Mesh_", createMesh.name , ".asset");
			if(File.Exists("Assets/Meshes/" + name)) name = string.Concat("Mesh_", createMesh.name ,createMesh.GetInstanceID() , ".asset");
			AssetDatabase.CreateAsset(_meshFiler.sharedMesh,"Assets/Meshes/"+name);
			AssetDatabase.SaveAssets();
		}
		else
		{
			Directory.CreateDirectory("Assets/Meshes/");
			save ();
		}
	}
}
