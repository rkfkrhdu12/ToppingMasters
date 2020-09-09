﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class CameraManager : MonoBehaviour
{
    private Transform _playerCharTransform;
    private Transform _pivotTransform;

    private RefData _mouseSensitivity;

    private Vector3 _curVelocity = Vector3.zero;
    public bool _isInit = false;
    public bool _isOption = true;

    private WaitForSeconds _waitTime = new WaitForSeconds(.25f);

    public void SetMouseSensitivity(in RefData Data)
    {
        _mouseSensitivity = Data;
    }

    IEnumerator SetPlayerCharacter()
    {
        while (_playerCharTransform == null)
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.PlayerCharacter != null)
                {
                    _playerCharTransform = GameManager.Instance.PlayerCharacter.transform;
                }
            }

            yield return _waitTime;
        }

        _isInit = true;
        yield return null;
    }

    void Start()
    {
        _isInit = false;
        _isOption = true;

        StartCoroutine(SetPlayerCharacter());

        _pivotTransform = transform;
    }

    public float smoothTime = .1f;

    void LateUpdate()
    {
        if (!_isOption || !_isInit) { return; }

        // Position Update
        _pivotTransform.position = Vector3.SmoothDamp(_pivotTransform.position, _playerCharTransform.position, ref _curVelocity, smoothTime);

        // Rotation Update
        float horizontal = Input.GetAxis("Mouse X");

        _pivotTransform.localEulerAngles += new Vector3(0, horizontal * _mouseSensitivity._Value * Time.deltaTime, 0);
    }
}
