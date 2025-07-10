using Unity.Cinemachine;
using UnityEngine;

public class PlayerFreeLookState : PlayerBaseState
{



  public PlayerFreeLookState(PlayerStateMachine stateMachine) : base(stateMachine, StatesType.FREELOOK) { }

  public override void Enter()
  {

    sm.Animator.CrossFadeInFixedTime(FreeLookBlendTree, 0.1f, sm.CurrentLayer);

    float targetSpeed = WasOnTheMove
    ? sm.InputHandler.InputMovement.magnitude
    : 0f;

    sm.Animator.SetFloat(FreeLookSpeed, targetSpeed);

    // SEMPRE HABILITAR CONTROLE DA CAMERA COM MOUSE
    EnableCameraInputController();

    PositionCameraTarget();
  }

  public override void Update(float deltaTime)
  {
    if (sm.InputHandler.InputMovement == Vector2.zero && WasOnTheMove)
    {
      WasOnTheMove = false;
    }

    // SEMPRE INICIAR COM FOCO DA CAMERA NA CABEÇA DO JOGADOR
    //PositionCameraTarget(deltaTime);

    FreeLookMovement(deltaTime);
  }

  public override void Exit()
  {

  }

  public override bool CanPerformAction()
  {
    return true;
  }
}
