using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ModularSensorManager : SensorComponent
{
    [SerializeField]
    private ModularAgent _agent;

    public override ISensor[] CreateSensors()
    {
        return _agent.SourceProviders.Select(p => p.SourceProvider.CreateSensor()).ToArray();
    }
}
