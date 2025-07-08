using UnityEngine;

public class Arrow : MonoBehaviour
{
  [field: SerializeField]
  private float Speed;

  public Vector3 m_target;

  void Update()
  {
    float step = Speed * Time.deltaTime;
    if (m_target != null)
    {
      transform.position = Vector3.MoveTowards(transform.position, m_target, step);
    }
  }

  public void SetTarget(Vector3 target)
  {
    m_target = target;
  }
}