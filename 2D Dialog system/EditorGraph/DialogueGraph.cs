using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;



public class DialogueGraph : EditorWindow
{
    private DialogueGraphView graphView;

    private string fileName = "New Narrative";

    [MenuItem("Graph/Dialogue Graph")]
   public static void OpenDialogueGraphWindow() {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable() {
        ConstractGraphView();
        GenerateToolbar();
    }

    private void OnDisable() {
        rootVisualElement.Remove(graphView);
    }

    private void ConstractGraphView() {
        graphView = new DialogueGraphView(this) {
            name = "Dialogue Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar() {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(val => {
            fileName = val.newValue;
        });
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => SaveData()) {
            text = "Save Data"
        });
        
        
        var fileNameTextField2 = new ObjectField()
        {
            objectType = typeof(DialogueContainer),
            allowSceneObjects = false,
            //value = ,
        };
        fileNameTextField2.RegisterValueChangedCallback(value =>
        {
            fileName = value.newValue.name;
            fileNameTextField.value = fileName;
        });
        toolbar.Add(fileNameTextField2);
        
        toolbar.Add(new Button(() => LoadData()) {
            text = "Load Data"
        });

        var nodeCreateButton = new Button(() => {
            graphView.CreateNode("", Vector2.zero);
        });
        nodeCreateButton.text = "Create Node";
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }

    private void SaveData() {
        if (!string.IsNullOrEmpty(fileName))
        {
            var saveUtility = GraphSaveUtility.GetInstance(graphView);
            saveUtility.SaveGraph(fileName);
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
        }
        
    }


    private void LoadData() {
        if (!string.IsNullOrEmpty(fileName))
        {
            var saveUtility = GraphSaveUtility.GetInstance(graphView);
            saveUtility.LoadGraph(fileName);
        }
    }

}
