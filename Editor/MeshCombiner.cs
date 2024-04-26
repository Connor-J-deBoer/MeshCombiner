using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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

        ObjectField pathFolder = root.Q<ObjectField>("Path");
        ListView selectedList = root.Q<ListView>("SelectedList");
        Button button = root.Q<Button>("Button");

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
            try
            {
                if (selectedObjects.Count < 2) throw new System.Exception("Need more than one object");
                foreach (GameObject selectedObject in selectedObjects)
                {

                }
                string path = AssetDatabase.GetAssetPath(pathFolder.value.GetInstanceID());
                Debug.Log(path);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        };
    }
}
