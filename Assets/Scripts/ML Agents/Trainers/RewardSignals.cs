using ModularMLAgents.Configuration;
using ModularMLAgents.Trainers;
using System;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using ModularMLAgents.Utilities;

namespace ModularMLAgents.Trainers
{
    [YamlObject]
    [System.Serializable]
    public partial class ExtrinsicRewards
    {
        public float Strength = 1.0f;
        public float Gamma = 0.99f;
    }

    [YamlObject]
    [System.Serializable]
    public partial class CuriosityRewards
    {
        public float Strength = 1.0f;
        public float Gamma = 0.99f;
        public NetworkSettings NetworkSettings;
        public float LearningRate = 3e-4f;
    }

    [YamlObject]
    [System.Serializable]
    public partial class GAILRewards
    {
        public float Strength = 1.0f;
        public float Gamma = 0.99f;
        public string DemoPath = string.Empty;
        public NetworkSettings NetworkSettings;
        public float LearningRate = 3e-4f;
        public bool UseActions = false;
        public bool UseVail = false;
    }

    [YamlObject]
    [System.Serializable]
    public partial class RNDRewards
    {
        public float Strength = 1.0f;
        public float Gamma = 0.99f;
        public NetworkSettings NetworkSettings;
        public float LearningRate = 3e-4f;
    }

    [YamlObject]
    [System.Serializable]
    public partial class RewardSignals
    {
        public bool UseExtrinsic = false;
        public ExtrinsicRewards Extrinsic = new ExtrinsicRewards();

        public bool UseCuriosity = false;
        public CuriosityRewards Curiosity = new CuriosityRewards();

        public bool UseGAIL = false;
        public GAILRewards Gail = new GAILRewards();

        public bool UseRND = false;
        public RNDRewards Rnd = new RNDRewards();
    }

    [CustomPropertyDrawer(typeof(RewardSignals))]
    public class RewardSignalsDrawer : PropertyDrawer
    {
        private bool _foldout = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rewardSignals = property.boxedValue as RewardSignals;

            _foldout = EditorGUILayout.Foldout(_foldout, "Reward signals");

            if (_foldout)
            {
                EditorGUILayout.BeginHorizontal();
                property.FindPropertyRelative("UseExtrinsic").boolValue = EditorGUILayout.Toggle(rewardSignals.UseExtrinsic, GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(!rewardSignals.UseExtrinsic);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("Extrinsic"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                property.FindPropertyRelative("UseCuriosity").boolValue = EditorGUILayout.Toggle(rewardSignals.UseCuriosity, GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(!rewardSignals.UseCuriosity);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("Curiosity"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                property.FindPropertyRelative("UseGAIL").boolValue = EditorGUILayout.Toggle(rewardSignals.UseGAIL, GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(!rewardSignals.UseGAIL);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("Gail"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                property.FindPropertyRelative("UseRND").boolValue = EditorGUILayout.Toggle(rewardSignals.UseRND, GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(!rewardSignals.UseRND);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("Rnd"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public class RewardSignalsFormatter : WriteOnlyYamlFormatter<RewardSignals>
    {
        public override void Serialize(ref Utf8YamlEmitter emitter, RewardSignals value, YamlSerializationContext context)
        {
            emitter.BeginMapping();
            {
                if (value.UseExtrinsic)
                {
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("extrinsic");
                        context.Resolver.GetFormatterWithVerify<ExtrinsicRewards>().Serialize(ref emitter, value.Extrinsic, context);
                    }
                    emitter.EndMapping();
                }

                if (value.UseCuriosity)
                {
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("curiosity");
                        context.Resolver.GetFormatterWithVerify<CuriosityRewards>().Serialize(ref emitter, value.Curiosity, context);
                    }
                    emitter.EndMapping();
                }

                if (value.UseGAIL)
                {
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("gail");
                        context.Resolver.GetFormatterWithVerify<GAILRewards>().Serialize(ref emitter, value.Gail, context);
                    }
                    emitter.EndMapping();
                }

                if (value.UseRND)
                {
                    emitter.BeginMapping();
                    {
                        emitter.WriteString("rnd");
                        context.Resolver.GetFormatterWithVerify<RNDRewards>().Serialize(ref emitter, value.Rnd, context);
                    }
                    emitter.EndMapping();
                }
            }
            emitter.EndMapping();
        }
    }
}
