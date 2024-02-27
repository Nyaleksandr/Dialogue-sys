using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class GraphSaveUtility
{
   private DialogueGraphView graphView;
   private DialogueContainer dialogueContainer;
   
   private List<Edge> Edges => graphView.edges.ToList();
   private List<BaseNode> Nodes => graphView.nodes.ToList().Cast<BaseNode>().ToList();
  // private List<DialogueNode> Nodes => graphView.nodes.ToList().Cast<DialogueNode>().ToList();
   
   public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
   {
      return new GraphSaveUtility
      {
         graphView = graphView
      };
   }

   public void SaveGraph(string fileName)
   {
      var nodeContainer = ScriptableObject.CreateInstance<DialogueContainer>();

      //var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
      var connectedPorts = Edges.ToArray();
      for (var i =0; i < connectedPorts.Length; i++)
      {
         var outputNode = connectedPorts[i].output.node as BaseNode;
         var inputNode = connectedPorts[i].input.node as BaseNode;
         
         nodeContainer.NodeLinks.Add(new NodeLinkData
         {
            BaseNodeGuid = outputNode.GUID,
            PortName = connectedPorts[i].output.portName,
            TargetNodeGuid = inputNode.GUID
         });
      }

      foreach (var node in Nodes)
      {
//         if(node.EntryPoint) continue;
         if (node is StartNode)
         {
            nodeContainer.StartNodeDatas.Add(new StartNodeData
            {
               Guid = node.GUID,
               Position = node.GetPosition().position
            });
         }
         if (node is DialogueNode)
         {
            var nodeT = node as DialogueNode;
            nodeContainer.DialogueNodeDatas.Add(new DialogueNodeData
            {
               Guid = nodeT.GUID,
               DialogueText = nodeT.DialogueText,
               ActorName = nodeT.ActorName,
               Position = nodeT.GetPosition().position,
               FaceAvatar = nodeT.FaceAvatar,
               FullBodyAvatar = nodeT.FullBodyAvatar
            });
            
         } 
         if (node is EventNode)
         {
            var nodeT = node as EventNode;
            nodeContainer.EventNodeDatas.Add(new EventNodeData
            {
               Guid = nodeT.GUID,
               EventScript = nodeT.EventScript,
               Position = nodeT.GetPosition().position
            });
            
         } 
         if (node is EndNode) {
            nodeContainer.EndNodeDatas.Add(new EndNodeData
            {
               Guid = node.GUID,
               Position = node.GetPosition().position
            });
         }

      }

      if (!AssetDatabase.IsValidFolder("Assets/Resources"))
      {
         AssetDatabase.CreateFolder("Assets", "Resources");
      }
      
      AssetDatabase.CreateAsset(nodeContainer,$"Assets/Resources/{fileName}.asset" );
      AssetDatabase.SaveAssets();
   }
   
   public void LoadGraph(string fileName)
   {
      dialogueContainer = Resources.Load<DialogueContainer>(fileName);
      if (dialogueContainer == null)
      {
         EditorUtility.DisplayDialog("File Not Found", "Narrative Data does not exist!", "OK");
         return;
      }

      CleareGraph();
      CreateNodes();
      ConnectNodes();
   }

   private void CleareGraph()
   {
      Nodes.Find(x => x is StartNode).GUID = dialogueContainer.NodeLinks[0].BaseNodeGuid;
      foreach (var node in Nodes)
      {
         if(node is StartNode) continue;

         Edges.Where(x => x.input.node == node).ToList()
            .ForEach(edge => graphView.RemoveElement(edge));
         
         graphView.RemoveElement(node);
      }
   }
   
   private void CreateNodes()
   {
      foreach (var node in dialogueContainer.DialogueNodeDatas)
      {
         var tempNode = graphView.CreateDialogueNode(node.ActorName, node.DialogueText, Vector2.zero, node.FaceAvatar, node.FullBodyAvatar);
         //var tempNode = graphView.CreateDialogueNode("", Vector2.zero);
         tempNode.GUID = node.Guid;
         graphView.AddElement(tempNode);

         var nodePorts = dialogueContainer.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
         nodePorts.ForEach(x => graphView.AddChoicePort(tempNode, x.PortName));
      }
      
      foreach (var node in dialogueContainer.EventNodeDatas)
      {
         var tempNode = graphView.CreateEventNode("", Vector2.zero, node.EventScript);
         tempNode.GUID = node.Guid;
         graphView.AddElement(tempNode);

         var nodePorts = dialogueContainer.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
         nodePorts.ForEach(x => graphView.AddChoicePort(tempNode, x.PortName));
      }
      
      foreach (var node in dialogueContainer.EndNodeDatas)
      {
         var tempNode = graphView.CreateEndNode("", Vector2.zero);
         tempNode.GUID = node.Guid;
         graphView.AddElement(tempNode);

         var nodePorts = dialogueContainer.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
         nodePorts.ForEach(x => graphView.AddChoicePort(tempNode, x.PortName));
      }
   }
   
   private void ConnectNodes()
   {
      for (int i = 0; i < Nodes.Count; i++)
      {
         var connections = dialogueContainer.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
         for (int j = 0; j < connections.Count; j++)
         {
            var targetNodeGUID = connections[j].TargetNodeGuid;
            var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
            LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);
            
            targetNode.SetPosition(new Rect(
               dialogueContainer.GetNodeDatas().First(x => x.Guid == targetNodeGUID).Position,
               graphView.defaultNodeSize));
         }
      }
   }

   private void LinkNodes(Port output, Port input)
   {
      var tempEdge = new Edge()
      {
         output = output,
         input = input
      };
      tempEdge?.input.Connect(tempEdge);
      tempEdge?.output.Connect(tempEdge);
      graphView.Add(tempEdge);
   }
}

