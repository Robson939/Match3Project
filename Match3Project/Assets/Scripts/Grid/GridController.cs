using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

public class GridController : GridLayoutGroup
{
    [SerializeField] private GameObject tileFramePrefab;
    [SerializeField] private int fixedSideCount;

    public new void OnEnable()
    {
        base.OnEnable();

        GameEvents.OnGetGrid += GetGrid;
    }
    public new void OnDisable()
    {
        base.OnDisable();

        GameEvents.OnGetGrid -= GetGrid;
    }


    private TileFrame[,] GetGrid()
    {
        TileFrame[,] tileFrames = new TileFrame[fixedSideCount, fixedSideCount];

        TileFrame[] tiles = transform.GetComponentsInChildren<TileFrame>();
        
        int i = 0;

        try
        {
            for (int y = 0; y < fixedSideCount; y++)
            {
                for (int x=0; x < fixedSideCount; x++)
                {
                    tileFrames[x,y] = tiles[i];
                    i++;
                }
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }

        return tileFrames;
    }




}
#if UNITY_EDITOR

[CustomEditor(typeof(GridController))]
public class GridControllerEditor : Editor
{
    private SerializedProperty tileFramePrefab;
    private SerializedProperty fixedSideCount;
    private GridController myScript;

    private void OnEnable()
    {
        tileFramePrefab = serializedObject.FindProperty("tileFramePrefab");
        fixedSideCount = serializedObject.FindProperty("fixedSideCount");
    }

    public override void OnInspectorGUI()
    {
        myScript = (GridController)target;
        serializedObject.Update();
        
        fixedSideCount.intValue = 
            EditorGUILayout.IntField("Fixed Side Count", fixedSideCount.intValue);
        tileFramePrefab.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Tile Frame Prefab", tileFramePrefab.objectReferenceValue, typeof(GameObject), false);
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Grid"))
        {
            GenerateGrid(myScript);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void GenerateGrid(GridController myScript)
    {
        float sideSize = myScript.GetComponent<RectTransform>().rect.width / fixedSideCount.intValue;
        myScript.cellSize = new Vector2(sideSize, sideSize);

        //todo: object pooling
        foreach (TileFrame child in myScript.transform.GetComponentsInChildren<TileFrame>())
        {
            DestroyImmediate(child.gameObject);
        }

        byte x = 0;
        byte y = 0;

        for (int i = 0; i < fixedSideCount.intValue * fixedSideCount.intValue; i++)
        {
            GameObject gameObj = (GameObject)Instantiate(tileFramePrefab.objectReferenceValue, myScript.transform);

            gameObj.name = $"TileFrame[{x},{y}]";
            gameObj.GetComponent<TileFrame>().pos = new GridPosition(x, y);

            x++;
            if (x >= fixedSideCount.intValue)
            {
                y++;
                x = 0;
            }
           
            //todo: save grid to SO_level
        }
    }
}
#endif