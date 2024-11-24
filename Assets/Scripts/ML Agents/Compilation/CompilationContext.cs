using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Progress;

public class CompilationContext
{
    private Dictionary<string, HashSet<string>> _imports = new Dictionary<string, HashSet<string>>();

    public void AddDependency(string module, string dependency)
    {
        if (!_imports.ContainsKey(module))
        {
            _imports.Add(module, new HashSet<string>());
        }
        _imports[module].Add(dependency);
    }

    public void AddDependency(string dependency) => AddDependency(string.Empty, dependency);

    private void InitializeDefaultDependencies()
    {
        AddDependency("mlagents.torch_utils", "torch");
        AddDependency("mlagents.torch_utils", "nn");

        AddDependency("mlagents.trainers.torch_entities.layers", "Initialization");
        AddDependency("mlagents_envs.base_env", "ObservationSpec");

        AddDependency("typing", "List");
    }

    private Dictionary<AgentGraphNodeData, string> _nodeIds = new Dictionary<AgentGraphNodeData, string>();
    private Dictionary<string, int> _repeats = new Dictionary<string, int>();

    public string Register(string name, AgentGraphNodeData reference)
    {
        if (!_repeats.ContainsKey(name))
        {
            _repeats[name] = 0;
        }
        _repeats[name]++;
        
        var validName = $"{name}_{_repeats[name]}";
        _nodeIds[reference] = validName;
        return validName;
    }

    private void InitializeReservedReferences()
    {
        List<string> reservedNames = new List<string>
        {
            "result",
            "input_tensor"
        };

        foreach (string name in reservedNames)
        {
            _repeats.Add(name, 1);
        }
    }

    public string GetReference(AgentGraphNodeData reference)
    {
        string id;
        _nodeIds.TryGetValue(reference, out id);

        if (id == null)
        {
            throw new System.ArgumentException("Referenced node is unknown");
        }
        
        return id;
    }

    private List<Expression> _expressions = new List<Expression>();

    private HashSet<AgentGraphNodeData> _endpoints = new HashSet<AgentGraphNodeData>();

    public void RegisterEndpoint(AgentGraphNodeData endpoint)
    {
        _endpoints.Add(endpoint); 
    }

    private void AddImports(ref StringBuilder builder)
    {
        foreach (var pair in _imports)
        {
            if (pair.Key == string.Empty)
            {
                foreach (var item in pair.Value)
                {
                    builder.Append($"import {item}\n");
                }
                continue;
            }

            if (pair.Value.Contains("*"))
            {
                builder.Append($"from {pair.Key} import *");
                continue;
            }

            builder.Append($"from {pair.Key} import ");
            builder.Append(string.Join(", ", pair.Value));
            builder.Append("\n");
        }
    }

    private List<string> _parameters = new List<string>();
    private Dictionary<string, int> _parameterRepeats = new Dictionary<string, int>();

    public string RegisterParameter(string name, string body)
    {
        if (!_repeats.ContainsKey(name))
        {
            _repeats[name] = 0;
        }
        _repeats[name]++;

        var validName = $"{name}_{_repeats[name]}";
        _parameters.Add($"self.{validName} = {body}");
        return validName;
    }

    private void AddPrefix(ref StringBuilder builder)
    {
        var indent = new string(' ', 8);
        var prefix = $@"
class Model(nn.Module):
    """"""
    Linear layers.
    """"""

    MODEL_EXPORT_VERSION = 3  # Corresponds to ModelApiVersion.MLAgents2_0

    def __init__(
        self,
        observation_specs: List[ObservationSpec]
    ):
        super().__init__()

        self.observation_specs = observation_specs
        {string.Join("\n" + indent, _parameters.Select(p => p))}

        self.version_number = torch.nn.Parameter(
            torch.Tensor([self.MODEL_EXPORT_VERSION]), requires_grad=False
        )

    def forward(self, input_tensor: torch.Tensor) -> torch.Tensor:
";

        builder.Append(prefix);
    }

    private void AddExpressions(ref StringBuilder builder)
    {
        var indent = new string(' ', 8);

        foreach (var e in _expressions)
        {
            builder.AppendLine($"{indent}{e.Id}={e.Body}");
        }

        // All the loose ends to be merged into one variable
        var tensors = _endpoints.Select(e => GetReference(e));
        foreach (var tensor in tensors)
        {
            builder.AppendLine($"{indent}{tensor}=torch.flatten({tensor}, start_dim=1)");
        }
        builder.AppendLine($"{indent}result = torch.cat([{string.Join(", ", tensors)}], dim=1)");

        builder.AppendLine($"{indent}return result");
    }

    private List<SourceNodeData> Sources => _graphData.Nodes
        .Where(n => n is SourceNodeData)
        .Cast<SourceNodeData>()
        .ToList();

