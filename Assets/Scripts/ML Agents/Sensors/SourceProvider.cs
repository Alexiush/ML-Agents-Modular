using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public abstract class SourceProvider : MonoBehaviour
{
    public abstract ISensor CreateSensor();
}
