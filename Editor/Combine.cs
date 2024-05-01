// Copyright © Connor deBoer 2024, All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Combine
{
    private MeshFilter[] _filters;
    private MeshRenderer[] _renderer;

    private string _path;
    private List<GameObject> _gameObjects;

    private Mesh _combinedMesh;

    public Combine(string path, List<GameObject> gameObjects)
    {
        _path = path;
        _gameObjects = gameObjects;
        _filters = gameObjects.Select(element => element.GetComponent<MeshFilter>()).ToArray();
        _renderer = gameObjects.Select(element => element.GetComponent<MeshRenderer>()).ToArray();
    }

    public void CombineMesh()
    {
        CombineInstance[] combine = new CombineInstance[_filters.Length];

        for (int i = 0; i < _gameObjects.Count; ++i)
        {
            combine[i].mesh = _filters[i].sharedMesh;
            combine[i].transform = _filters[i].transform.localToWorldMatrix;
        }

        _combinedMesh = new Mesh();
        _combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _combinedMesh.CombineMeshes(combine);

        SaveAssets.SaveFile($"{_path}_Mesh.asset", _combinedMesh);
    }

    public void CombineTexture()
    {
        int width = 0;
        int height = 0;
        HashSet<Texture2D> baseMaps = new HashSet<Texture2D>();

        for (int i = 0; i < _renderer.Length; ++i)
        {
            Texture2D texture = (Texture2D)_renderer[i].sharedMaterial.mainTexture;
            baseMaps.Add(texture);
        }

        width = baseMaps.Sum(texture => texture.width);
        height = baseMaps.Sum(texture => texture.height);

        Texture2D combinedBaseMap = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Rect[] uvCoordinates = combinedBaseMap.PackTextures(baseMaps.ToArray(), 0, width);

        SaveAssets.SaveFile($"{_path}_Base.png", combinedBaseMap);

        GenerateMaterials();
        UpdateUV(uvCoordinates, baseMaps);
    }

    private void UpdateUV(Rect[] uvCoordinates, HashSet<Texture2D> baseMaps)
    {
        // Build UV offset table
        Dictionary<Texture2D, Rect> uvOffsetLookupTable = new Dictionary<Texture2D, Rect>();
        for (int i = 0; i < uvCoordinates.Length; ++i)
        {
            uvOffsetLookupTable.Add(baseMaps.ElementAt(i), uvCoordinates[i]);
        }

        // Calculate new UVs
        int uvCount = 0;
        Vector2[] meshUVs = new Vector2[_combinedMesh.vertices.Length];

        Rect offset;
        for (int i = 0; i < _filters.Length; ++i)
        {
            offset = uvOffsetLookupTable[(Texture2D)_renderer[i].sharedMaterial.mainTexture];
            for (int j = 0; j < _filters[i].sharedMesh.vertices.Length; ++j)
            {
                float x = (_filters[i].sharedMesh.uv[j].x * offset.width) + offset.x;
                float y = (_filters[i].sharedMesh.uv[j].y * offset.height) + offset.y;
                meshUVs[uvCount] = new Vector2(x, y);
                ++uvCount;
            }
        }

        // Apply new UVs
        _combinedMesh.uv = meshUVs;
    }

    private void GenerateMaterials()
    {
        Material material = new Material(Shader.Find("HDRP/Lit"));
        Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(SaveAssets.Rearrange($"{_path}_Base.png")[1], typeof(Texture2D));
        Debug.Log(texture);
        material.mainTexture = texture;

        SaveAssets.SaveFile($"{_path}_Material.mat", material);
    }
}