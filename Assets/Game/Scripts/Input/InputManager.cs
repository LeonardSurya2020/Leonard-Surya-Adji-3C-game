using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    #region Parameters
    public Action<Vector2> onMoveInput;
    public Action<bool> onSprintInput;
    public Action onJumpInput;
    public Action onClimbInput;
    public Action onCancelClimb;
    #endregion

    #region Main Functions
    private void Update()
    {
        CheckMovementInput();
        CheckCrouchInput();
        CheckJumpInput();
        CheckChangePOVInput();
        CheckClimbInput();
        CheckGlideInput();
        CheckCancelInput();
        CheckPunchInput();
        CheckSprintInput();
        CheckMainMenuInput();
    }
    #endregion


    #region Input Control Functions
    // Movement (W,A,S,D)
    private void CheckMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 inputAxis = new Vector2(horizontalInput, verticalInput);
        if(onMoveInput != null )
        {
            onMoveInput(inputAxis);
        }
    }


    // Sprint (shift)
    private void CheckSprintInput()
    {
        bool isPressSprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isPressSprintInput)
        {
            Debug.Log("sprinting");
            onSprintInput(true);
        }
        else
        {
            onSprintInput(false);
        }
    }

    // Crouch (left ctrl)
    private void CheckCrouchInput()
    {
        bool isPressCrouchInput = Input.GetKeyDown(KeyCode.LeftControl);
        if (isPressCrouchInput)
        {
            Debug.Log("Crouching");
        }
        else
        {
            Debug.Log("Not Crouching");
        }
    }

    // Jump (space)
    private void CheckJumpInput()
    {
        bool isPressJumpInput = Input.GetKeyDown(KeyCode.Space);

        if(isPressJumpInput)
        {
            onJumpInput();
        }
    }

    // Change POV (Q)
    private void CheckChangePOVInput()
    {
        bool isPressChangePOVInput = Input.GetKeyDown(KeyCode.Q);

        if (isPressChangePOVInput)
        {
            Debug.Log("Change POV");
        }
    }

    // Climb (E)
    private void CheckClimbInput()
    {
        bool isPressClimbInput = Input.GetKeyDown(KeyCode.E);
        if (isPressClimbInput)
        {
            onClimbInput();
        }
    }

    // Glide (G)
    private void CheckGlideInput()
    {
        bool isPressGlideInput = Input.GetKeyDown(KeyCode.G);
        if (isPressGlideInput)
        {
            Debug.Log("Gliding");
        }
    }

    // cancel glide/climb (C)
    private void CheckCancelInput()
    {
        bool isPressCancelInput = Input.GetKeyDown(KeyCode.C);

        if(isPressCancelInput)
        {
            if(onCancelClimb != null)
            {
                onCancelClimb();
            }
            
        }
    }

    // Punch (left mouse click)
    private void CheckPunchInput()
    {
        bool isPressPunchInput = Input.GetKeyDown(KeyCode.Mouse0);

        if(isPressPunchInput)
        {
            Debug.Log("Punching");
        }
    }

    // Back to main menu (esc)
    private void CheckMainMenuInput()
    {
        bool isPressMainMenuInput = Input.GetKeyDown(KeyCode.Escape);

        if(isPressMainMenuInput)
        {
            Debug.Log("Open main menu");
        }
    }

    #endregion


}
