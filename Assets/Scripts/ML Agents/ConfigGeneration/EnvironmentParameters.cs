using ModularMLAgents.Utilities;
using System.Collections.Generic;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Serialization;

namespace ModularMLAgents.Configuration
{
    [YamlObject]
    [System.Serializable]
    public partial class EnvironmentParameters
    {
        [SubclassSelector, SerializeReference]
        public List<EnvironmentParameter> Parameters = new List<EnvironmentParameter>();
    }

    [YamlObject]
    [System.Serializable]
    [YamlObjectUnion("!float", typeof(EnvironmentParameterFloat))]
    [YamlObjectUnion("!int", typeof(EnvironmentParameterInteger))]
    [YamlObjectUnion("!bool", typeof(EnvironmentParameterBoolean))]
    [YamlObjectUnion("!string", typeof(EnvironmentParameterString))]
    [YamlObjectUnion("!sample", typeof(EnvironmentParameterSampled))]
    public abstract partial class EnvironmentParameter : IDynamicallyYAMLSerialized
    {
        public string Name;

        public abstract void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context);
    }

    public class EnvironmentParametersFormatter : WriteOnlyYamlFormatter<EnvironmentParameters>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, EnvironmentParameters value, YamlSerializationContext context)
        {
            emitter.BeginMapping();
            foreach (var parameter in value.Parameters)
            {
                parameter.Serialize(ref emitter, context);
            }
            emitter.EndMapping();
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class EnvironmentParameterFloat : EnvironmentParameter
    {
        public float Value;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.WriteString(Name);
            context.Serialize(ref emitter, Value);
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class EnvironmentParameterInteger : EnvironmentParameter
    {
        public int Value;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.WriteString(Name);
            context.Serialize(ref emitter, Value);
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class EnvironmentParameterBoolean : EnvironmentParameter
    {
        public bool Value;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.WriteString(Name);
            context.Serialize(ref emitter, Value);
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class EnvironmentParameterString : EnvironmentParameter
    {
        public string Value;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.WriteString(Name);
            context.Serialize(ref emitter, Value);
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class EnvironmentParameterSampled : EnvironmentParameter
    {
        [SubclassSelector, SerializeReference]
        public Sampler Value;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.WriteString(Name);
            context.Serialize(ref emitter, Value);
        }
    }
}
