using ModularMLAgents.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ModularMLAgents
{
    public class AgentGraphContext
    {
        private Dictionary<string, int> _nameRepeats = new Dictionary<string, int>();

        public AgentGraphContext(AgentGraphData data)
        {
            // Make one big list of AgentGraphElements (nodes + groups)
            // Sort it by names
            // For each asset check whether it's name matches the pattern (name_number)
            // Whenever the matched pattern does not hold valid pass it to rename queue with it's "base"

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

                // Check whether it matches data currently stored in repeats dictionary

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

        private Dictionary<UnityEngine.Object, List<Action<string>>> _nameChangeCallbacks = new Dictionary<UnityEngine.Object, List<Action<string>>>();

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
    }
}
