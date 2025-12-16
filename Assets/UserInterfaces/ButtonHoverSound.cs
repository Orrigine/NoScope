using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler
{
	public AudioClip hoverClip;
	public AudioSource audioSource;

	void Awake()
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = GetComponentInParent<AudioSource>();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (hoverClip != null && audioSource != null)
		{
			audioSource.PlayOneShot(hoverClip);
		}
	}
}
