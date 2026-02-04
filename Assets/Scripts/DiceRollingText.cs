using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceRollingText : MonoBehaviour
{
    public Text text;
    public float interval = 0.1f;

    private Coroutine rollCoroutine;

    void Awake()
    {
        if (text == null)
            text = GetComponent<Text>();
    }

    public void StartRolling()
    {
        StopRolling();
        rollCoroutine = StartCoroutine(Roll());
    }

    public void StopRolling(int finalValue)
    {
        StopRolling();
        text.text = finalValue.ToString();
    }

    public void StopRolling()
    {
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);
            rollCoroutine = null;
        }
    }

    IEnumerator Roll()
    {
        while (true)
        {
            for (int i = 1; i <= 6; i++)
            {
                text.text = i.ToString();
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
