using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance;

    [SerializeField] private TMP_Text instabilityText;
    [SerializeField] private Image instabilityFillbar;

    private void Awake()
    {
        if (Instance != null) Destroy(this.gameObject);
        else Instance = this;
    }

    private void Start()
    {
        UpdateInstability(0f);
    }


    public void UpdateInstability(float amount)
    {   
        instabilityText.text = amount.ToString("#.##");
        instabilityFillbar.fillAmount = amount / 100f;
    }
}
