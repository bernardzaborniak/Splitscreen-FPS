﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;


public class PlayerController : MonoBehaviour
{
    #region Fields
    public Camera topdownCam;
    public Camera fpCam;

    enum PlayerControlMode
    {
        TopDown,
        FirstPerson,
        RTS //we can move around the world as a dead ghost - also RTS Mode
    }

    PlayerControlMode controlMode;

    public UnityEvent onDieEvent;

    public PlayerMovement playerMovement;
    public GameEntity playerEntity;
    public Transform deadPlayerGhostTransform;
    public PlayerMovementRTS deadPlayerMovement;
    public PlayerInput playerInput;
    public EC_PlayerWeaponSystem weaponSystem;
    public InteractableShower interactableShower;

    public Vector3 desiredLookVektor;
    Vector3 movementVector;

    Vector3 movementInputVector;
    Vector2 lookInputVector; // for controller
    Vector2 lookInputVectorUsed;
    //Vector2 lookInputVectorLastFrame; //for better controls
    Vector2 currentMousePosition; //for keyboard & mouse
    Vector2 mouseDelta;
    //Vector2 mousePositionLastFrame;

    bool weaponPressed = false;
    int pressedWeaponID;

    bool interacting = false;
    public bool looseWeaponOnDeactivate = true;


    public float xSensitivity = 0.3f;
    public float ySensitivity = 0.1f;

    #endregion

    #region Controls

    public void OnMovement(InputValue value)
    {
        movementInputVector = value.Get<Vector2>();
    }

    public void OnW1Press()
    {
        if(controlMode == PlayerControlMode.TopDown) weaponSystem.UseWeaponStart(0);
        pressedWeaponID = 0;
        weaponPressed = true;
        weaponSystem.UseWeaponStart(pressedWeaponID);
    }

    public void OnW1Release()
    {
        weaponPressed = false;
        weaponSystem.UseWeaponEnd(pressedWeaponID);
    }

    public void OnReload()
    {
        weaponSystem.ReloadWeapon();
    }

    public void OnInteractStart()
    {
        interacting = true;
        interactableShower.StartInteract();
    }

    public void OnInteractStop()
    {
        interacting = false;
        interactableShower.StopInteract();
    }

    public void OnJump()
    {
        playerMovement.Jump();
    }

    //we dash in the direction our wasd or left stick is facing, if their diection is null, we dash in the current look direction
    public void OnDash()
    {

        if (movementVector != new Vector3(0, 0, 0))
        {
            playerMovement.Dash(movementVector.normalized);
            //Debug.Log("dash no move");
        }
        else
        {
            playerMovement.Dash(desiredLookVektor.normalized);
            //Debug.Log("dash MMove");
        }
    }

    void OnRotateTowards(InputValue value)
    {
        //Debug.Log("control sheme: " + playerInput.controlScheme);
        if(playerInput.controlScheme == "Gamepad")
        {
            lookInputVector = value.Get<Vector2>();
            //Debug.Log("gamepad");
            if(lookInputVector != new Vector2(0,0))
            {
                lookInputVectorUsed = lookInputVector;
                //lookInputVector = lookInputVectorLastFrame;
            }
        }
        else
        {      
            if(controlMode == PlayerControlMode.TopDown)
            {
                currentMousePosition = value.Get<Vector2>();

            }

        }
    }


    void OnNextWeapon(InputValue value)
    {
        if (playerInput.controlScheme == "Gamepad")
        {
            weaponSystem.SelectNextWeapon();
        }
        else
        {
            if (value.Get<float>() > 0)
            {
                weaponSystem.SelectNextWeapon();
            }
        }
    }

    void OnPreviousWeapon(InputValue value)
    {
        if (playerInput.controlScheme == "Gamepad")
        {
            weaponSystem.SelectPreviousWeapon();
        }
        else
        {
            if (value.Get<float>() < 0)
            {
                weaponSystem.SelectPreviousWeapon();
            }
        }
    }

    void OnSelectWeapon1()
    {
        weaponSystem.ChangeWeapon(0);
    }

    void OnSelectWeapon2()
    {
        weaponSystem.ChangeWeapon(1);
    }

    void OnSelectWeapon3()
    {
        weaponSystem.ChangeWeapon(2);
    }

    public void OnLookAroundFP(InputValue value)
    {
        mouseDelta = value.Get<Vector2>();
    }




    #endregion

    void Start()
    {
        desiredLookVektor = playerEntity.transform.forward;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToogleCameraMode();
        }

        //get movement
        float hor = movementInputVector.x;
        float ver = movementInputVector.y;

        #region determine movementVector

        if (controlMode == PlayerControlMode.FirstPerson)
        {
            movementVector = playerEntity.transform.right*hor + playerEntity.transform.forward*ver;

           
        }
        else
        {
            Vector3 camRight = (topdownCam.transform.right).normalized;
            Vector3 horV = camRight * hor;
            Vector3 verV = new Vector3(-camRight.z, 0f, camRight.x) * ver;
            movementVector = horV + verV;
        }

