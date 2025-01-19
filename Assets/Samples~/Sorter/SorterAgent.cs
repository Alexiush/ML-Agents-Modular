using ModularMLAgents.Components;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SorterAgent : ModularAgent
{
    [Range(1, 20)]
    public int DefaultMaxNumTiles;
    private const int _highestTileValue = 20;

    int _numberOfTilesToSpawn;
    int _maxNumberOfTiles;
    public Rigidbody AgentRb;

    public List<NumberTile> NumberTilesList = new List<NumberTile>();

    public List<NumberTile> CurrentlyVisibleTilesList = new List<NumberTile>();
    private List<Transform> AlreadyTouchedList = new List<Transform>();

    private List<int> _usedPositionsList = new List<int>();
    private Vector3 _startingPos;

    GameObject _area;
    EnvironmentParameters _resetParams;

    private int _nextExpectedTileIndex;


    public override void Initialize()
    {
        _area = transform.parent.gameObject;
        _maxNumberOfTiles = _highestTileValue;
        _resetParams = Academy.Instance.EnvironmentParameters;
        AgentRb = GetComponent<Rigidbody>();
        _startingPos = transform.position;

        MaxStep = 5000;

        base.Initialize();
    }

    public override void OnEpisodeBegin()
    {
        _maxNumberOfTiles = (int)_resetParams.GetWithDefault("num_tiles", DefaultMaxNumTiles);

        _numberOfTilesToSpawn = Random.Range(1, _maxNumberOfTiles + 1);
        SelectTilesToShow();
        SetTilePositions();

        transform.position = _startingPos;
        AgentRb.velocity = Vector3.zero;
        AgentRb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("tile"))
        {
            return;
        }
        if (AlreadyTouchedList.Contains(col.transform))
        {
            return;
        }
        if (col.transform.parent != CurrentlyVisibleTilesList[_nextExpectedTileIndex].transform)
        {
            // The Agent Failed
            AddReward(-1);
            EndEpisode();
        }
        else
        {
            // The Agent Succeeded
            AddReward(1);
            var tile = col.gameObject.GetComponentInParent<NumberTile>();
            tile.VisitTile();
            _nextExpectedTileIndex++;

            AlreadyTouchedList.Add(col.transform);

            //We got all of them. Can reset now.
            if (_nextExpectedTileIndex == _numberOfTilesToSpawn)
            {
                EndEpisode();
            }
        }
    }

    void SetTilePositions()
    {

        _usedPositionsList.Clear();

        //Disable all. We will enable the ones selected
        foreach (var item in NumberTilesList)
        {
            item.ResetTile();
            item.gameObject.SetActive(false);
        }


        foreach (var item in CurrentlyVisibleTilesList)
        {
            //Select a rnd spawnAngle
            bool posChosen = false;
            int rndPosIndx = 0;
            while (!posChosen)
            {
                rndPosIndx = Random.Range(0, _highestTileValue);
                if (!_usedPositionsList.Contains(rndPosIndx))
                {
                    _usedPositionsList.Add(rndPosIndx);
                    posChosen = true;
                }
            }
            item.transform.localRotation = Quaternion.Euler(0, rndPosIndx * (360f / _highestTileValue), 0);
            item.gameObject.SetActive(true);
        }
    }

    void SelectTilesToShow()
    {

        CurrentlyVisibleTilesList.Clear();
        AlreadyTouchedList.Clear();

        int numLeft = _numberOfTilesToSpawn;
        while (numLeft > 0)
        {
            int rndInt = Random.Range(0, _highestTileValue);
            var tmp = NumberTilesList[rndInt];
            if (!CurrentlyVisibleTilesList.Contains(tmp))
            {
                CurrentlyVisibleTilesList.Add(tmp);
                numLeft--;
            }
        }

        //Sort Ascending
        CurrentlyVisibleTilesList.Sort((x, y) => x.NumberValue.CompareTo(y.NumberValue));
        _nextExpectedTileIndex = 0;
    }
}
