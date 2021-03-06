﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class InGameManager : MonoBehaviour
{
    public CameraManager CameraManager;
    public Canvas Canvas;

    [SerializeField]
    private ScrollController _mouseSensitivityOptionScroll;

    public List<PlayerController> PlayerCharacters = new List<PlayerController>();

    [SerializeField]
    private TeamManager _teamMgr = null;

    [SerializeField]
    private Timer _Timer = null;

    [SerializeField]
    private ItemManager _itemMgr = null;

    delegate void GameSequence();
    Queue<GameSequence> GameSequences = new Queue<GameSequence>();

    [SerializeField]
    TartSystemManager _tartMgr; 

    [SerializeField]
    int _score = -1;

    [SerializeField]
    bool _isTimeOut = false;
    public bool IsTimeOut { get { return _isTimeOut; } }

    PhotonView _pView;

    [SerializeField]
    private GameObject[] _playerSpawnPoints = null;

    public void SetScore(int score)
    {
        if (_score == -1)
        {
            _score = score;

            // Score Sequence 동작

        }
    }

    // Timer.cs 에서 호출
    public void OnTimeOut()
    {
        _isTimeOut = true;
    }

    public void AddPlayer(PlayerController pCtrl)
    {
        if (PlayerCharacters.Contains(pCtrl)) { return; }

        for (int i = 0; i < PlayerCharacters.Count; ++i)
        {
            if (PlayerCharacters[i] == null)
            {
                PlayerCharacters.Remove(PlayerCharacters[i]);
            }
        }

        PlayerCharacters.Add(pCtrl);
        _teamMgr.Register(pCtrl);

        int curPlayerCharCount = PlayerCharacters.Count - 1;

        PlayerCharacters[curPlayerCharCount].transform.position 
            = _playerSpawnPoints[curPlayerCharCount].transform.position;

        LogManager.Log("AddPlayer : " + pCtrl.gameObject.GetPhotonView().name);

        pCtrl.Init();
    }

    private void OnStart()
    {
        _isTimeOut = false;
    }

    private void Awake()
    {
        GameManager.Instance.InGameManager = this;

        _mouseSensitivityOptionScroll.Init();

        CameraManager.SetMouseSensitivity(in _mouseSensitivityOptionScroll.Data);

        GameManager.Instance.CurState = GameManager.eSceneState.InGame;

        GameSequences.Enqueue(new GameSequence(PlayerAllIn));
        GameSequences.Enqueue(new GameSequence(OnUI));
        GameSequences.Enqueue(new GameSequence(GameTimeOut));

        _pView = gameObject.GetPhotonView();
    }

    private void Update()
    {
        if(GameSequences.Count == 0) { return; }

        (GameSequences.Peek())();
    }

    [SerializeField]
    bool _isDebugMode = false;

    void PlayerAllIn()
    {
        if (PlayerCharacters.Count == 4 || _isDebugMode)
        {
            // 예외처리

            for (int i = 0; i < PlayerCharacters.Count; ++i)
            {
                if (PlayerCharacters[i] != null)
                    PlayerCharacters[i].transform.position = _playerSpawnPoints[i].transform.position;
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;

            _Timer.OnStart();

            if (PhotonNetwork.IsMasterClient)
                _tartMgr.RandomChoiceOfTart();

            GameSequences.Dequeue();
        }
    }

    [SerializeField]
    private GameObject _ui = null;
    void OnUI()
    {
        _ui.SetActive(true);

        GameSequences.Dequeue();
    }

    void GameTimeOut()
    {
        if (_isTimeOut)
        {
            // 예외처리

            LogManager.Log("Time Out");
            // 타르트 결과 산정
        }
    }

    public void OnFever()
    {

        LogManager.Log("Fever Time !");

    }

    public void OnItemEvent()
    {
        if (!_pView.IsMine) { return; }
        if (_itemMgr == null) { _itemMgr = GetComponent<ItemManager>(); if (_itemMgr == null) return; }

        _itemMgr.Spawn();
    }
}
