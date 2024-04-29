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

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        SaveAssets.SaveFile($"{_path}.asset", combinedMesh);
    }

    public void CombineTexture()
    {
        int originalSize = _renderer[0].sharedMaterial.mainTexture.width;
        int powerOfTwo = GetTextureSize(_renderer);
        int size = powerOfTwo * originalSize;

        HashSet<Texture2D> baseMap = new HashSet<Texture2D>();
        Texture2D combinedBaseMap = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int i = 0; i < _renderer.Length; ++i)
        {
            baseMap.Add((Texture2D)_renderer[i].sharedMaterial.mainTexture);
        }

        Rect[] uvCoordinates = combinedBaseMap.PackTextures(baseMap.ToArray(), 0, size);

        SaveAssets.SaveFile($"{_path}_Base.png", combinedBaseMap);
    }

    private void UpdateUV()
    {

    }

    private int GetTextureSize(MeshRenderer[] renderer)
    {
        HashSet<Texture2D> textures = new HashSet<Texture2D>();
        // loop through all the textures in an object and only add unique ones
        for (int i = 0; i < renderer.Length; ++i)
        {
            textures.Add((Texture2D)renderer[i].sharedMaterial.mainTexture);
        }

        return Mathf.NextPowerOfTwo(textures.Count);
    }
}