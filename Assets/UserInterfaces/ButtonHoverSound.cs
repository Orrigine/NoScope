using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
	public AudioClip hoverClip;
	public AudioSource audioSource;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (hoverClip != null && audioSource != null)
		{
			audioSource.PlayOneShot(hoverClip);
		}
	}
}
