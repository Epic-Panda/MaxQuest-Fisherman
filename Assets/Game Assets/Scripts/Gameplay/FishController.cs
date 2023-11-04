using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;

public class FishController : EPBehaviour
{
    [SerializeField] ItemData m_itemData;
    [SerializeField] float m_speed;
    [SerializeField] Vector3 m_size;

    Bounds m_movementBounds;

    Coroutine m_movementCo;

    public ItemData ItemData { get { return m_itemData; } }

    public void Setup(Bounds bounds)
    {
        m_movementBounds = bounds;
        m_movementBounds.extents -= m_size / 2;

        m_movementCo = StartCoroutine(RandomMovement());
    }

    public void Respawn()
    {
        if(m_movementCo != null)
            StopCoroutine(m_movementCo);

        SelfTransform.position = new Vector3(Random.Range(m_movementBounds.min.x, m_movementBounds.max.x), Random.Range(m_movementBounds.min.y, m_movementBounds.max.y), m_movementBounds.center.z);
        gameObject.SetActive(true);

        m_movementCo = StartCoroutine(RandomMovement());
    }

    public void MoveToPosition(Vector3 endPoint, float movementDuration)
    {
        if(m_movementCo != null)
            StopCoroutine(m_movementCo);

        m_movementCo = StartCoroutine(Movement(endPoint, movementDuration));
    }

    IEnumerator Movement(Vector3 endPoint, float movementDuration)
    {
        Vector3 startPoint = SelfTransform.position;

        SelfTransform.rotation = startPoint.x > endPoint.x ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        float elapsed = 0;

        while(movementDuration > elapsed)
        {
            elapsed += Time.deltaTime;
            SelfTransform.position = Vector3.Lerp(startPoint, endPoint, elapsed / movementDuration);
            yield return null;
        }
    }

    IEnumerator RandomMovement()
    {
        float elapsed;
        float totalTime;

        Vector3 startPoint;
        Vector3 endPoint;

        while(true)
        {
            startPoint = SelfTransform.position;
            endPoint = GetNextPosition();

            SelfTransform.rotation = startPoint.x > endPoint.x ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

            totalTime = Vector3.Distance(startPoint, endPoint) / m_speed;
            elapsed = 0;

            while(totalTime > elapsed)
            {
                elapsed += Time.deltaTime;
                SelfTransform.position = Vector3.Lerp(startPoint, endPoint, elapsed / totalTime);
                yield return null;
            }
        }
    }

    Vector3 GetNextPosition()
    {
        // check if fish is on the left side
        if(m_movementBounds.center.x > SelfTransform.position.x)
            return new Vector3(Random.Range(m_movementBounds.center.x, m_movementBounds.max.x), Random.Range(m_movementBounds.center.y, m_movementBounds.max.y), m_movementBounds.center.z);

        return new Vector3(Random.Range(m_movementBounds.min.x, m_movementBounds.center.x), Random.Range(m_movementBounds.min.y, m_movementBounds.center.y), m_movementBounds.center.z);
    }
}
