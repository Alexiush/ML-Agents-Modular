using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System.Reflection;
using System;
using System.Linq;
using Unity.VisualScripting;

[System.Serializable]
public abstract class AgentGraphNodeData : ScriptableObject, ICompilable
{
    [SerializeField] public AgentGraphElementMetadata Metadata;
    [SerializeField] public List<AgentGraphPortData> Ports;

    public abstract AgentGraphNode Load();

    public abstract string GetExpressionBody(CompilationContext compilationContext);

    public Expression Compile(CompilationContext compilationContext) => new Expression
    {
        Id = compilationContext.Register(GetType().Name, this),
        Body = GetExpressionBody(compilationContext),
    };
}

[System.Serializable]
public class AgentGraphPortData
{
    public Orientation Orientation;
    public Direction Direction;
    public Port.Capacity Capacity;

    public string AssemblyName;
    public string TypeName;
    public System.Type Type => AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.FullName == AssemblyName)
            .Single()
            .GetType(TypeName);

    public string Name;
    public string GUID;

    public AgentGraphPortData(Port port)
    {
        Orientation = port.orientation;
        Direction = port.direction;
        Capacity = port.capacity;

        AssemblyName = port.portType.Assembly.FullName;
        TypeName = port.portType.FullName;

        Name = port.name;
        GUID = port.viewDataKey;
    }

    public void Instantiate(Node node)
    {
        var port = node.InstantiatePort(Orientation, Direction, Capacity, Type);
        port.name = Name;
        port.viewDataKey = GUID;
    }

}
