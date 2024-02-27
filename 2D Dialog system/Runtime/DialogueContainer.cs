using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
    public List<StartNodeData> StartNodeDatas = new List<StartNodeData>();
    public List<DialogueNodeData> DialogueNodeDatas = new List<DialogueNodeData>();
    public List<EventNodeData> EventNodeDatas = new List<EventNodeData>();
    public List<EndNodeData> EndNodeDatas = new List<EndNodeData>();


    public List<BaseNodeData> GetNodeDatas ()
    {
        List<BaseNodeData> list = new List<BaseNodeData>();

        list.AddRange(StartNodeDatas);
        list.AddRange(DialogueNodeDatas);
        list.AddRange(EventNodeDatas);
        list.AddRange(EndNodeDatas);
        return list;
    }
}
