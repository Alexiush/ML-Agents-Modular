using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;

namespace ModularMLAgents.Models
{
    public class AgentGraphContext : IConnectionsContext
    {
        private AgentGraphView _graphView;

        private HashSet<string> _names = new();

        private string GetBaseName(string name)
        {
            var repeatPattern = new Regex(@".*_\d+", RegexOptions.IgnoreCase);
            var nameBase = name;

            if (repeatPattern.IsMatch(name))
            {
                var delimiterIndex = name.LastIndexOf('_');
                nameBase = name.Substring(0, delimiterIndex);
            }

            return nameBase;
        }

        private void InitializeNameRegistry(AgentGraphData data)
        {
            _names.Clear();

            var graphElements = data.Nodes
                .Concat<UnityEngine.Object>(data.Groups)
                .OrderBy(o => o.name)
                .ToList();

            foreach (var graphElement in graphElements)
            {
                var nameBase = GetBaseName(graphElement.name);
                string uniqueName = GetValidName(nameBase);
                if (uniqueName != graphElement.name)
                {
                    Rename(graphElement, uniqueName);
                }

                _names.Add(uniqueName);
            }
        }

        public AgentGraphContext(AgentGraphData data, AgentGraphView graphView)
        {
            _graphView = graphView;

            InitializeNameRegistry(data);
        }

        private string GenerateUniqueString(string nameBase)
        {
            if (!_names.Contains(nameBase))
            {
                return nameBase;
            }

            int i = 1;
            while (true)
            {
                string candidate = $"{nameBase}_{i}";
                if (!_names.Contains(candidate))
                {
                    return candidate;
                }
                i++;
            }
        }

        private string GetValidName(string name)
        {
            string uniqueName = GenerateUniqueString(name);
            return uniqueName;
        }

        private Dictionary<UnityEngine.Object, List<Action<string>>> _nameChangeCallbacks = new();

        public void SubscribeToNameChanges(UnityEngine.Object obj, Action<string> listener)
        {
            if (!_nameChangeCallbacks.ContainsKey(obj))
            {
                _nameChangeCallbacks[obj] = new List<Action<string>>();
            }

            _nameChangeCallbacks[obj].Add(listener);
        }

        public void UnsubscribeFromNameChanges(UnityEngine.Object obj, Action<string> listener)
        {
            if (!_nameChangeCallbacks.ContainsKey(obj))
            {
                return;
            }

            _nameChangeCallbacks[obj].Remove(listener);
        }

        public string RegisterName(UnityEngine.Object asset, string name)
        {
            return Rename(asset, name);
        }

        public void FreeName(string name)
        {
            _names.Remove(name);
        }

        public string Rename(UnityEngine.Object asset, string name)
        {
            if (name == asset.name)
            {
                return name;
            }

            string newName = GetValidName(name);

            FreeName(asset.name);
            asset.name = newName;
            _names.Add(newName);

            if (_nameChangeCallbacks.ContainsKey(asset))
            {
                _nameChangeCallbacks[asset].ForEach(l => l?.Invoke(newName));
            }

            return newName;
        }

        public T CreateInstance<T>(string name) where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            RegisterName(instance, name);

            return instance;
        }

        public List<AgentGraphNodeData> GetInputNodes(AgentGraphNodeData node) => _graphView.ports
            .Where(p => node.Ports
                .Where(p => p.Direction == Direction.Input)
                .Select(p => p.GUID)
                .Contains(p.viewDataKey)
            )
            .SelectMany(p => p.connections.Select(e => e.output.node))
            .OfType<IAgentGraphNode>()
            .Select(n => n.GetData())
            .OfType<AgentGraphNodeData>()
            .ToList();
        public List<string> GetInputs(AgentGraphNodeData node) => GetInputNodes(node).Select(x => x.name).ToList();
        public List<AgentGraphNodeData> GetOutputNodes(AgentGraphNodeData node) => _graphView.ports
            .Where(p => node.Ports
                .Where(p => p.Direction == Direction.Output)
                .Select(p => p.GUID)
                .Contains(p.viewDataKey)
            )
            .SelectMany(p => p.connections.Select(e => e.input.node))
            .OfType<IAgentGraphNode>()
            .Select(n => n.GetData())
            .OfType<AgentGraphNodeData>()
            .ToList();
        public List<string> GetOutputs(AgentGraphNodeData node) => GetOutputNodes(node).Select(x => x.name).ToList();

        private HashSet<AgentGraphNodeData> _changedNodes = new();
        public bool HasNodeChanges => _changedNodes.Count > 0;

        public void UpdateChangesStatus(AgentGraphNodeData node, bool status)
        {
            if (status)
            {
                _changedNodes.Add(node);
            }
            else
            {
                _changedNodes.Remove(node);
            }

            _graphView.UpdateGraphDataStatus();
        }

        public void ResetChanges()
        {
            _changedNodes.Clear();
        }
    }
}
