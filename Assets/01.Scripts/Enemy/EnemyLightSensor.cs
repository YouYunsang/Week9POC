using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public sealed class EnemyLightSensor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float _exposureToSuspicious = 1.0f;
    [SerializeField] private float _exposureIncreasePerSecond = 1.0f;
    [SerializeField] private float _exposureDecreasePerSecond = 0.75f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask _blockingLayerMask;
    [SerializeField] private Transform _sensorPoint;

    private readonly List<PlayerFlashlight> _lightSources = new List<PlayerFlashlight>();

    private EnemyController _enemyController;
    private float _currentExposure;
    private bool _isSuspiciousByLight;

    public float CurrentExposure => _currentExposure;

    private void Awake()
    {
        // 같은 GameObject의 EnemyController를 캐싱한다.
        _enemyController = GetComponent<EnemyController>();

        if (_sensorPoint == null)
        {
            // 별도 센서 위치가 없으면 적 오브젝트 위치를 기준으로 한다.
            _sensorPoint = transform;
        }
    }

    private void Update()
    {
        bool isExposedToLight = IsExposedToAnyLight();

        if (isExposedToLight)
        {
            // 빛에 노출되면 감지 누적치를 증가시킨다.
            _currentExposure += _exposureIncreasePerSecond * Time.deltaTime;
        }
        else
        {
            // 빛이 꺼졌거나 벽에 막히면 감지 누적치를 천천히 감소시킨다.
            _currentExposure -= _exposureDecreasePerSecond * Time.deltaTime;
        }

        _currentExposure = Mathf.Clamp(_currentExposure, 0.0f, _exposureToSuspicious);

        if (_currentExposure >= _exposureToSuspicious)
        {
            TryEnterChaseByLight();
            return;
        }

        TryReturnToPatrolAfterLightLost();
    }

    private void TryEnterChaseByLight()
    {
        if (_enemyController.CurrentState != EnemyState.Patrol)
        {
            return;
        }

        if(_enemyController.CurrentState == EnemyState.Stunned)
            return;

        // 감지 누적치가 임계값에 도달하면 의심 상태로 전환한다.
        _enemyController.BeginChase(EnemyMoveMode.Sprint);

        Debug.Log("적 잠수부가 플래시라이트 빛을 감지해 의심 상태가 되었습니다.");
    }

    private void TryReturnToPatrolAfterLightLost()
    {
        if (!_isSuspiciousByLight)
        {
            return;
        }

        if (_currentExposure > 0.0f)
        {
            return;
        }

        if (_enemyController.CurrentState != EnemyState.Suspicious)
        {
            _isSuspiciousByLight = false;
            return;
        }

        // 빛으로 인해 의심 상태가 되었고, 노출도가 완전히 사라졌다면 순찰로 복귀한다.
        _isSuspiciousByLight = false;
        _enemyController.ChangeState(EnemyState.Patrol);

        Debug.Log("빛 노출이 사라져 적이 순찰 상태로 복귀했습니다.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerFlashlight flashlight = other.GetComponentInParent<PlayerFlashlight>();

        if (flashlight == null)
        {
            return;
        }

        if (_lightSources.Contains(flashlight))
        {
            return;
        }

        // 플래시라이트 Cone Trigger 안에 들어온 빛 소스를 등록한다.
        _lightSources.Add(flashlight);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerFlashlight flashlight = other.GetComponentInParent<PlayerFlashlight>();

        if (flashlight == null)
        {
            return;
        }

        // 플래시라이트 Cone Trigger 밖으로 나가면 등록을 해제한다.
        _lightSources.Remove(flashlight);
    }

    private bool IsExposedToAnyLight()
    {
        for (int i = _lightSources.Count - 1; i >= 0; i--)
        {
            PlayerFlashlight flashlight = _lightSources[i];

            if (flashlight == null)
            {
                _lightSources.RemoveAt(i);
                continue;
            }

            if (!flashlight.IsOn)
            {
                continue;
            }

            if (!HasLineOfSightFromLight(flashlight))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool HasLineOfSightFromLight(PlayerFlashlight flashlight)
    {
        Vector2 start = flashlight.OriginPosition;
        Vector2 end = _sensorPoint.position;
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            return true;
        }

        // 플래시라이트와 적 사이에 벽/문이 있는지 Raycast로 확인한다.
        RaycastHit2D hit = Physics2D.Raycast(
            start,
            direction.normalized,
            distance,
            _blockingLayerMask);

        return hit.collider == null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 감지 임계값과 누적/감쇠 속도는 음수가 되지 않게 제한한다.
        _exposureToSuspicious = Mathf.Max(0.01f, _exposureToSuspicious);
        _exposureIncreasePerSecond = Mathf.Max(0.0f, _exposureIncreasePerSecond);
        _exposureDecreasePerSecond = Mathf.Max(0.0f, _exposureDecreasePerSecond);
    }

    private void OnDrawGizmos()
    {
        Transform sensor = _sensorPoint != null ? _sensorPoint : transform;

        // 빛 감지 센서 위치.
        Gizmos.color = new Color(0.4f, 0.7f, 1.0f, 0.9f);
        Gizmos.DrawWireSphere(sensor.position, 0.18f);

        if (_lightSources == null)
        {
            return;
        }

        for (int i = 0; i < _lightSources.Count; i++)
        {
            PlayerFlashlight flashlight = _lightSources[i];

            if (flashlight == null)
            {
                continue;
            }

            if (flashlight.IsOn)
            {
                // 현재 빛 후보와 적 센서 사이의 감지선.
                Gizmos.color = new Color(0.4f, 0.7f, 1.0f, 0.8f);
            }
            else
            {
                // 꺼진 플래시라이트 후보.
                Gizmos.color = new Color(0.25f, 0.25f, 0.25f, 0.45f);
            }

            Gizmos.DrawLine(flashlight.OriginPosition, sensor.position);
        }
    }
#endif
}