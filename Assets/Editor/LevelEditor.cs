using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum PlatformType
{
    Grass,
    Glass,
    Normal,
    None,
}

public enum PickableType
{
    Coin,
    Heart,
    None,
}

public class LevelEditor : EditorWindow 
{
    public static int levelNumber; 

    private int gridSize;
    private Vector3 spawnObjectPosition = new Vector3(0f, 0.5f, 0f);
    private string saveLevelPath = "Assets/Prefabs/Level/";

    public PlatformType platformType;
    public PickableType pickableType;

    #region Prefabs
    private GameObject platformGrassPrefab;
    private GameObject platformGlassPrefab;
    private GameObject platformNormalPrefab;
    private GameObject coinPrefab;
    private GameObject heartPrefab;
    #endregion

    #region Textures
    private Texture heartTexture;
    private Texture coinTexture;
    private Texture defaultTexture;

    private Texture grassPlatformTexture;
    private Texture glassPlatformTexture;
    private Texture normalPlatformTexture;
    private Texture defaultPlatformTexture;

    #endregion

    private Dictionary<int, PlatformInfo> dictionary = new Dictionary<int, PlatformInfo>();
    private bool isDeleteOn;

    
    private void Awake() 
    {
        Debug.Log("Loading Assets");

        #region Loading Prefabs
        platformGrassPrefab = Resources.Load<GameObject>("PlatformGrass");
        platformGlassPrefab = Resources.Load<GameObject>("PlatformGlass");
        platformNormalPrefab = Resources.Load<GameObject>("PlatformNormal");
        heartPrefab = Resources.Load<GameObject>("Heart");
        coinPrefab = Resources.Load<GameObject>("Coin");
        #endregion

        #region Loading Textures
        heartTexture = Resources.Load<Texture>("Sprites/Heart");
        coinTexture = Resources.Load<Texture>("Sprites/Coin");
        defaultTexture = Resources.Load<Texture>("Sprites/White1x1");

        grassPlatformTexture = Resources.Load<Texture>("Sprites/grass");
        glassPlatformTexture = Resources.Load<Texture>("Sprites/glass");
        normalPlatformTexture = Resources.Load<Texture>("Sprites/NormalPlatform");
        defaultPlatformTexture = Resources.Load<Texture>("Sprites/White1x1");

        #endregion

        gridSize = 5;
    }

