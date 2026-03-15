using UnityEngine;

public class DefaultAbsorbable : MonoBehaviour, I_Absorbable
{
    public void Absorb(Character c)
    {
        Debug.Log("Absorption Interaction");
       
        if (c.CompareTag("Player"))
        {
            // - Reset Dash
            PlayerCharacter player = c.GetComponent<PlayerCharacter>();
            player.StopCoroutine(player.pc.dashCR);
            c.GetComponent<PlayerCharacter>().pc.canDash = true;
        }

        Destroy(this.gameObject);
    }    

    public float GetInstabilityValue() {  return 25f; }
}
