using ModularMLAgents.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ModularMLAgents.Editor
{
    public class AgentGraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private AgentGraphView _graphView;
        private Texture2D _indentationIcon;

        public void Initialize(AgentGraphView graphView)
        {
            _graphView = graphView;

            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, Color.clear);
            _indentationIcon.Apply();
        }

        private List<SearchTreeEntry> FromGrouping(IGrouping<string, (Type t, string[] p)> grouping, int depth)
        {
            if (grouping.Count() == 1)
            {
                var element = grouping.First();
                var entry = new SearchTreeEntry(new GUIContent(element.p.ElementAt(depth - 1), _indentationIcon))
                {
                    userData = element.t,
                    level = depth
                };

                return new List<SearchTreeEntry> { entry };
            }

            var entries = grouping
                .GroupBy(pair => pair.p.ElementAt(depth))
                .SelectMany(g => FromGrouping(g, depth + 1))
                .ToList();

            var group = new SearchTreeGroupEntry(new GUIContent(grouping.First().p.ElementAt(depth)), depth);
            entries.Insert(0, group);

            return entries;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var nodeTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(IAgentGraphNode)))
                .Where(t => t.IsDefined(typeof(NodePathAttribute), false))
                .Where(t => t.IsClass && !t.IsAbstract);

            var paths = nodeTypes
                .Select(t => Attribute.GetCustomAttribute(t, typeof(NodePathAttribute)) as NodePathAttribute)
                .Select(a => a.Path.Split("\\"));

            var typesAndPaths = nodeTypes.Zip(paths, (t, p) => (t, p));

            var entries = typesAndPaths
                .GroupBy(pair => pair.p.ElementAt(0))
                .SelectMany(g => FromGrouping(g, 1))
                .ToList();

            var group = new SearchTreeGroupEntry(new GUIContent("Agent graph node"), 0);
            entries.Insert(0, group);

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var data = SearchTreeEntry.userData as System.Type;

            if (data == null)
            {
                return false;
            }

            var windowOffset = _graphView.Window.position.position;
            var viewOffset = new Vector2(_graphView.viewTransform.position.x, _graphView.viewTransform.position.y);
            Vector2 localMousePosition = context.screenMousePosition - windowOffset - viewOffset;

            _graphView.CreateNode(localMousePosition / _graphView.viewTransform.scale, data);
            return true;
        }
    }
}
