using System.Collections;
using UnityEngine;

public class PlayerCharacter : Character
{
    public PlayerController pc;

    public float instability = 0f;
    private Coroutine instabilityCR;
    private bool pauseInstabilityDecay = false;

    public delegate void InstabilityDelegate(float instabilityValue);
    public static event InstabilityDelegate OnInstabilityChanged;

    private void Awake()
    {
        OnInstabilityChanged += UI_Manager.Instance.UpdateInstability;
        Debug.Log("OnInstabilityChanged Event Added!");
    }

    private void OnDestroy()
    {
        OnInstabilityChanged -= UI_Manager.Instance.UpdateInstability;
        Debug.Log("OnInstabilityChanged Event Removed!");
    }   
        
    public override void ProcessAbsorbEffect(I_Absorbable target) 
    { 
        AddInstability(target.GetInstabilityValue());
        OnInstabilityChanged.Invoke(instability);

        target.Absorb(this);
    }

    private void Update()
    {               
        if (instability > 0f && pauseInstabilityDecay == false)
        {
            Debug.Log("Help");
            instability -= Time.deltaTime / 5f;
            Mathf.Clamp(instability, 0f, 1f);
            OnInstabilityChanged.Invoke(instability);
        }
        
    }

    private void AddInstability(float amount)
    {
        instability += amount;

        if (instabilityCR != null)
        {
            StopCoroutine(instabilityCR);            
        }

        instabilityCR = StartCoroutine(InstabilityDecayTimer());
    }

    private IEnumerator InstabilityDecayTimer()
    {
        pauseInstabilityDecay = true;
        yield return new WaitForSeconds(3f);
        pauseInstabilityDecay = false;
    }
}