    [MenuItem("Window/Custom Windows/LevelEditor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditor>("Level Generator");
    }

    private void OnGUI() 
    {
        //Prefab to spawn
        /*        platformGrassPrefab = (GameObject) EditorGUILayout.ObjectField("Choose Grass Platform Prefab", platformGrassPrefab, typeof(GameObject), false); 
                platformGlassPrefab = (GameObject) EditorGUILayout.ObjectField("Choose Glass Platform Prefab", platformGlassPrefab, typeof(GameObject), false); 
                platformNormalPrefab = (GameObject) EditorGUILayout.ObjectField("Choose Normal Platform Prefab", platformNormalPrefab, typeof(GameObject), false); 

                coinPrefab = (GameObject) EditorGUILayout.ObjectField("Choose Coin Prefab", coinPrefab, typeof(GameObject), false); 
                heartPrefab = (GameObject) EditorGUILayout.ObjectField("Choose Heart Prefab", heartPrefab, typeof(GameObject), false );
        */

        gridSize = EditorGUILayout.IntField("Enter Grid Size" ,gridSize);

        saveLevelPath = EditorGUILayout.TextField("Path To Save Level", saveLevelPath);

        GUI.color = Color.yellow;
        //Spawn Level Button
        if (GUILayout.Button("Spawn Level"))
        {
            if(dictionary.Count > 0) SpawnSelected();
            else Debug.LogError("Error choose some button on a grid first!");
            
        }

        GUI.color = Color.white;
        GUI.color = Color.red;

        //Prints the grid cells we clicked
        if(GUILayout.Button("Debug Clicked Platforms"))
        {
            foreach(KeyValuePair<int, PlatformInfo> kvp in dictionary)
            {
                Debug.Log(kvp.Value.ToString());
            }
        }

        GUI.color = Color.white;

        GUI.color = Color.green;

        //Clears out the dictionary
        if(GUILayout.Button("Reset Grid"))
        {
            dictionary.Clear();
        }

        GUI.color = Color.white;

        platformType = (PlatformType) EditorGUILayout.EnumPopup("Choose Platform", platformType);
        pickableType = (PickableType) EditorGUILayout.EnumPopup("Choose Pickable", pickableType);

        isDeleteOn = GUILayout.Toggle(isDeleteOn, "Remove");

        //Creates the grid
        CreateGrid(gridSize, 25, 25, 50, 200, "Level Grid", 25);

        if (GUILayout.Button("Testing popup"))
        {
            isPlatformClick = !isPlatformClick;
        }

        if (isPlatformClick)
        {
            Debug.Log(previousSelectedPlatform);

            //We selected something new
            if (previousSelectedPlatform != selectedPlatformIndex)
            {
                previousSelectedPlatform = selectedPlatform;
                //ChangePlatformType();
                isPlatformClick = false;
            }
            else
            {
                selectedPlatformIndex = EditorGUI.Popup(new Rect(50, 50, 100, 50), selectedPlatformIndex, platformStrings);
            }

        }

           
        //i* 4 + j for number in row in grid
    }

    private bool isPlatformClick = false;
    private bool isPickableClick = false;

    private int selectedPlatformIndex;
    private int previousSelectedPlatform = 3;

    //private PlatformType
    private string[] platformStrings = new string[4] { "Grass", "Glass", "Normal", "None" };
    private string[] pickableStrings = new string[3] { "Coin", "Heart", "None" };

    private int selectedPlatform;

    //private void ChangePlatformType()
    //{
    //    dictionary[selectedPlatform].ChangePlatform(PlatformType.)
    //}

    private void CreateGrid(int gridSize, float buttonSize, float spaceBetween, float posX, float posY, string boxName, float boxOffset)
    {
        float initialPosY = posY;
        float initialPosX = posX;
        float boxSize2 = 10+ buttonSize*3 ; // *2

        float boxSize = gridSize * boxSize2 + (gridSize - 1) * spaceBetween/2;

        int buttonIndex = 1;
        GUI.Box(new Rect(posX - boxOffset, posY - boxOffset, boxSize + boxOffset*2, boxSize + boxOffset*2), boxName);

        //GUI.Button(new Rect(initialPosX+ 5, initialPosY+5, buttonSize, buttonSize), "Asd");

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GUI.Box(new Rect(initialPosX, initialPosY , boxSize2, boxSize2), "Platform " + buttonIndex);

                //Pickable Button
                if (dictionary.ContainsKey(buttonIndex))
                {
                    switch (dictionary[buttonIndex].GetPickableType())
                    {
                        case PickableType.Heart:
                            if (GUI.Button(new Rect(initialPosX, initialPosY + boxSize2 / 2 - 20, buttonSize*3, buttonSize), heartTexture))
                            {
                                if (dictionary.ContainsKey(buttonIndex))
                                {
                                    dictionary[buttonIndex].ChangePickable(pickableType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, PlatformType.None, pickableType);

                                }
                            }
                            break;
                        case PickableType.Coin:
                            if (GUI.Button(new Rect(initialPosX, initialPosY + boxSize2 / 2 - 20, buttonSize*3, buttonSize), coinTexture))
                            {
                                if (dictionary.ContainsKey(buttonIndex))
                                {
                                    dictionary[buttonIndex].ChangePickable(pickableType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, PlatformType.None, pickableType);

                                }
                            }
                            break;
                        case PickableType.None:
                            if (GUI.Button(new Rect(initialPosX, initialPosY + boxSize2 / 2 - 20, buttonSize * 3, buttonSize), defaultTexture))
                            {
                                if (dictionary.ContainsKey(buttonIndex))
                                {
                                    dictionary[buttonIndex].ChangePickable(pickableType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, PlatformType.None, pickableType);

                                }
                            }
                            break;
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(initialPosX , initialPosY + boxSize2 / 2 - 20, buttonSize*3, buttonSize), defaultTexture))
                    {
                        if (dictionary.ContainsKey(buttonIndex))
                        {
                            dictionary[buttonIndex].ChangePickable(pickableType);
                        }
                        else
                        {
                            dictionary[buttonIndex] = new PlatformInfo(i, j, PlatformType.None, pickableType);

                        }
                    }
                }

                //Platform Button
                if(dictionary.ContainsKey(buttonIndex))
                {
                    switch (dictionary[buttonIndex].GetPlatformType())
                    {
                        case PlatformType.Grass:
                            if (GUI.Button(new Rect(initialPosX + boxSize2 / 2 - 41, initialPosY + boxSize2 / 2 + 5, buttonSize * 3, buttonSize), grassPlatformTexture))
                            {
                                if (dictionary[buttonIndex].GetPlatformType() != PlatformType.None)
                                {
                                    dictionary[buttonIndex].ChangePlatform(platformType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);
                                }
                            }
                            break;
                        case PlatformType.Glass:
                            if (GUI.Button(new Rect(initialPosX + boxSize2 / 2 - 41, initialPosY + boxSize2 / 2 + 5, buttonSize * 3, buttonSize), glassPlatformTexture))
                            {
                                if (dictionary[buttonIndex].GetPlatformType() != PlatformType.None)
                                {
                                    dictionary[buttonIndex].ChangePlatform(platformType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);

                                }
                            }
                            break;
                        case PlatformType.Normal:
                            if (GUI.Button(new Rect(initialPosX + boxSize2 / 2 - 41, initialPosY + boxSize2 / 2 + 5, buttonSize * 3, buttonSize), normalPlatformTexture))
                            {
                                if (dictionary[buttonIndex].GetPlatformType() != PlatformType.None) //dictionary.ContainsKey(buttonIndex)
                                {
                                    dictionary[buttonIndex].ChangePlatform(platformType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);

                                }
                            }
                            break;
                        case PlatformType.None:
                            if (GUI.Button(new Rect(initialPosX + boxSize2 / 2 - 41, initialPosY + boxSize2 / 2 + 5, buttonSize * 3, buttonSize), defaultPlatformTexture))
                            {
                                if (dictionary.ContainsKey(buttonIndex))
                                {
                                    dictionary[buttonIndex].ChangePlatform(platformType);
                                }
                                else
                                {
                                    dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);

                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (GUI.Button(new Rect(initialPosX + boxSize2 / 2 - 41, initialPosY + boxSize2 / 2 + 5, buttonSize * 3, buttonSize), defaultPlatformTexture))
                    {
                        if (dictionary.ContainsKey(buttonIndex))
                        {
                            dictionary[buttonIndex].ChangePlatform(platformType);
                        }
                        else
                        {
                            dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);

                        }
                        //dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);
                    }
                }

                //Platform Button
                //if (GUI.Button(new Rect(initialPosX + boxSize2 /2 - 41, initialPosY + boxSize2/2 + 5, buttonSize*3, buttonSize), "2"))
                //{
                //    if(dictionary.ContainsKey(buttonIndex))
                //    {
                //        dictionary[buttonIndex].ChangePlatform(platformType);
                //    }
                //    else
                //    {
                //        dictionary[buttonIndex] = new PlatformInfo(i, j, platformType, PickableType.None);
                //    }
                //}
                
                initialPosX += buttonSize + spaceBetween + boxSize2 / 2;
                buttonIndex++;
            }
            initialPosY += buttonSize + spaceBetween + boxSize2 / 2 ;
            initialPosX = posX;
        }
        GUI.color = Color.white;

    }

    private GameObject GetPrefab(PlatformType type)
    {
        switch (type)
        {
            case PlatformType.Grass: 
                return platformGrassPrefab;

            case PlatformType.Glass: 
                return platformGlassPrefab;

            case PlatformType.Normal: 
                return platformNormalPrefab;

            default: 
            return null;
        }
    }

    private GameObject GetPickable(PickableType type)
    {
        switch(type)
        {
            case PickableType.Coin: return coinPrefab;
            case PickableType.Heart: return heartPrefab;
            default: return null;
        }
    }

    private void SpawnSelected()
    {
        levelNumber++;
        GameObject parentObject = new GameObject();
        parentObject.name = "Level " + levelNumber;

        parentObject.transform.position = Vector3.zero;

        foreach(KeyValuePair<int, PlatformInfo> kvp in dictionary)
        {

            GameObject pickable = GetPickable(kvp.Value.GetPickableType());
            //PrefabUtility.UnpackPrefabInstance(GetPrefab(kvp.Value.GetPlatformType()), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            GameObject platform = GetPrefab(kvp.Value.GetPlatformType());

            //PrefabUtility.UnpackPrefabInstance(platform, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            if(platform != null) Instantiate(platform, kvp.Value.GetPosition(false), Quaternion.identity, parentObject.transform);
            if(pickable != null)
            {
                    Instantiate(pickable, kvp.Value.GetPosition(true), Quaternion.identity, parentObject.transform);
            } 
          
        }

        PrefabUtility.SaveAsPrefabAsset(parentObject, saveLevelPath + parentObject.name+ ".prefab");
        GameObject.DestroyImmediate(parentObject);
    }

}
public class PlatformInfo
{
    private int posX;
    private int posY;
    private PlatformType platformType;

    private PickableType pickableType;  

    public PlatformInfo( int posX, int posY, PlatformType platformType, PickableType pickableType)
    {
        this.posX = posY;
        this.posY = posX;
        this.platformType = platformType;
        this.pickableType = pickableType;
    }

    public void ChangePlatform(PlatformType newPlatformType)
    {
        this.platformType = newPlatformType;
    }

    public void ChangePickable(PickableType newPickableType)
    {
        this.pickableType = newPickableType;
    }

    public PlatformType GetPlatformType()
    {
        return platformType;
    }
    public PickableType GetPickableType()
    {
        return pickableType;
    }

    public Vector3 GetPosition(bool isPickable)
    {
        if(!isPickable) 
        {
            return new Vector3(posX, -posY, 0);

        }
        else 
        {
            return new Vector3(posX, -posY + 0.5f, 0);
            
        }
    }

    public override string ToString()
    {
        return "( " + posX  + "," + posY + " ) Is Type : " + platformType.ToString() + " and Has picklable: " + pickableType.ToString();
    }
}