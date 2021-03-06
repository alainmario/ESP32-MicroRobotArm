﻿// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
//  TransparenceSetter.cs
//  Author: T-Kuhn.
//  Sapporo, September, 2018. Released into the public domain.
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;

namespace RobotArm
{
    public class TransparenceSetter : MonoBehaviour, IAnim
    {
        public bool StartAnim { set { _makeTransparent.Value = value; } }
        ReactiveProperty<bool> _makeTransparent;
        [SerializeField] bool _makeTransparentToggle;
        [SerializeField] bool _makeVisibleToggle;
        [SerializeField] float _tweenDuration = 1f;
        [SerializeField] Renderer[] _linkTargets;
        [SerializeField] Renderer[] _motorTargets;
        [SerializeField] Material _transparentMatLinks;
        [SerializeField] Material _opaqueMatLinks;
        [SerializeField] Material _transparentMatMotors;
        [SerializeField] Material _opaqueMatMotors;
        Tween _transparencyTween;
        float _currentAlpha;
        bool _isTransparent;

        void Start()
        {
            _makeTransparent = new ReactiveProperty<bool>(true);
            _makeTransparent.Subscribe(x => TweenAllTo(x ? 1f : 0f));
        }

        void Update()
        {
            if (_makeVisibleToggle)
            {
                _makeTransparent.Value = true;

                _makeVisibleToggle = false;
            }
            if (_makeTransparentToggle)
            {
                _makeTransparent.Value = false;

                _makeTransparentToggle = false;
            }

            UpdateRenderers(_linkTargets);
            UpdateRenderers(_motorTargets);
        }

        void UpdateRenderers(Renderer[] renderers)
        {
            foreach (var rend in renderers)
            {
                var col = rend.material.GetColor("_Color");
                col.a = _currentAlpha;
                rend.material.SetColor("_Color", col);
            }
        }

        void TweenAllTo(float alpha)
        {
            if (_transparencyTween != null) { _transparencyTween.Kill(); }

            if (!_isTransparent)
            {
                SetAllMaterialsTo(_linkTargets, _transparentMatLinks);
                SetAllMaterialsTo(_motorTargets, _transparentMatMotors);
            }
            else
            {
                SetAllGameObjectsTo(_linkTargets, true);
                SetAllGameObjectsTo(_motorTargets, true);
            }

            _transparencyTween = DOTween.To(() => _currentAlpha, x => _currentAlpha = x, alpha, _tweenDuration)
                .OnComplete(() =>
                {
                    _isTransparent = _currentAlpha < 0.01f;
                    if (!_isTransparent)
                    {
                        SetAllMaterialsTo(_linkTargets, _opaqueMatLinks);
                        SetAllMaterialsTo(_motorTargets, _opaqueMatMotors);
                    }
                    else
                    {
                        SetAllGameObjectsTo(_linkTargets, false);
                        SetAllGameObjectsTo(_motorTargets, false);
                    }
                });
        }

        void SetAllMaterialsTo(Renderer[] renderers, Material m)
        {
            foreach (var rend in renderers)
            {
                rend.material = new Material(m);
                rend.material.SetInt("_ZWrite", 1);
            }
        }

        void SetAllGameObjectsTo(Renderer[] renderers, bool state)
        {
            foreach (var rend in renderers)
            {
                rend.gameObject.SetActive(state);
            }
        }
    }
}