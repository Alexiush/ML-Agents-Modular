using ModularMLAgents.Components;
using Unity.MLAgents;
using UnityEngine;

public class HallwayAgent : ModularAgent
{
    public GameObject Ground;
    public GameObject Area;
    public GameObject CubeGoal;
    public GameObject SphereGoal;
    public GameObject Cube;
    public GameObject Sphere;
    public bool useVectorObs;
    public Rigidbody AgentRb;

    public HallwaySettings HallwaySettings;
    int m_Selection;
    StatsRecorder m_statsRecorder;

    public override void Initialize()
    {
        HallwaySettings = FindObjectOfType<HallwaySettings>();
        AgentRb = GetComponent<Rigidbody>();
        m_statsRecorder = Academy.Instance.StatsRecorder;
        MaxStep = 3000;

        base.Initialize();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("cube_goal") || col.gameObject.CompareTag("sphere_goal"))
        {
            if ((m_Selection == 0 && col.gameObject.CompareTag("cube_goal")) ||
                (m_Selection == 1 && col.gameObject.CompareTag("sphere_goal")))
            {
                SetReward(1f);
                m_statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
            }
            else
            {
                SetReward(-0.1f);
                m_statsRecorder.Add("Goal/Wrong", 1, StatAggregationMethod.Sum);
            }
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        var agentOffset = -15f;
        var blockOffset = 0f;
        m_Selection = Random.Range(0, 2);
        if (m_Selection == 0)
        {
            Cube.transform.position =
                new Vector3(0f + Random.Range(-3f, 3f), 2f, blockOffset + Random.Range(-5f, 5f))
                + Ground.transform.position;
            Sphere.transform.position =
                new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                + Ground.transform.position;
        }
        else
        {
            Cube.transform.position =
                new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                + Ground.transform.position;
            Sphere.transform.position =
                new Vector3(0f, 2f, blockOffset + Random.Range(-5f, 5f))
                + Ground.transform.position;
        }

        transform.position = new Vector3(0f + Random.Range(-3f, 3f),
            1f, agentOffset + Random.Range(-5f, 5f))
            + Ground.transform.position;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        AgentRb.velocity *= 0f;

        var goalPos = Random.Range(0, 2);
        if (goalPos == 0)
        {
            CubeGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + Area.transform.position;
            SphereGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + Area.transform.position;
        }
        else
        {
            SphereGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + Area.transform.position;
            CubeGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + Area.transform.position;
        }
        m_statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
    }
}
