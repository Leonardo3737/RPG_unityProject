using UnityEngine;

public class Arrow : MonoBehaviour
{
  [SerializeField]
  private float Speed = 50f;
  private Vector3 Velocity;
  private Vector3 StartPosition;

  [field: SerializeField]
  private AudioSource AudioSource;
  [field: SerializeField]
  private AudioClip DefaultImpactSound;
  [field: SerializeField]
  private AudioClip EnemyImpactSound;
  private bool IsLaunched = false;
  private bool IsCollided = false;

  private readonly float LimityTime = 5f;
  private float Timer = 0;


  public void SetTarget(Vector3 target)
  {
    StartPosition = transform.position;
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

    if (Timer > LimityTime)
    {
      Destroy(gameObject);
      return;
    }


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
  void OnTriggerEnter(Collider collision)
  {
    // Parar a física
    if (collision.CompareTag("Enemy") || collision.CompareTag("Arrow") || IsCollided || !IsLaunched)
    {
      return;
    }

    if (collision.CompareTag("EnemyCollider"))
    {
      MakeSound(AudioSource, EnemyImpactSound);
      var enemyStateMachine = collision.GetComponentInParent<EnemyStateMachine>();
      if (enemyStateMachine == null) return;

      Vector3 directionToOrigin = StartPosition - enemyStateMachine.transform.position;

      directionToOrigin.y = 0; // ignora diferença de altura

      enemyStateMachine.transform.rotation = Quaternion.LookRotation(directionToOrigin);

      enemyStateMachine.OnDamage(20, "Damage-1", null);
    }
    else
    {
      MakeSound(AudioSource, DefaultImpactSound);
    }


    GetComponent<Rigidbody>().isKinematic = true;

    //Tornar a flecha filha do objeto atingido (pra "grudar")
    // Define o novo pai
    transform.SetParent(collision.transform, true);


    IsCollided = true;
  }

  public void MakeSound(AudioSource source, AudioClip sound)
	{
		source.PlayOneShot(sound);

		var radius = source.maxDistance;
		var position = transform.position;

		Collider[] listeners = Physics.OverlapSphere(position, radius, LayerMask.GetMask("Enemy"));
		
    foreach (var col in listeners)
		{
			if (!col.TryGetComponent(out EnemyStateMachine enemyStateMachine)) continue;
      enemyStateMachine.OnHeardSound(position);
		}
	}

}
