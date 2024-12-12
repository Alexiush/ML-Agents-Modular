using ModularMLAgents.Compilation;
using ModularMLAgents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ModularMLAgents.Models
{
    [System.Serializable]
    public abstract class AgentGraphNodeData : ScriptableObject, ICompilable
    {
        [SerializeField]
        [HideInInspector]
        public AgentGraphElementMetadata Metadata;

        [SerializeField]
        [ValidationObserved]
        [HideInInspector]
        public List<AgentGraphPortData> Ports = new List<AgentGraphPortData>();

        public abstract IAgentGraphNode Load(AgentGraphContext context);

        public abstract string GetExpressionBody(CompilationContext compilationContext);

        public abstract string GetAccessor(CompilationContext compilationContext, AgentGraphNodeData outputReceiver);

        public abstract List<TensorShape> GetOutputShape(IConnectionsContext compilationContext);

        public abstract List<TensorShape> GetPartialOutputShape(IConnectionsContext compilationContext, AgentGraphNodeData outputReceiver);

        public Expression Compile(CompilationContext compilationContext) => new Expression
        {
            Name = compilationContext.Register(this),
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
}
