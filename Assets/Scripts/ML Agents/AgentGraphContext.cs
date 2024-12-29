using ModularMLAgents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ModularMLAgents
{
    public class AgentGraphContext : IConnectionsContext
    {
        private AgentGraphView _graphView;

        private Dictionary<string, int> _nameRepeats = new();

        private void InitializeNameRegistry(AgentGraphData data)
        {
            var objectsToRename = new List<(UnityEngine.Object obj, string name)>();

            var graphElements = data.Nodes
                .Concat<UnityEngine.Object>(data.Groups)
                .OrderBy(o => o.name);

            var repeatPattern = new Regex(@".*_\d+", RegexOptions.IgnoreCase);

            foreach (var graphElement in graphElements)
            {
                var nameBase = graphElement.name;
                int repeats = 1;

                if (repeatPattern.IsMatch(graphElement.name))
                {
                    var delimiterIndex = graphElement.name.LastIndexOf('_');

                    nameBase = graphElement.name.Substring(0, delimiterIndex);
                    repeats = int.Parse(graphElement.name.Substring(delimiterIndex + 1, graphElement.name.Length - delimiterIndex - 1));
                }

                int actualRepeats = _nameRepeats.ContainsKey(nameBase) ? _nameRepeats[nameBase] + 1 : 1;

                if (repeats != actualRepeats)
                {
                    objectsToRename.Add((graphElement, nameBase));
                    continue;
                }
                _nameRepeats[nameBase] = actualRepeats;
            }

            objectsToRename.ForEach(p => Rename(p.obj, p.name));
        }

        public AgentGraphContext(AgentGraphData data, AgentGraphView graphView)
        {
            _graphView = graphView;

            InitializeNameRegistry(data);
        }

        private string GetValidName(string name)
        {
            if (!_nameRepeats.ContainsKey(name))
            {
                _nameRepeats[name] = 0;
            }
            _nameRepeats[name]++;

            var validName = _nameRepeats[name] == 1 ? name : $"{name}_{_nameRepeats[name]}";
            return validName;
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
            string newName = GetValidName(name);
            asset.name = newName;

            if (_nameChangeCallbacks.ContainsKey(asset))
            {
                _nameChangeCallbacks[asset].ForEach(l => l?.Invoke(newName));
            }

            return newName;
        }

        public void FreeName(string name)
        {
            if (_nameRepeats.ContainsKey(name))
            {
                _nameRepeats[name]--;
            }

            var repeatPattern = new Regex(@".*_\d+", RegexOptions.IgnoreCase);

            var nameBase = name;
            int repeats = 1;

            if (repeatPattern.IsMatch(name))
            {
                var delimiterIndex = name.LastIndexOf('_');

                nameBase = name.Substring(0, delimiterIndex);
                repeats = int.Parse(name.Substring(delimiterIndex + 1, name.Length - delimiterIndex - 1));
                int actualRepeats = _nameRepeats.ContainsKey(nameBase) ? _nameRepeats[nameBase] : 0;

                if (repeats == actualRepeats)
                {
                    _nameRepeats[nameBase]--;
                }
            }
        }

        public string Rename(UnityEngine.Object asset, string name)
        {
            string newName = GetValidName(name);
            asset.name = newName;

            _nameRepeats[name]--;

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

        // Contains current state of node changes (bool, or if possible set of changed nodes)
        // On save resets the data

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
