using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;



public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{

    private DialogueGraphView graphView;
    private EditorWindow window;

    public void Init (EditorWindow window, DialogueGraphView graphView) {
        this.graphView = graphView;
        this.window = window;
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        var tree = new List<SearchTreeEntry> {
        new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),//добавляет в меню правой кнопки пункт
        new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),// добавляет пункт в меню поиска нодов

        
        new SearchTreeEntry(new GUIContent("Dialogue Node")) {
            userData = new DialogueNode(), level = 2
        },
        new SearchTreeEntry(new GUIContent("Event Node")) {
            userData = new EventNode(), level = 2
        },
        new SearchTreeEntry(new GUIContent("End Node")) {
            userData = new EndNode(), level = 2
        }

        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context) {
        var worldMousePosition = window.rootVisualElement.ChangeCoordinatesTo(
            window.rootVisualElement.parent, context.screenMousePosition - window.position.position);
        var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousePosition);
        switch (SearchTreeEntry.userData) {
            case DialogueNode dialogueNode:
                graphView.CreateNode("", localMousePosition);
                return true;
            case EventNode eventNode:
                graphView.CreateNewEventNode("Event Node", localMousePosition);
                return true;
            
            case EndNode endNode:
                graphView.CreateNewEndNode("End Node", localMousePosition);
                return true;
            default:
                return false;
        }

    }
}
