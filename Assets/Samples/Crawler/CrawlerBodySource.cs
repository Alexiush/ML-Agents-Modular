using ModularMLAgents.Sensors;
using ModularMLAgents.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CrawlerBodySource : SourceProvider
{
    [SerializeField]
    private CrawlerAgent _agent;

    public override ISensor[] CreateSensors()
    {
        // For each limb instantiate prefab
        // it contains two transforms: upper and lower parts of the limb
        // Register them wihin controller
        // Setup upper limbs connection to Crawler's rigidbody

        return new ISensor[] { new CrawlerBodySensor(_agent, 22, "SourceBody") };
    }

    public override int SourcesCount => 1;

    public override ObservationSpec ObservationSpec => ObservationSpec.Vector(22);
}