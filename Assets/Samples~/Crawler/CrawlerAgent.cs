using ModularMLAgents.Components;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples.Crawler;
using UnityEngine;

public class CrawlerAgent : ModularAgent
{
    public Transform Target;
    public Transform Body;

    [Header("Walk Speed")]
    [Range(0.1f, _maxWalkingSpeed)]
    [SerializeField]
    [Tooltip(
        "The speed the agent will try to match.\n\n" +
        "TRAINING:\n" +
        "For VariableSpeed envs, this value will randomize at the start of each training episode.\n" +
        "Otherwise the agent will try to match the speed set here.\n\n" +
        "INFERENCE:\n" +
        "During inference, VariableSpeed agents will modify their behavior based on this value " +
        "whereas the CrawlerDynamic & CrawlerStatic agents will run at the speed specified during training "
    )]
    //The walking speed to try and achieve
    private float _targetWalkingSpeed = _maxWalkingSpeed;
    const float _maxWalkingSpeed = 15; //The max walking speed

    public float TargetWalkingSpeed
    {
        get { return _targetWalkingSpeed; }
        set { _targetWalkingSpeed = Mathf.Clamp(value, .1f, _maxWalkingSpeed); }
    }

    [field: SerializeField]
    [field: Range(2, 10)]
    public int LimbsCount { get; private set; } = 4;

    public List<Transform> BodyParts { get; private set; } = new List<Transform>();

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    public OrientationCubeController OrientationCube { get; private set; }

    //The indicator graphic gameobject that points towards the target
    public DirectionIndicator DirectionIndicator { get; private set; }
    public JointDriveController JdController { get; private set; }

    private void ResetTarget(Vector3 pos)
    {
        Target.position = pos;
        Target.rotation = Quaternion.identity;
    }

    [SerializeField]
    private Transform _limbPrefab;

    protected override void OnEnable()
    {
        for (int i = 0; i < _limbsParent.childCount; i++)
        {
            Destroy(_limbsParent.GetChild(i).gameObject);
        }

        // Instantiate body parts
        JdController = GetComponent<JointDriveController>();
        JdController.SetupBodyPart(Body);

        Enumerable.Range(0, LimbsCount)
            .Select(i => Instantiate(_limbPrefab, Vector3.zero, Quaternion.Euler(0, 360 * i / LimbsCount, 0), _limbsParent))
            .ToList()
            .ForEach(t =>
            {
                t.localPosition = Vector3.down;

                var upperLimb = t.GetChild(0);
                upperLimb.GetComponent<ConfigurableJoint>().connectedBody = Body.GetComponent<Rigidbody>();
                JdController.SetupBodyPart(upperLimb);
                BodyParts.Add(upperLimb);

                var lowerLimb = upperLimb.GetChild(0);
                JdController.SetupBodyPart(lowerLimb);
                BodyParts.Add(lowerLimb);
            });

        base.OnEnable();
    }

    [SerializeField]
    private Transform _limbsParent;

    public override void Initialize()
    {
        base.Initialize();

        OrientationCube = GetComponentInChildren<OrientationCubeController>();
        DirectionIndicator = GetComponentInChildren<DirectionIndicator>();
    }

    public override void OnEpisodeBegin()
    {
        foreach (var bodyPart in JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        //Random start rotation to help generalize
        Body.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);

        UpdateOrientationObjects();

        //Set our goal walking speed
        TargetWalkingSpeed = Random.Range(0.1f, _maxWalkingSpeed);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    private void FixedUpdate()
    {
        UpdateOrientationObjects();

        var cubeForward = OrientationCube.transform.forward;

        // Set reward for this step according to mixture of the following elements.
        // a. Match target speed
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        var matchSpeedReward = GetMatchingVelocityReward(cubeForward * TargetWalkingSpeed, GetAvgVelocity());

        // b. Rotation alignment with target direction.
        //This reward will approach 1 if it faces the target direction perfectly and approach zero as it deviates
        var lookAtTargetReward = (Vector3.Dot(cubeForward, Body.forward) + 1) * .5F;

        AddReward(matchSpeedReward * lookAtTargetReward);
    }

    /// <summary>
    /// Update OrientationCube and DirectionIndicator
    /// </summary>
    private void UpdateOrientationObjects()
    {
        OrientationCube.UpdateOrientation(Body, Target);
        if (DirectionIndicator)
        {
            DirectionIndicator.MatchOrientation(OrientationCube.transform);
        }
    }

    /// <summary>
    ///Returns the average velocity of all of the body parts
    ///Using the velocity of the body only has shown to result in more erratic movement from the limbs
    ///Using the average helps prevent this erratic movement
    /// </summary>
    public Vector3 GetAvgVelocity()
    {
        Vector3 velSum = Vector3.zero;
        Vector3 avgVel = Vector3.zero;

        //ALL RBS
        int numOfRb = 0;
        foreach (var item in JdController.bodyPartsList)
        {
            numOfRb++;
            velSum += item.rb.velocity;
        }

        avgVel = velSum / numOfRb;
        return avgVel;
    }

    /// <summary>
    /// Normalized value of the difference in actual speed vs goal walking speed.
    /// </summary>
    public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
    {
        //distance between our actual velocity and goal velocity
        var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, TargetWalkingSpeed);

        //return the value on a declining sigmoid shaped curve that decays from 1 to 0
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / TargetWalkingSpeed, 2), 2);
    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(1f);
    }
}