    public int GetSourceNumber(SourceNodeData source) => Sources.IndexOf(source);

    private void BuildExpressionsList()
    {
        HashSet<string> connectedPorts = new HashSet<string>();
        HashSet<AgentGraphNodeData> processedNodes = new HashSet<AgentGraphNodeData>();
        List<AgentGraphNodeData> processingStack = new List<AgentGraphNodeData>();

        processingStack.AddRange(Sources);

        while (processingStack.Count > 0)
        {
            // Get the first node for which all input ports are in connected ports
            var node = processingStack
                .Where(n => 
                {
                    var inputPorts = n.Ports.Where(p => p.Direction == Direction.Input);
                    return inputPorts.Count() == 0 || inputPorts.All(p => connectedPorts.Contains(p.GUID));
                })
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (node is null)
            {
                // processingStack.ForEach(n => Debug.Log(n.GetType()));
                throw new System.Exception("Error during compilation: unable to initialize all nodes");
            }

            if (processedNodes.Contains(node))
            {
                // A duplicate, to be removed
                processingStack.Remove(node);
                continue;
            }

            _expressions.Add(node.Compile(this));
            processedNodes.Add(node);
            processingStack.Remove(node);

            var connections = _consumerPorts[node];
            connections.ToList().ForEach(p => connectedPorts.Add(p));

            var connectedNodes = _consumerPorts[node]
                .Select(p => _portToNode[p])
                .Distinct();
            processingStack.AddRange(connectedNodes);
        }
    }

    public string Compile()
    {
        InitializeDefaultDependencies();
        InitializeReservedReferences();
        PrecomputeQueries();

        BuildExpressionsList();

        var builder = new StringBuilder();

        // Add imports
        AddImports(ref builder);
        // Use prefix of default module
        AddPrefix(ref builder);
        // Insert expressions
        AddExpressions(ref builder);

        return builder.ToString();
    }

    private AgentGraphData _graphData;

    public CompilationContext(AgentGraphData graphData)
    {
        _graphData = graphData;
    }

    private Dictionary<string, AgentGraphNodeData> _portToNode;
    private Dictionary<string, List<string>> _edges;
    private Dictionary<AgentGraphNodeData, List<string>> _sourcePorts;
    private Dictionary<AgentGraphNodeData, List<string>> _consumerPorts;

    private void PrecomputeQueries()
    {
        // Ports - map each port to the node it is on
        _portToNode = _graphData.Nodes
            .SelectMany(n => n.Ports.Select(p => (port: p.GUID, node: n)))
            .ToDictionary(keySelector: kv => kv.port, elementSelector: kv => kv.node);

        // Edges: port to its counterpart
        _edges = _graphData.Edges
            .SelectMany(e => new List<(string input, string output)>
            {
                (e.InputGUID, e.OutputGUID),
                (e.OutputGUID, e.InputGUID),
            })
            .GroupBy(e => e.input)
            .ToDictionary(keySelector: kv => kv.Key, elementSelector: kv => kv.Select(e => e.output).ToList());

        // Inputs - map each node to the list of ports that provide them with inputs
        _sourcePorts = _graphData.Nodes
            .Select(n =>
            {
                var sourcePorts = n.Ports
                    .Where(p => p.Direction == Direction.Input)
                    .Where(p => _edges.ContainsKey(p.GUID))
                    .SelectMany(p => _edges[p.GUID])
                    .ToList();

                return (node: n, ports: sourcePorts);
            })
            .ToDictionary(keySelector: kv => kv.node, elementSelector: kv => kv.ports);

        // Outputs - map each node to the list of ports that it provides with inputs 
        _consumerPorts = _graphData.Nodes
            .Select(n =>
            {
                var sourcePorts = n.Ports
                    .Where(p => p.Direction == Direction.Output)
                    .Where(p => _edges.ContainsKey(p.GUID))
                    .SelectMany(p => _edges[p.GUID])
                    .ToList();

                return (node: n, ports: sourcePorts);
            })
            .ToDictionary(keySelector: kv => kv.node, elementSelector: kv => kv.ports);
    }

    public List<AgentGraphNodeData> GetInputNodes(AgentGraphNodeData node)
    {
        return _sourcePorts[node]
            .Select(p => _portToNode[p])
            .Distinct()
            .ToList();
    }

    public List<string> GetInputs(AgentGraphNodeData node)
    {
        return GetInputNodes(node)
            .Select(n => GetReference(n))
            .ToList();
    }

    public List<string> GetOutputs(AgentGraphNodeData node)
    {
        return _consumerPorts[node]
            .Select(p => _portToNode[p])
            .Distinct()
            .Select(n => GetReference(n))
            .ToList();
    }
}
