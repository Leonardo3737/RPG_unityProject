using UnityEngine;

public class Arrow : MonoBehaviour
{
  [SerializeField] private float Speed = 50f;
  private Vector3 Velocity;
  private bool IsLaunched = false;
  private bool IsCollided = false;

  private readonly float LimityTime = 5f;
  private float Timer = 0;


  public void SetTarget(Vector3 target)
  {
    // A mesma direção que MoveTowards usaria
    Vector3 direction = (target - transform.position).normalized;

    Velocity = direction * Speed;
    IsLaunched = true;

    // Rotação inicial
    transform.rotation = Quaternion.LookRotation(direction);
  }

  void Update()
  {
    if (!IsLaunched) return;

    Timer += Time.deltaTime;

    /* if (Timer > LimityTime)
    {
      Destroy(gameObject);
      return;
    } */


    if (IsCollided)
    {
      return;
    }
    // Aplica gravidade
    Velocity += Physics.gravity * Time.deltaTime;

    // Move a flecha
    transform.position += Velocity * Time.deltaTime;

    // Gira a flecha para apontar na direção atual
    if (Velocity.sqrMagnitude > 0.01f)
      transform.rotation = Quaternion.LookRotation(Velocity);
  }
  void OnCollisionEnter(Collision collision)
  {
    // Parar a física
    GetComponent<Rigidbody>().isKinematic = true;

    // Tornar a flecha filha do objeto atingido (pra "grudar")
    //transform.parent = collision.transform;

    IsCollided = true;
    Debug.Log("A collider has made contact with the DoorObject Collider");
  }

}
