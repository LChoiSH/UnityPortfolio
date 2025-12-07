using System;
using System.Collections;
using UnityEngine;

namespace UnitSystem
{
    [RequireComponent(typeof(Unit))]
    public class Mover : MonoBehaviour
{
    private Unit unit;
    private Coroutine moveCoroutine;

    public bool IsMoving { get; private set; }
    public float MoveSpeed => unit != null ? unit.Stat.moveSpeed : 1f;

    public event Action<Vector3> onMoveStart;
    public event Action<Vector3> onMoveComplete;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public void SnapTo(Vector3 worldPos)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            IsMoving = false;
        }

        transform.position = worldPos;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition));
    }

    public void Stop()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            IsMoving = false;
        }
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition)
    {
        IsMoving = true;
        onMoveStart?.Invoke(targetPosition);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                MoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
        IsMoving = false;
        moveCoroutine = null;

        onMoveComplete?.Invoke(targetPosition);
    }
}
}
