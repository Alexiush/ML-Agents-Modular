using ModularMLAgents.Configuration;
using ModularMLAgents.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace ModularMLAgents.Configuration
{
    [YamlObject]
    [System.Serializable]
    [YamlObjectUnion("!uniform", typeof(UniformSampler))]
    [YamlObjectUnion("!gaussian", typeof(GaussianSampler))]
    [YamlObjectUnion("!multirangeuniform", typeof(MultirangeUniformSampler))]
    public abstract partial class Sampler : IDynamicallyYAMLSerialized
    {
        public abstract void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context);
    }

    public class SamplerFormatter : WriteOnlyYamlFormatter<Sampler>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, Sampler value, YamlSerializationContext context)
        {
            value.Serialize(ref emitter, context);
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class UniformSampler : Sampler
    {
        public float MinValue;
        public float MaxValue;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.BeginMapping();
            {
                emitter.WriteString("sampler_type");
                emitter.WriteString("uniform");

                emitter.WriteString("sampler_parameters");
                emitter.BeginMapping();
                {
                    emitter.WriteString("min_value");
                    context.Serialize(ref emitter, MinValue);

                    emitter.WriteString("max_value");
                    context.Serialize(ref emitter, MaxValue);
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class GaussianSampler : Sampler
    {
        public float Mean;
        public float StDev;

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.BeginMapping();
            {
                emitter.WriteString("sampler_type");
                emitter.WriteString("gaussian");

                emitter.WriteString("sampler_parameters");
                emitter.BeginMapping();
                {
                    emitter.WriteString("mean");
                    context.Serialize(ref emitter, Mean);

                    emitter.WriteString("st_dev");
                    context.Serialize(ref emitter, StDev);
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();
        }
    }

    [YamlObject]
    [System.Serializable]
    public partial class MultirangeUniformSampler : Sampler
    {
        [System.Serializable]
        public class Interval
        {
            public int Start;
            public int End;
        }

        public List<Interval> Intervals = new List<Interval>();

        public override void Serialize(ref Utf8YamlEmitter emitter, YamlSerializationContext context)
        {
            emitter.BeginMapping();
            {
                emitter.WriteString("sampler_type");
                emitter.WriteString("multirangeuniform");

                emitter.WriteString("sampler_parameters");
                emitter.BeginMapping();
                {
                    emitter.WriteString("intervals");
                    var intervalsString = $"[{string.Join(", ", Intervals.Select(i => $"[{i.Start}, {i.End}]"))}]";
                    emitter.WriteScalar(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(intervalsString)));
                }
                emitter.EndMapping();
            }
            emitter.EndMapping();
        }
    }
}