        #endregion

        #region determine desiredLookDirectionVector

        if(controlMode == PlayerControlMode.TopDown)
        {
            //rotate towards

            if (playerInput.controlScheme == "Gamepad")
            {
                if (lookInputVector != new Vector2(0, 0))
                {
                    desiredLookVektor = Quaternion.Euler(0, topdownCam.transform.localEulerAngles.y, 0) * new Vector3(lookInputVectorUsed.x, 0f, lookInputVectorUsed.y);
                }
                else
                {
                    if (movementVector != new Vector3(0, 0, 0))
                    {
                        desiredLookVektor = movementVector;
                    }
                }


            }
            else
            {
                Vector2 direction = new Vector2(Screen.width / 2, Screen.height / 2) - currentMousePosition;
                desiredLookVektor = Quaternion.Euler(0, topdownCam.transform.localEulerAngles.y + 180, 0) * new Vector3(direction.x, 0f, direction.y);
            }

           
        }
        else if (controlMode == PlayerControlMode.RTS)
        {
            deadPlayerMovement.UpdateMovement(movementVector);
        }
        else if (controlMode == PlayerControlMode.FirstPerson)
        {
            if (playerInput.controlScheme == "Gamepad")
            {

                //TODO

                /*if (lookInputVector != new Vector2(0, 0))
                {
                    currentLookVector = Quaternion.Euler(0, topdownCam.transform.localEulerAngles.y, 0) * new Vector3(lookInputVectorUsed.x, 0f, lookInputVectorUsed.y);
                }
                else
                {
                    if (movementVector != new Vector3(0, 0, 0))
                    {
                        currentLookVector = movementVector;
                    }
                }*/

            }
            else
            {
                Vector3 desiredLookVektorHorizontal = (Quaternion.AngleAxis(mouseDelta.x * xSensitivity, playerEntity.transform.up) * playerEntity.transform.forward).normalized;
                Vector3 desiredLookVektorVertical = (Quaternion.AngleAxis(-mouseDelta.y * ySensitivity, fpCam.transform.right) * fpCam.transform.forward).normalized;
                desiredLookVektor = new Vector3(desiredLookVektorHorizontal.x, desiredLookVektorVertical.y, desiredLookVektorHorizontal.z);
            }

            //first person mode gets rotation applied directly in update, not in fixedUpdate
            playerMovement.InstantRotateTo(desiredLookVektor);

        }

        #endregion

        //weapon
        if (weaponPressed)
        {
            weaponSystem.UseWeaponHold(pressedWeaponID);
        }

        if (interacting)
        {
            interactableShower.HoldInteract();
        }
    }

    private void FixedUpdate()
    {
        //top down gets player rotation applied only in fixedUpdate
        if (controlMode == PlayerControlMode.TopDown)
        {
            playerMovement.SmoothRotateTo(desiredLookVektor);
        }

        playerMovement.UpdateMovement(movementVector);     
    }

    public void ActivatePlayer()
    {
        playerEntity.GetComponent<EC_Health>().ResetHealth();
        deadPlayerGhostTransform.gameObject.SetActive(false);
        playerEntity.gameObject.SetActive(true);
        topdownCam.GetComponent<SmoothCameraFollow>().target = playerEntity.transform;

        controlMode = PlayerControlMode.TopDown;

    }

    public void DeactivatePlayer()
    {
        if (looseWeaponOnDeactivate) playerEntity.GetComponent<EC_PlayerWeaponSystem>().ResetWeapons();

        playerEntity.gameObject.SetActive(false);
        Vector3 rtsCamPosition = playerEntity.transform.position;
        rtsCamPosition.y = 0;
        deadPlayerGhostTransform.position = rtsCamPosition;
        deadPlayerGhostTransform.gameObject.SetActive(true);
        topdownCam.GetComponent<SmoothCameraFollow>().target = deadPlayerGhostTransform;


        controlMode = PlayerControlMode.RTS;

    }

    public void ToogleCameraMode()
    {
        if (controlMode == PlayerControlMode.TopDown)
        {
            controlMode = PlayerControlMode.FirstPerson;
            Cursor.lockState = CursorLockMode.Locked;
            topdownCam.gameObject.SetActive(false);
            fpCam.gameObject.SetActive(true);
        }
        else if (controlMode == PlayerControlMode.FirstPerson)
        {
            controlMode = PlayerControlMode.TopDown;
            Cursor.lockState = CursorLockMode.None;
            topdownCam.gameObject.SetActive(true);
            fpCam.gameObject.SetActive(false);
        }
    }

    public void TeleportPlayer(Vector3 position)
    {
        playerEntity.transform.position = position;
        topdownCam.GetComponent<SmoothCameraFollow>().TeleportToDesiredPosition();
    }

    public void OnDie()
    {
        DeactivatePlayer();
        onDieEvent.Invoke();
    }
}
