//***************************************************************/
// Editor tool  is defined here
//**************************************************************/

using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class CustomEditorCaveGenerator : EditorWindow
{
	//initialise editor window object
	static EditorWindow e_window = null;

	//get 2d map length and breadth
    public int e_mapLength = 50;
    public int e_mapBreadth = 50;

	//storing only open space of each generated map
	private int[] x;
	private int[] y;

	//AI spawn
	public int[,] aiSafeSpawn;

	//seed values to randomise patterns
    public string e_seedValue = "rawdata";
    public bool useRandomSeed;

	//Editor layout panel width
	private int editorLayoutWidth = 125;

	//reference to gameobjects
	private GameObject e_gameobject;

	//reference to each spawned mapobjects in map
	public GameObject[] mapItems;

	//texture for minimap
	private RenderTexture e_renderTexture;

	//create reference
	public MapCreator e_mapcreator;
    private MeshGenerator m_meshgenerator;

	//prefab name
    private string savePrefabname;

	//assign suitable extensions
	private string prefabExtension = ".prefab";
	private string savePath = "Assets/CaveGenerator/Resources/";
	private string assetExtension  = ".asset";

	//used to toggle gui section
	private bool isActive;
	private bool isdisabled = false;

	Vector2 scrollPosition = Vector2.zero;

	//add to unity menu
	[MenuItem("Cave Generator/Generate Cave %g")]

    private static void CreateWindow()
    {
		//if already open , close it
		if (e_window) 
		{
			e_window.Close ();
		}

		e_window = EditorWindow.GetWindow<CustomEditorCaveGenerator>();
        e_window.titleContent.text = "Create Map";
		e_window.minSize = new Vector2(500, 710);
		e_window.maxSize = new Vector2(551, 1500);

    }

	void Awake()
	{
		//if (e_gameobject = GameObject.Find ("Map(Clone)")) 
		//{
		//	e_mapcreator = e_gameobject.GetComponent<MapCreator>();
		//	m_meshgenerator = e_gameobject.GetComponent<MeshGenerator>();
		//}
	}
		

    void OnGUI()
    {
		if (e_gameobject = GameObject.Find ("Map(Clone)")) 
		{
			isActive = false;
		} 
		else 
		{
			isActive = true;
		}

		//scrollbar
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true,  GUILayout.Width(Screen.width),  GUILayout.Height(Screen.height));
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.LabelField("Enter Map Size",EditorStyles.boldLabel); 
		EditorGUILayout.Space ();
		EditorGUILayout.HelpBox ( "Values above 150 is resource intensive and not recommended !", MessageType.Warning,true );
		EditorGUILayout.Space ();

		//map size input region
        EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Map Width : ", EditorStyles.largeLabel, GUILayout.Width(editorLayoutWidth), GUILayout.Height(25));
        e_mapLength = EditorGUILayout.IntField(e_mapLength, GUILayout.Width(editorLayoutWidth));

		EditorGUILayout.Space ();

		EditorGUILayout.LabelField("Map Height : ", EditorStyles.largeLabel, GUILayout.Width(editorLayoutWidth),GUILayout.Height(25));
        e_mapBreadth = EditorGUILayout.IntField(e_mapBreadth, GUILayout.Width(editorLayoutWidth));   

		EditorGUILayout.Space ();
        EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});
		//end map size input region

		//Seed input region
		EditorGUILayout.LabelField("Enter Seed Value",EditorStyles.boldLabel); 
		EditorGUILayout.Space ();

        EditorGUILayout.BeginHorizontal();
        useRandomSeed = GUILayout.Toggle(useRandomSeed, "Generate Random Seed", EditorStyles.radioButton, GUILayout.Width(155));   
        EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space ();

        if (!useRandomSeed)
        {
            EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Seed Value : ", EditorStyles.largeLabel, GUILayout.Width(editorLayoutWidth),GUILayout.Height(25));
			e_seedValue = EditorGUILayout.TextField(e_seedValue, GUILayout.Width(editorLayoutWidth));
            EditorGUILayout.EndHorizontal();
        }

		EditorGUILayout.Space();
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});
		EditorGUILayout.Space();
		//end seed input region

		GUI.color = Color.green;
        EditorGUILayout.BeginHorizontal();

		//Button "Generate Map" start
        if (GUILayout.Button("Generate Map", EditorStyles.miniButton))
        {
			//mapItems
			mapItems = GameObject.FindGameObjectsWithTag("Respawn");

			//remove any existing game objects
			if (mapItems != null)
			{
				foreach (GameObject mapItem in mapItems)
				{
					DestroyImmediate(mapItem);
				}
			}


			if (e_gameobject != null) 
			{
				DestroyImmediate (e_gameobject);
			}

			//connection
            e_gameobject =  Instantiate(Resources.Load("Map") as GameObject);
			AssignLayer(e_gameobject, 13);
			e_mapcreator = e_gameobject.GetComponent<MapCreator>();
			m_meshgenerator = e_gameobject.GetComponent<MeshGenerator>();

            if (e_mapcreator != null)
            {
				e_mapcreator.mapLength = e_mapLength;
                e_mapcreator.mapBreadth = e_mapBreadth;
                e_mapcreator.m_bUseRandomSeed = useRandomSeed;

                if (!useRandomSeed)
                {
                    e_mapcreator.m_strSeed = e_seedValue;
                }
                e_mapcreator.GenerateMap();    
            }         

	
        }

		GUI.color = Color.white;
        if (e_mapcreator != null)
        {
            EditorUtility.SetDirty(e_mapcreator);
        }

        
        EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
	
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});
		//Button "Generate Map" end

			EditorGUI.BeginDisabledGroup (isActive == true);

			//Save to folder region 
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Save to folder",EditorStyles.boldLabel); 
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox ("Enter a name and save before adding elements !!\nUsing same name would overwrite the existing prefab\nSave path : Assets/CaveGenerator/Resources/ <given name>", MessageType.Warning,true );
			EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Enter prefab name",EditorStyles.label,GUILayout.Width(150)); 
			savePrefabname = EditorGUILayout.TextField(savePrefabname, GUILayout.Width(editorLayoutWidth));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			GUI.color = Color.yellow;
            if (GUILayout.Button("Save as Prefab", EditorStyles.miniButton))
            {
				if (m_meshgenerator != null) 
				{
					if (!Directory.Exists (savePath + savePrefabname )) 
					{
						//create folder
						Directory.CreateDirectory (savePath + savePrefabname );
						AssetDatabase.Refresh ();
					}
				
					if (Directory.Exists (savePath + savePrefabname))
					{
						SaveMesh (e_gameobject, m_meshgenerator.m_Walls.sharedMesh, "_wall");
						SaveMesh (e_gameobject, m_meshgenerator.m_Cave.sharedMesh, "_cave");
				
					}
				}	
				//combine assets and create prefab
				CreatePrefab ();		
           
             }
			AssetDatabase.Refresh();
            EditorGUILayout.EndHorizontal();

			GUI.color = Color.white;
			EditorGUILayout.Space();
			GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});
			EditorGUILayout.Space();
			//end save to folder region

			//Button "add items to map" region
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Add Items To Map",EditorStyles.boldLabel); 
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox ( "It will add item randomly on the explorable part of the cave.\nOn map regenerate the items are deleted.\nUse this to again populate the map.", MessageType.Warning,true );
			EditorGUILayout.Space();
			
			GUI.color = Color.HSVToRGB(0.2f,0.5f,0.7f);
			if (GUILayout.Button ("Add items", EditorStyles.miniButton)) 
			{
				SpawnItems ();
			}
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			GUI.color = Color.white;
			//end Button "add items to map" region

			EditorGUI.EndDisabledGroup ();
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});

		//AI Controlled Character region
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("AI Controlled Character ",EditorStyles.boldLabel); 

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox ( "You can add more than one AI characters.\nSpawn could be anywhere in generated open area.\nAI will take control in Play Mode.\nExisting AI Characters will be deleted on new map generations !", MessageType.Warning,true );

		GUI.color = Color.cyan;
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button ("Import AI Character", EditorStyles.miniButton)) 
		{
			// add local vavigation mesh builder to scene
			AddLocalNavMeshBuilder ();
			// add AI character to scene
			AddAI ();
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		GUI.color = Color.white;
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});
		//End AI Controlled Character region

		//Import player character
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Player Controlled Character ",EditorStyles.boldLabel); 
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.HelpBox ( "Adds player and camera which follows the player.\nIt will disable all other cameras in scene !\nPlease move them to open spots once spawned !", MessageType.Warning,true );

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.gray;
		if (GUILayout.Button ("Import Player Controls", EditorStyles.miniButton)) 
		{
			AddPlayer ();
			AddPlayerCamera ();
		}
		GUI.color = Color.white;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(3)});
		//End import player character

		//Mini-map Display region
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Mini-Map Display ",EditorStyles.boldLabel); 
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.HelpBox ( "Adds Canvas UI and other HUD elements.\nIt will use unity's layers 12 and 13 !\nDont assign these layers to any other gameobjects.", MessageType.Warning,true );
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.magenta;
		if (GUILayout.Button ("Import Mini-Map", EditorStyles.miniButton)) 
		{
			// add a preset minimap camera to scene
			AddMiniMapCamera ();
			//Add a canvas UI with minimap functionality
			AddMiniMapCanvas ();
		}
		EditorGUILayout.EndHorizontal();

		GUI.color = Color.white;
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		GUI.color = Color.red;
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(6)});
		GUI.color = Color.white;
		//End Mini-map Display region

		//Help Region
		EditorGUILayout.Space();
		EditorGUILayout.LabelField(" Need Help ? ",EditorStyles.boldLabel); 
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.HelpBox ( "Go through the document to know about the setup of the tool, background works, faced issues, known bugs and future plans etc.!", MessageType.Info,true );
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.green;
		if (GUILayout.Button ("View Document", EditorStyles.miniButton)) 
		{
			Application.OpenURL ((Application.dataPath) + "/CaveGenerator/Documentation/cave_generator_doc.pdf");
		}
		EditorGUILayout.EndHorizontal();

		GUI.color = Color.white;
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		GUI.color = Color.red;
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(6)});

		EditorGUILayout.Space();
		GUI.color = Color.white;
		//End Help Region

		//created by silly me
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		EditorGUI.BeginDisabledGroup ( isdisabled == false);
		EditorGUILayout.LabelField("created by John Dennis Thottam ",EditorStyles.boldLabel); 
		EditorGUI.EndDisabledGroup ();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		//End created by silly me

		//end scrollbar
		GUILayout.EndScrollView();
    }

	//******************Layer Info***********************************/
	//layer 12 is for minimap (only visible in minimap & minimap camera)
	//layer 13 is for level (visible in player and minimap camera)
	//**************************************************************/

	//map item spawner
	public void SpawnItems()
	{
		try
		{
			for (int i = 0; i < e_mapLength; i++) 
			{
				for (int j = 0; j < e_mapBreadth; j++) 
				{
					if (e_mapcreator.safeSpace [i, j] == 0) 
					{
						if (UnityEngine.Random.value > 0.99f) 
						{
							Object prefab = AssetDatabase.LoadAssetAtPath("Assets/CaveGenerator/Prefab/MapItem.prefab", typeof(GameObject));
							GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
							clone.tag= "Respawn";
							GameObject childObject = clone.transform.GetChild (0).gameObject;
							AssignLayer (childObject, 12);
							clone.transform.position = new Vector3(i - (e_mapLength / 2), -3, j - (e_mapBreadth/ 2));
						}
					}
				}
			}
		}
		catch
		{
			Debug.Log(" Failed ! Please generate map again !");
		}
	}

	//assign layer to gameobjects
	public void AssignLayer(GameObject gobject, int layerNumber)
	{
		gobject.layer = layerNumber;

		foreach (Transform trans in gobject.GetComponentsInChildren<Transform>(true))
		{
			trans.gameObject.layer = layerNumber;
		}
	}

	//add AI character
	public void AddAI()
	{
		try{
		int[] x = new int[e_mapcreator.mapLength * e_mapcreator.mapBreadth];
		int[] y = new int[e_mapcreator.mapLength * e_mapcreator.mapBreadth];

		int count=0;
		int r;
		for (int i = 0; i < e_mapcreator.mapLength; i++) 
		{
			for (int j = 0; j < e_mapcreator.mapBreadth; j++) 
			{
				if (e_mapcreator.safeSpace [i, j] == 0) 
				{
					x [count] = i;
					y [count] = j;
					count++;
				
				}
			}
		}
			
		r = Random.Range (0, count);
		if (x[r] != 0 && y[r] != 0) 
		{
			Object prefab = AssetDatabase.LoadAssetAtPath ("Assets/CaveGenerator/Prefab/AI_character.prefab", typeof(GameObject));
			GameObject clone = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			clone.tag = "Respawn";
			GameObject childObject = clone.transform.GetChild (0).gameObject;
				AssignLayer (childObject, 12);
			clone.transform.position = new Vector3 (x [r] - (e_mapcreator.mapLength / 2), -3, y [r] - (e_mapcreator.mapBreadth / 2));
		}
	}
	catch
	{
		isActive =! isActive;
		Debug.Log(" Failed ! Please generate map again !");
	}
	}


	//Add navmesh
	private void AddLocalNavMeshBuilder()
	{
		if (!GameObject.Find ("LocalNavMeshBuilder(Clone)")) 
		{
			Object prefab = AssetDatabase.LoadAssetAtPath("Assets/CaveGenerator/Prefab/LocalNavMeshBuilder.prefab", typeof(GameObject));
			GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			clone.transform.position = new Vector3(0,0,0);
		}
	}

	//add player
	private void AddPlayer()
	{
		

		if (!GameObject.Find ("cave_player(Clone)")) 
		{
			Object prefab = AssetDatabase.LoadAssetAtPath("Assets/CaveGenerator/Prefab/cave_player.prefab", typeof(GameObject));
			GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

			GameObject childObject = clone.transform.GetChild (0).gameObject;
			AssignLayer (childObject, 12);

			clone.transform.position = new Vector3(0,-3,0);

		}
	}

	//add player follow camera
	private void AddPlayerCamera()
	{
		if (!GameObject.Find ("camera_cave_player(Clone)")) 
		{
			Object prefab = AssetDatabase.LoadAssetAtPath("Assets/CaveGenerator/Prefab/camera_cave_player.prefab", typeof(GameObject));
			GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			clone.transform.position = new Vector3(-1,6,-5);
			clone.transform.rotation = Quaternion.Euler(50,1,0);
		}
	}

	//Add minimap UI canvas
	private void AddMiniMapCanvas()
	{
		if (!GameObject.Find ("minimap_canvas(Clone)")) 
		{
			Object prefab = AssetDatabase.LoadAssetAtPath("Assets/CaveGenerator/Prefab/minimap_canvas.prefab", typeof(GameObject));
			GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

			GameObject childObject = clone.transform.GetChild (0).gameObject;
			GameObject childObject1 = childObject.transform.GetChild (0).gameObject;

			childObject1.GetComponent<RawImage>().texture = e_renderTexture;
		}
	}

	//Add minimap camera
	private void AddMiniMapCamera()
	{
		if (!System.IO.File.Exists("Assets/CaveGenerator/Materials/rt.renderTexture"))
		{
			RenderTexture rt = new RenderTexture(256, 256, 24);
			AssetDatabase.CreateAsset(rt, "Assets/CaveGenerator/Resources/rt.renderTexture");
			AssetDatabase.Refresh ();
		}

		e_renderTexture =  Instantiate(Resources.Load("rt") as RenderTexture);

		if (!GameObject.Find ("MiniMapCamera(Clone)")) 
		{
			Object prefab = AssetDatabase.LoadAssetAtPath("Assets/CaveGenerator/Prefab/MiniMapCamera.prefab", typeof(GameObject));
			GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

			clone.GetComponent<Camera>().cullingMask = 1 << 12 | 1 << 13;

			clone.GetComponent<Camera>().targetTexture = e_renderTexture;
			clone.transform.position = new Vector3(0,50,0);
			clone.transform.rotation = Quaternion.Euler(90,0,0);
		}
	}

	//create prefab
    private void CreatePrefab()
    {
		try{
        if (e_gameobject != null)
        {
            string path = savePath + savePrefabname + "/" + savePrefabname + prefabExtension;
            Debug.Log(path);
            PrefabUtility.CreatePrefab(path, e_gameobject, ReplacePrefabOptions.Default);
			//e_gameobject.layer = 13;
			AssignLayer (e_gameobject, 13);
            AssetDatabase.Refresh();
			//Directory.GetFiles()
			}
		}
		catch{
			Debug.Log ("Enter a valid name !");	
		}
    }



    public void SaveMesh(GameObject gobject, Mesh mesh, string name)
    {    
		//only run in unity edior
#if UNITY_EDITOR
		string path = savePath + savePrefabname + "/" + savePrefabname + name + assetExtension;        
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

}
//private string prefabExtension = ".prefab";
//private string savePath = "Assets/CaveGenerator/Resources/";
//private string assetExtension  = ".asset";