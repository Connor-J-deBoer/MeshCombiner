// Copyright © Connor deBoer 2024, All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class MeshCombiner : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/Mesh Combiner")]
    public static void ShowExample()
    {
        MeshCombiner wnd = GetWindow<MeshCombiner>();
        wnd.titleContent = new GUIContent("MeshCombiner");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        TextField name = root.Q<TextField>("CombinedName");
        ObjectField pathFolder = root.Q<ObjectField>("Path");
        ListView selectedList = root.Q<ListView>("SelectedList");
        Button button = root.Q<Button>("Button");
        Toggle createNew = root.Q<Toggle>("CreateNew");

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
            if (selectedObjects.Count == 0) throw new System.Exception("No Objects Selected");
            string path = AssetDatabase.GetAssetPath(pathFolder.value.GetInstanceID());
            Combine combiner = new Combine($"{path}/{name.text}", selectedObjects);

            combiner.CombineMesh();
            //combiner.CombineTexture();
            if (createNew.value) combiner.CreateNewObject();
        };
    }
}