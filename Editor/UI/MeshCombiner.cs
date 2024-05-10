// Copyright © Connor deBoer 2024, All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class MeshCombiner : EditorWindow
{
    [SerializeField] private VisualTreeAsset _visualTreeAsset = default;

    private Object _path;

    [MenuItem("Tools/Mesh Combiner")]
    public static void ShowExample()
    {
        MeshCombiner wnd = GetWindow<MeshCombiner>();
        wnd.titleContent = new GUIContent("MeshCombiner");
    }

    private void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = _visualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        TextField name = root.Q<TextField>("CombinedName");
        ObjectField pathFolder = root.Q<ObjectField>("Path");
        ListView selectedList = root.Q<ListView>("SelectedList");
        Button button = root.Q<Button>("Button");
        Toggle createNewPrefab = root.Q<Toggle>("CreateNewPrefab");
        Toggle createNewTexture = root.Q<Toggle>("CreateNewTexture");

        _path = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("path"), typeof(Object));
        if (_path != null) pathFolder.value = _path;

        pathFolder.objectType = typeof(DefaultAsset);

        List<GameObject> selectedObjects = new List<GameObject>();
        Selection.selectionChanged += () =>
        {
            selectedObjects = Selection.gameObjects.Where(obj => 
                obj.TryGetComponent(out MeshFilter meshFilter) && 
                obj.TryGetComponent(out MeshRenderer meshRenderer)).ToList();

            selectedList.makeItem = () => new ObjectField();
            selectedList.bindItem = (element, index) => (element as ObjectField).value = selectedObjects[index];
            selectedList.itemsSource = selectedObjects;
        };

        button.clicked += () =>
        {
            if (pathFolder.value == null) throw new System.Exception("No Directory Specified");
            if (selectedObjects.Count == 0) throw new System.Exception("No Objects Selected");

            string path = AssetDatabase.GetAssetPath(pathFolder.value.GetInstanceID());
            EditorPrefs.SetString("path", path);
            Combine combiner = new Combine($"{path}/{name.text}", selectedObjects);

            combiner.CombineMesh();
            if (createNewTexture.value) combiner.CombineTexture();
            if (createNewPrefab.value) combiner.CreateNewObject();
        };
    }
}