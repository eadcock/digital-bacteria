using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using quiet;
using System.Linq;

#if UNITY_EDITOR
[CustomEditor(typeof(CreatePlantMap))]
public class CreatePlantMapDrawer : Editor
{
    private void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CreatePlantMap cpm = target as CreatePlantMap;

        if (GUILayout.Button("Regenerate Texture"))
        {
            cpm.GeneratePlantMap();
            cpm.GenerateCreatureMap();
        }

        if(GUILayout.Button("Regenerate Grid"))
        {
            cpm.GeneratePlantGrid();
        }

        if(GUILayout.Button("Display Probability Grid"))
        {
            cpm.DisplayProbabilityGrid();
        }

        if(GUILayout.Button("Hide Probability Grid"))
        {
            cpm.HideProbabilityGrid();
        }

        if (GUILayout.Button("Plant Map"))
        {
            cpm.TogglePlantMap();
        }

        if (GUILayout.Button("Creature Map"))
        {
            cpm.ToggleCreatureMap();
        }
    }
}
#endif


[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Grid))]
public class CreatePlantMap : MonoBehaviour
{
#nullable enable
    public Sprite? plantMap;
    public Sprite? creatureMap;

    public SpriteRenderer spriteRenderer;

    public Grid grid;

    protected byte[,] plantProbabilityGrid;
    protected byte[,] creatureProbGrid;

    /// Probability Grid Display Texture
    private Sprite? pgdt;

    public byte[,] PlantProbabilityGrid
    {
        get
        {
            if(plantProbabilityGrid == null)
            {
                GeneratePlantProbabilityGrid();
            }

            return plantProbabilityGrid;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        grid = GetComponent<Grid>();

        if(plantMap == null)
        {
            GeneratePlantMap();
            GeneratePlantGrid();
            GeneratePlantProbabilityGrid();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GeneratePlantMap()
    {
        Texture2D raw = Resources.Load<Texture2D>("plant_output");

        plantMap = Sprite.Create(raw, new Rect(0, 0, raw.width, raw.height), new Vector2(0, 0));

        spriteRenderer.sprite = plantMap;
    }

    public void TogglePlantMap()
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = plantMap;
    }

    public void GenerateCreatureMap()
    {
        Texture2D raw = Resources.Load<Texture2D>("creature_output");

        creatureMap = Sprite.Create(raw, new Rect(0, 0, raw.width, raw.height), new Vector2(0, 0));

        spriteRenderer.sprite = creatureMap;
    }

    public void ToggleCreatureMap()
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = creatureMap;
    }

    public void HideMap()
    {
        spriteRenderer.enabled = false;
    }

    public void GeneratePlantGrid()
    {
        if (plantMap == null) GeneratePlantMap();

        Debug.Assert(plantMap != null);

#pragma warning disable CS8602 // it won't ever be null here, but thanks for the heads up
        Texture2D raw = plantMap.texture;
#pragma warning restore CS8602

        Vector2 resolution = raw.texelSize;

        grid.cellSize = resolution;
    }

    public void GeneratePlantProbabilityGrid()
    {
        if (plantMap == null) GeneratePlantMap();

        Debug.Assert(plantMap != null);

        (int width, int height) = (plantMap == null ? 0 : plantMap.texture.width, plantMap == null ? 0 : plantMap.texture.height);

        plantProbabilityGrid = new byte[width, height];

#pragma warning disable CS8602 // it won't ever be null here, but thanks for the heads up
        Color32[] pixel = plantMap.texture.GetPixels32();
#pragma warning restore CS8602

        for (int i = 0; i < plantProbabilityGrid.GetLength(1); i++)
        {
            for(int j = 0; j < plantProbabilityGrid.GetLength(0); j++)
            {
                var p = pixel[i * plantProbabilityGrid.GetLength(1) + j];
                plantProbabilityGrid[j, i] = (byte)(p.r + p.b + p.g);
            }
        }
    }

    public void DisplayProbabilityGrid()
    {
        if(pgdt == null)
        {
            Texture2D texture = new(plantMap.texture.width, plantMap.texture.height);

            pgdt = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            Color32[] pgdtC = plantProbabilityGrid.Flatten().Select(p => new Color32(p, p, p, 1)).ToArray();
            pgdt.texture.SetPixels32(pgdtC);
        }

        spriteRenderer.sprite = pgdt;
    }

    public void HideProbabilityGrid()
    {
        spriteRenderer.sprite = plantMap;
    }

    public GameObject CreatePlaceholder()
    {
        return CreatePlaceholder(Vector3.zero);
    }

    public GameObject CreatePlaceholder(Vector3 pos)
    {
        GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholder.transform.SetParent(transform);
        placeholder.transform.position = pos;

        return placeholder;
    }
}
