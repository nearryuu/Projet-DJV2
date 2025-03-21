﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
//--------------------------------------------------------------------
//Base class for all entity controllers.
//Uses a ControlledCollider for movement, and an AbilityModuleManager for movement modules
//Implemented further in GroundedCharacterController and HoverCharacterController
//--------------------------------------------------------------------
public abstract class CharacterControllerBase : MovableObject
{
    protected DirectionInput m_MovementInput;
    protected PlayerInput m_PlayerInput;
    protected bool m_MovementIsLocked;
    protected bool lookingRight;

    public int MaxHP;
    public int currentHP;

    [SerializeField] protected ControlledCollider m_ControlledCollider;
	[SerializeField] protected AbilityModuleManager m_AbilityManager;
    [SerializeField] protected GameObject m_object;
    [SerializeField] protected GameObject HealthBar;
    
    private HealthBarFade healthBarScript;
    void Awake()
    {
        currentHP = MaxHP;
        if (m_ControlledCollider == null)
        {
            Debug.LogError("Controlled collider not set up");
            return;
        }
        if (m_AbilityManager != null)
        {
            m_AbilityManager.InitAllModules(this);
        }
        
        healthBarScript = HealthBar.GetComponent<HealthBarFade>();
    }

    protected override void FixedUpdate ()
    { 
        if (m_ControlledCollider == null)
        {
            return;
        }
        if (m_MovementIsLocked)
        {
            return;
        }
        m_ControlledCollider.UpdateContextInfo();
        UpdateController();
        if (m_AbilityManager != null)
        {
            m_AbilityManager.UpdateBestApplicableModule();
            m_AbilityManager.UpdateInactiveModules();
            if (m_AbilityManager.IsAbilityModuleRunning())
            {
                m_AbilityManager.FixedUpdateCurrentModule();
            }
            else
            {
                DefaultUpdateMovement();
            }
            m_AbilityManager.PostFixedUpdateModuleSelection();
        }
        else
        { 
            DefaultUpdateMovement();
        }
        GravityVector = Vector2.zero;
    }

    protected virtual void UpdateController()
    {
    }

    public bool IsLookingRight(){
        return lookingRight;
    }
    public ControlledCollider GetCollider()
    {
        return m_ControlledCollider;
    }
    public AbilityModuleManager GetAbilityModuleManager()
    {
        return m_AbilityManager;
    }

    public void SetPosition(Vector3 a_Position)
    {
        if (m_ControlledCollider != null)
        {
            m_ControlledCollider.SetPosition(a_Position);
            m_ControlledCollider.UpdateContextInfo();
        }
    }

    public void SetRotation(Quaternion a_Rotation)
    {
        if (m_ControlledCollider != null)
        {
            m_ControlledCollider.SetRotation(a_Rotation);
            m_ControlledCollider.UpdateContextInfo();
        }
    }

    //Lock player in place
    public void LockMovement(bool a_Lock)
    {
        m_MovementIsLocked = a_Lock;
    }
    public bool IsMovementLocked()
    {
        return m_MovementIsLocked;
    }

    public void SpawnAndResetAtPosition(Vector3 a_Position)
    {
        if (m_AbilityManager != null)
        {
            m_AbilityManager.ForceExitModules();
        }
        if (m_ControlledCollider != null)
        {
            m_ControlledCollider.SetVelocity(Vector2.zero);
            m_ControlledCollider.SetPosition(a_Position);
            m_ControlledCollider.UpdateContextInfo();
            m_ControlledCollider.ClearColPoints();
        }
    }

    //Set inputs (by PlayerInput)
    public virtual void SetPlayerInput(PlayerInput a_PlayerInput)
    {
        m_PlayerInput = a_PlayerInput;
        if (a_PlayerInput.GetDirectionInput("Move") != null)
        {
            m_MovementInput = a_PlayerInput.GetDirectionInput("Move");
        }
        else
        {
            Debug.LogError("Move input not set up in character input");
        }
        
    }

    //Get inputs from player
    public PlayerInput GetPlayerInput()
    {
        return m_PlayerInput;
    }
    public Vector2 GetInputMovement()
    {
        if (m_MovementInput != null)
        {
            return m_MovementInput.m_ClampedInput;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public void TakeDamage(int damage){
        currentHP -= damage;
        StartCoroutine(healthBarScript.PerteHp((float)currentHP, (float)MaxHP));
        if (currentHP<=0){
            StartCoroutine(CharacterDies());
        }
    }

    protected IEnumerator CharacterDies(){
        Destroy(m_object);
        yield return new WaitForSecondsRealtime(4);
        SceneManager.LoadScene("MainMenuScene");
    }

    public string GetCurrentSpriteState()
    {
        if (m_AbilityManager != null)
        {
            if (m_AbilityManager.GetCurrentModule() != null)
            {
                return m_AbilityManager.GetCurrentModule().GetSpriteState();
            }
        }
        return GetCurrentSpriteStateForDefault();
    }
	//Used for ability modules to specify an "up" direction for whatever state they might be in
	public Vector2 GetCurrentVisualUp()
	{
		if (m_AbilityManager != null)
		{
			if (m_AbilityManager.GetCurrentModule() != null)
			{
				return m_AbilityManager.GetCurrentModule().GetCurrentVisualUp();
			}
		}
		return Vector2.up;
	}

    protected virtual string GetCurrentSpriteStateForDefault()
    {
        return "";
    }
}
