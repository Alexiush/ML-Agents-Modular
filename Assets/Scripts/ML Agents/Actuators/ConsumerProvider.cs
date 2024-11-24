using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;

public abstract class ConsumerProvider : MonoBehaviour
{
    public abstract IActuator CreateActuator();
}
