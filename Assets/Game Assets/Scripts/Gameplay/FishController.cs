using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EP.Utils.Core;

public class FishController : EPBehaviour
{
    [SerializeField] ItemData m_itemData;
    [SerializeField] float m_speed;

    Bounds m_movementBounds;

    Coroutine m_movementCo;

    public void Setup(Bounds bounds)
    {
        m_movementBounds = bounds;
        StartMovement();
    }

    public void StartMovement()
    {
        if(m_movementCo != null)
            StopCoroutine(m_movementCo);

        m_movementCo = StartCoroutine(Movement());
    }

    IEnumerator Movement()
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
