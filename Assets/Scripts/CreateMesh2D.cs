using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class CreateMesh2D : MonoBehaviour {
	/* face down options */
	public OrientationOptions orientationOptions;
	/** Is collidable or just probe*/
	public ColliderOptions colliderOptions;
	/** Lenght of mesh */
	public float length = 70f;
	/** Determines smoothness */
	public int numOfTopVertices = 3;
	/** y curve , y values of vertices */
	public AnimationCurve topVerticesCurve = AnimationCurve.Linear(0f,1f,1f,1f);
	/** y values multiplier */
	public float height = 10f;
	/** snap x to vertice count for true lenght*/
	public bool trueLenght = false;
	/** want to tile with xlenght */
	public bool tileWithXLenght = false;
	/** we want curve to be smooth or broken? */
	public bool smoothCurve = true;
	public float nonSmoothFactor = .5f;
	
	public Vector3[] Vertices {
		get {
			return _vertices;
		}
		set{
			_vertices = value;
		}
	}
	
	public bool EditingVertices {
		get{
			return _editingVertices;
		}
		set {
			_editingVertices = value;
		}
	}
	
	/** Mesh filter component */
	private MeshFilter _meshFilter;
	/** Renderer */
	private MeshRenderer _renderer;
	/** Mesh component */
	private Mesh mMesh;
	/** Total vertice count */
	private int _verticeCount;
	/** Vertice x pos counter */
	private float _verticeXPosCounter = 0;
	/** x length add of one vertice, pos actually */
	private float verticeXPositionAdd{get { return length/(numOfTopVertices-1);}}
	/** Vertices array */
	Vector3[] _vertices;
	/** UV's */
	Vector2[] _UVs;
	/** Triangles */
	int[] _tris;
	/** tris holder int */
	int _trisStartVerticeHolder;
	int _trisCounter = 0;
	/* Polygon collider */
	PolygonCollider2D _polyCollider;
	Vector2[] _colliderPoints;
	/** Mesh collider op*/
	MeshCollider _meshCollider;
	bool _editingVertices = false;
	
	protected void Awake ()
	{
		if(GetComponent<MeshFilter>() == null ) _meshFilter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
		if(GetComponent<MeshRenderer>() == null ) _renderer = gameObject.AddComponent<MeshRenderer>();
		
		_meshFilter = GetComponent<MeshFilter>();
		_renderer = gameObject.GetComponent<MeshRenderer>();
		// total vertice count 
		_verticeCount = numOfTopVertices * 2; 
		if(trueLenght) length = numOfTopVertices -1;
		
		// set up vertice array 
		_vertices = new Vector3[_verticeCount];
		// set up uv s array
		_UVs = new Vector2[_verticeCount];
		// set up tris array
		_tris = new int[(( numOfTopVertices - 1) * 6)];
		
		_colliderPoints = new Vector2[_verticeCount];
		// Get collider data
		
		if(colliderOptions.isCollidable)
		{
			if(colliderOptions.colliderType.Equals(ColliderOptions.ColliderType.Polygon))
			{
				if(GetComponent<PolygonCollider2D>() == null) _polyCollider= gameObject.AddComponent<PolygonCollider2D>() as PolygonCollider2D;
				_polyCollider = GetComponent<PolygonCollider2D>();
			}
			else 
			{
				if(GetComponent<MeshCollider>() == null) _meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
				_meshCollider = GetComponent<MeshCollider>();
			}
			
		}
		if(tileWithXLenght)
			_renderer.sharedMaterial.mainTextureOffset = new Vector2(numOfTopVertices -1f , _renderer.sharedMaterial.mainTextureOffset.y);
	}
	
	
	#if UNITY_EDITOR
	public virtual void Update () 
	{
		
		if(_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
		if(colliderOptions.colliderType.Equals(ColliderOptions.ColliderType.Polygon)&& colliderOptions.isCollidable)
		{
			if(_polyCollider == null ) _polyCollider = GetComponent<PolygonCollider2D>();
		}
		else 
		{
			if(_meshCollider == null && colliderOptions.isCollidable) _meshCollider = GetComponent<MeshCollider>();
		}
		if(numOfTopVertices > 1) makeMesh(topVerticesCurve);
		
	}
	#endif
	/// <summary>
	/// Makes the mesh, adjusts top vertices due to curve 
	/// </summary>
	/// <returns>Created mesh.</returns>
	public Mesh makeMesh (AnimationCurve curve)
	{
		#if UNITY_EDITOR
		if(_verticeCount != numOfTopVertices * 2 ) reCalculate();
		#endif
		topVerticesCurve = curve;
		if(!orientationOptions.orientation.Equals(OrientationOptions.Orientation.FaceDown) && !_editingVertices)
		{
			// CALCULATE VERTICES 
			for(int i = 0; i< _verticeCount; i++)
			{
				if(i <= _verticeCount / 2 -1)
				{
					// make bottom vertices
					_vertices[i] = new Vector3(_verticeXPosCounter,
					                           0f ,
					                           0f);
					
					if(colliderOptions.isCollidable)_colliderPoints[i] = new Vector2(_verticeXPosCounter, 0f);
					// count added vertices
					_verticeXPosCounter += verticeXPositionAdd;
					if(i == _verticeCount /2 -1) _verticeXPosCounter = 0;
				}
				else 
				{
					float y = i != _verticeCount -1 ? topVerticesCurve.Evaluate( (float)( i - numOfTopVertices)/ numOfTopVertices) * height : 
						topVerticesCurve.Evaluate( (float)( i - numOfTopVertices + 1)/ numOfTopVertices) * height;
					if( i % 2 == 0 && !smoothCurve) y -= nonSmoothFactor;
					
					_vertices[i] = new Vector3(_verticeXPosCounter , 
					                           y ,
					                           0f);
					if(colliderOptions.isCollidable)_colliderPoints[i] = new Vector2(_verticeXPosCounter, y);
					_verticeXPosCounter += verticeXPositionAdd;
				}
			}
		}
		else if(orientationOptions.orientation.Equals(OrientationOptions.Orientation.FaceDown) && !_editingVertices)
		{
			// CALCULATE VERTICES 
			for(int i = 0; i< _verticeCount; i++)
			{
				if(i <= _verticeCount / 2 -1)
				{
					float y = i != _verticeCount/2 -1 ? topVerticesCurve.Evaluate( (float)( i )/ numOfTopVertices) * height : 
						topVerticesCurve.Evaluate( (float)( i  + 1)/ numOfTopVertices) * height;
					if( i % 2 == 0 && !smoothCurve) y -= nonSmoothFactor;
					
					_vertices[i] = new Vector3(_verticeXPosCounter, 
					                           y ,
					                           0f);
					
					if(colliderOptions.isCollidable && colliderOptions.colliderType == ColliderOptions.ColliderType.Polygon)_colliderPoints[i] = new Vector2(_verticeXPosCounter, 0f);
					// count added vertices
					_verticeXPosCounter += verticeXPositionAdd;
					if(i == _verticeCount /2 -1) _verticeXPosCounter = 0;
				}
				else 
				{
					float y = i != _verticeCount -1 ? topVerticesCurve.Evaluate( (float)( i - numOfTopVertices)/ numOfTopVertices) * height : 
						topVerticesCurve.Evaluate( (float)( i - numOfTopVertices + 1)/ numOfTopVertices) * height;
					if( i % 2 == 0 && !smoothCurve) y -= nonSmoothFactor;
					
					_vertices[i] = new Vector3(_verticeXPosCounter, 
					                           y ,
					                           orientationOptions.depth);
					if(colliderOptions.isCollidable && colliderOptions.colliderType == ColliderOptions.ColliderType.Polygon)_colliderPoints[i] = new Vector2(_verticeXPosCounter, y);
					_verticeXPosCounter += verticeXPositionAdd;
				}
			}
		}
		_trisCounter =0;
		// CALCULATE TRIANGLES
		for (int j = 0; j < _tris.Length/6; j++)
		{
			// side
			_tris[_trisCounter] 	= j;
			_tris[_trisCounter + 1] = j + numOfTopVertices+ 1;
			_tris[_trisCounter + 2] = j  + 1;
			_tris[_trisCounter + 3] = j;
			_tris[_trisCounter + 4] = j + numOfTopVertices ;
			_tris[_trisCounter + 5] = j + numOfTopVertices+ 1;
			_trisCounter += 6;
		}
		
		// CALCULATE UV S
		for(int k = 0; k < 2; k++)
		{
			for(int l = 0; l < numOfTopVertices; l++)
			{
				int index = k<1 ? l + (k * _UVs.Length / numOfTopVertices) : l + (k * _UVs.Length / numOfTopVertices) + numOfTopVertices - 2;
				_UVs[index] = new Vector2(l * (1f/(numOfTopVertices - 1f)), k );
			}
		}
		
		_verticeXPosCounter = 0f;
		_trisCounter = 0;
		// apply mesh
		mMesh = new Mesh();
	
		mMesh.vertices = _vertices;
		mMesh.uv = _UVs;
		mMesh.triangles = _tris;
		
		mMesh.RecalculateNormals();
		mMesh.RecalculateBounds();
		mMesh.Optimize();
		tangentSolver(mMesh, _vertices, _UVs, _tris);
		_meshFilter.mesh = mMesh;
		// adjust collider
		if(colliderOptions.isCollidable)
		{
			if(_polyCollider != null && colliderOptions.colliderType.Equals(ColliderOptions.ColliderType.Polygon))
			{
				_polyCollider.points = _colliderPoints;
			}
			else if( _meshCollider != null && colliderOptions.colliderType.Equals(ColliderOptions.ColliderType.Mesh3D))
			{
				_meshCollider.sharedMesh = mMesh;
			}
			else
			{
				#if UNITY_EDITOR
				reCalculate();
				#endif
			}
		}
		
		// check lenght option
		if(trueLenght) length = numOfTopVertices -1;
		
		// check tiling option
		if(tileWithXLenght)_renderer.sharedMaterial.mainTextureScale = new Vector2(numOfTopVertices -1f , _renderer.sharedMaterial.mainTextureScale.y) ;
		
		return mMesh;
	}
	
	void tangentSolver(Mesh theMesh, Vector3[] vertices,Vector2[] texcoords,int[] triangles )
	{
		int vertexCount = theMesh.vertexCount;
		//		Vector3[] vertices = theMesh.vertices;
		Vector3[] normals = theMesh.normals;
		//		Vector2[] texcoords = theMesh.uv;
		//		int[] triangles = theMesh.triangles;
		int triangleCount = triangles.Length / 3;
		Vector4[] tangents = new Vector4[vertexCount];
		Vector3[] tan1 = new Vector3[vertexCount];
		Vector3[] tan2 = new Vector3[vertexCount];
		int tri = 0;
		for (int i = 0; i < (triangleCount); i++)
		{
			int i1 = triangles[tri];
			int i2 = triangles[tri + 1];
			int i3 = triangles[tri + 2];
			
			Vector3 v1 = vertices[i1];
			Vector3 v2 = vertices[i2];
			Vector3 v3 = vertices[i3];
			
			Vector2 w1 = texcoords[i1];
			Vector2 w2 = texcoords[i2];
			Vector2 w3 = texcoords[i3];
			
			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;
			
			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;
			
			float r = 1.0f / (s1 * t2 - s2 * t1);
			Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
			
			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;
			
			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
			
			tri += 3;
		}
		
		for (int i = 0; i < (vertexCount); i++)
		{
			Vector3 n = normals[i];
			Vector3 t = tan1[i];
			
			// Gram-Schmidt orthogonalize
			Vector3.OrthoNormalize(ref n, ref t);
			
			tangents[i].x = t.x;
			tangents[i].y = t.y;
			tangents[i].z = t.z;
			
			// Calculate handedness
			tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0) ? -1.0f : 1.0f;
		}
		theMesh.tangents = tangents;
	}
	
	#if UNITY_EDITOR
	/*
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		foreach(Vector3 v in _vertices)
		{
			Gizmos.DrawSphere(v + transform.position, .03f);
		}
	}
	*/
	void reCalculate ()
	{
		if(GetComponent<MeshFilter>() == null ) _meshFilter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
		if(GetComponent<MeshRenderer>() == null ) _renderer = gameObject.AddComponent<MeshRenderer>();
		
		// total vertice count 
		_verticeCount = numOfTopVertices * 2; 
		if(trueLenght) length = numOfTopVertices -1;
		
		// set up vertice array 
		_vertices = new Vector3[_verticeCount];
		// set up uv s array
		_UVs = new Vector2[_verticeCount];
		// set up tris array
		_tris = new int[(( numOfTopVertices - 1) * 6)];
		
		_trisCounter = 0;
		
		// Get collider data
		if(colliderOptions.isCollidable)
		{
			if(colliderOptions.colliderType.Equals(ColliderOptions.ColliderType.Polygon))
			{
				if(GetComponent<PolygonCollider2D>() == null) _polyCollider= gameObject.AddComponent<PolygonCollider2D>() as PolygonCollider2D;
				_polyCollider = GetComponent<PolygonCollider2D>();
			}
			else 
			{
				if(GetComponent<MeshCollider>() == null) _meshCollider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
				_meshCollider = GetComponent<MeshCollider>();
			}
			
		}
	}
	#endif
	
}
[System.Serializable]
public class OrientationOptions
{
	public enum Orientation{
		FaceDown, FaceSide
	}
	public Orientation orientation = Orientation.FaceSide;
	public float depth = 5f;
}
[System.Serializable]
public class ColliderOptions
{
	public enum ColliderType
	{
		Polygon, Mesh3D
	}
	public bool isCollidable = false;
	public ColliderType colliderType = ColliderType.Mesh3D;
	
}
