using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;


public class DialogueGraphView : GraphView
{

    public readonly Vector2 defaultNodeSize = new Vector2(100, 200);

    private NodeSearchWindow searchWindow;

    public DialogueGraphView(EditorWindow editorWindow) {

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(EditorWindow editorWindow) {
        searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Init(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(
            new SearchWindowContext(context.screenMousePosition), searchWindow);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port => {
            if(startPort != port && startPort.node != port.node) {
                compatiblePorts.Add(port);
            }
            
            });

        return compatiblePorts;
    }

    private Port GeneratePort(BaseNode node, Direction portDirection, 
        Port.Capacity capacity = Port.Capacity.Single) {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    private StartNode GenerateEntryPointNode() {
        var node = new StartNode {
            title = "START",
            GUID = Guid.NewGuid().ToString()
        };
        
        

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.name = "Next";
        node.outputContainer.Add(generatedPort);

        node.capabilities &= ~Capabilities.Deletable;//запрет на удаление

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 200));
        return node;
    }

    public void AddChoicePort(BaseNode dialogueNode, string overridenPortName = "") {
        var generatedPort = GeneratePort(dialogueNode, Direction.Output);
        var portLable = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(portLable);

        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
        //generatedPort.portName = $"Choice {outputPortCount}";
        var choicePortName = string.IsNullOrEmpty(overridenPortName)
            ? $"Choice {outputPortCount}" : overridenPortName;

        var textField = new TextField {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(val => generatedPort.portName = val.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort)) {
            text = "x"
        };
        generatedPort.contentContainer.Add(deleteButton);

        generatedPort.portName = choicePortName;
        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
    }

    private void RemovePort(BaseNode node, Port generatedPort) {
        var targetEdge = edges.ToList().Where(
        x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any()) {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }
        
        node.outputContainer.Remove(generatedPort);
        node.RefreshPorts();
        node.RefreshExpandedState();
    }

    public void CreateNode(string nodeName, Vector2 position) {
        AddElement(CreateDialogueNode("",nodeName, position, null, null));
    }

    public DialogueNode CreateDialogueNode(string actorName, string nodeText,
        Vector2 position, Sprite FaceAvatar, Sprite FullBodyAvatar)
    {
        
        var dialogueNode = new DialogueNode {
            title = "DialogueNode",
            ActorName = actorName,
            DialogueText = nodeText,
            FaceAvatar = FaceAvatar,
            FullBodyAvatar = FullBodyAvatar,
            GUID = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        var button = new Button(() => { AddChoicePort(dialogueNode); });
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);
        
        var avatarLable = new Label();
        avatarLable.text = "  Аватар";
        dialogueNode.mainContainer.Add(avatarLable);
        var spriteAvatarField = new ObjectField()
        {
            objectType = typeof(Sprite),
            allowSceneObjects = false,
            //          value = inputSprite.Value,
        };

        // When we change the variable from graph view.
        spriteAvatarField.RegisterValueChangedCallback(value =>
        {
//            inputSprite.Value = value.newValue as Sprite;
//            imagePreview.image = (inputSprite.Value != null ? inputSprite.Value.texture : null);
            dialogueNode.FaceAvatar = value.newValue as Sprite;
        });
        //       imagePreview.image = (inputSprite.Value != null ? inputSprite.Value.texture : null);
        if(FaceAvatar != null) spriteAvatarField.SetValueWithoutNotify(FaceAvatar);
        dialogueNode.mainContainer.Add(spriteAvatarField);
        
        var namaeLable = new Label();
        namaeLable.text = "  Имя";
        dialogueNode.mainContainer.Add(namaeLable);
        var namaeTextField = new TextField(string.Empty);
        namaeTextField.RegisterValueChangedCallback(evt => {
            dialogueNode.ActorName = evt.newValue;
        });
        namaeTextField.value = "Namae";
        if(nodeText.Length > 0)namaeTextField.SetValueWithoutNotify(actorName);
        dialogueNode.mainContainer.Add(namaeTextField);

        
        var textLable = new Label();
        textLable.text = "  Текст";
        dialogueNode.mainContainer.Add(textLable);
        
        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt => {
            dialogueNode.DialogueText = evt.newValue;
        //   dialogueNode.title = evt.newValue;
        });
        textField.multiline = true;
        if(nodeText.Length > 0)textField.SetValueWithoutNotify(nodeText);
        dialogueNode.mainContainer.Add(textField);


        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();

        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));
        return dialogueNode;
    }

    public void CreateNewEventNode(string nodeName, Vector2 position) {
        AddElement(CreateEventNode(nodeName, position, null));
    }
    public EventNode CreateEventNode(string nodeName, Vector2 position, ScriptableObject eventScript) {

        var eventNode = new EventNode {
            title = "EventNode",
            EventScript = eventScript,
            GUID = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(eventNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        eventNode.inputContainer.Add(inputPort);

        eventNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        var button = new Button(() => { AddChoicePort(eventNode); });
        button.text = "New Choice";
        eventNode.titleContainer.Add(button);
        
        
        var avatarLable = new Label();
        avatarLable.text = "  Event";
        eventNode.mainContainer.Add(avatarLable);
        var eventScriptField = new ObjectField()
        {
            objectType = typeof(ScriptableObject),
            allowSceneObjects = false,
            //          value = inputSprite.Value,
        };
        // When we change the variable from graph view.
        eventScriptField.RegisterValueChangedCallback(value =>
        {
            eventNode.EventScript = value.newValue as ScriptableObject;
        });
        //       imagePreview.image = (inputSprite.Value != null ? inputSprite.Value.texture : null);
        if(eventScript != null) eventScriptField.SetValueWithoutNotify(eventScript);
        eventNode.mainContainer.Add(eventScriptField);
        

        eventNode.RefreshExpandedState();
        eventNode.RefreshPorts();

        eventNode.SetPosition(new Rect(position, defaultNodeSize));
        return eventNode;
    }


    public void CreateNewEndNode(string nodeName, Vector2 position) {
      AddElement(CreateEndNode(nodeName, position));
    }
    public EndNode CreateEndNode(string nodeName, Vector2 position) {

        var eventNode = new EndNode() {
            title = "EndNode",
            GUID = Guid.NewGuid().ToString()
        };

        var inputPort = GeneratePort(eventNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        eventNode.inputContainer.Add(inputPort);

        eventNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        var button = new Button(() => { AddChoicePort(eventNode); });
        button.text = "New Choice";
        eventNode.titleContainer.Add(button);

        eventNode.RefreshExpandedState();
        eventNode.RefreshPorts();

        eventNode.SetPosition(new Rect(position, defaultNodeSize));
        return eventNode;
    }


}
