// Copyright � Connor deBoer 2024, All Rights Reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

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
        List<CombineInstance[]> allSubCombines = new List<CombineInstance[]>();

        for (int i = 0; i < _gameObjects.Count; ++i)
        {
            combine[i].mesh = _filters[i].sharedMesh;
            combine[i].transform = _filters[i].transform.localToWorldMatrix;
            
            if (_filters[i].sharedMesh.subMeshCount != _filters[0].sharedMesh.subMeshCount)
            {
                throw new Exception("Not all meshes have same sub mesh count");
            }
            
            if (_filters[0].sharedMesh.subMeshCount > 1)
            {
                CombineInstance[] subCombine = new CombineInstance[_filters[0].sharedMesh.subMeshCount];
                for (int j = 0; j < _filters[0].sharedMesh.subMeshCount; ++j)
                {
                    CombineInstance subCombineInstance = new CombineInstance();
                    subCombineInstance.mesh = _filters[i].sharedMesh;
                    subCombineInstance.subMeshIndex = j;
                    subCombineInstance.transform = _filters[i].transform.worldToLocalMatrix;
                    subCombine[j] = subCombineInstance;
                }
                allSubCombines.Add(subCombine);
            }
        }

        _combinedMesh = new Mesh();
        _combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        _combinedMesh.CombineMeshes(combine);

        if (_filters[0].sharedMesh.subMeshCount > 1)
        {
            CombineInstance[] finalCombine = new CombineInstance[allSubCombines.Count];
            for (int i = 0; i < finalCombine.Length; ++i)
            {
                finalCombine[i] = new CombineInstance();
                finalCombine[i].mesh = new Mesh();
                finalCombine[i].mesh.CombineMeshes(allSubCombines[i]);
                finalCombine[i].transform = Matrix4x4.identity;
            }
            _combinedMesh.CombineMeshes(finalCombine, false);
        }

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

    public void CreateNewObject()
    {
        string name = _path.Split('/').Last();
        Type[] components = new Type[2]
        {
            typeof(MeshFilter),
            typeof(MeshRenderer),
        };
        GameObject combinedObject = new GameObject(name, components);

        MeshFilter combinedMeshFilter = combinedObject.GetComponent<MeshFilter>();
        MeshRenderer combinedMeshRenderer = combinedObject.GetComponent<MeshRenderer>();

        Mesh combinedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(SaveAssets.Rearrange($"{_path}_Mesh.asset")[1], typeof(Mesh));
        Material combinedMaterial = (Material)AssetDatabase.LoadAssetAtPath(SaveAssets.Rearrange($"{_path}_Material.mat")[1], typeof(Material));

        combinedMeshFilter.sharedMesh = combinedMesh;
        combinedMeshRenderer.sharedMaterial = combinedMaterial;

        SaveAssets.SaveFile($"{_path}.prefab", combinedObject);
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

        material.mainTexture = texture;

        HDMaterial.ValidateMaterial(material);

        SaveAssets.SaveFile($"{_path}_Material.mat", material);
    }
}