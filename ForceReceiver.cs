using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField]
    private CharacterController Controller;

    public float VerticalVelocity;

    public Vector3 Movement => Vector3.up * VerticalVelocity;

    void Update()
    {
        if (VerticalVelocity < 0 && Controller.isGrounded)
        {
            VerticalVelocity = Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            VerticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }
}
