using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class testComplte : MonoBehaviour
{
    public Button btnTest;
    public Animator anim;
    public Animator[] animStars;
    public GameObject starsGo;
    private void Start()
    {

        this.starsGo.SetActive(false);

        foreach (var animStar in this.animStars)
        {
            animStar.speed = 0;
            animStar.gameObject.SetActive(false);
        }
        this.anim.speed = 0;

        btnTest.onClick.AddListener(() =>
        {
            this.anim.speed = 1;
            this.anim.Play("uipopup_complete");

            StartCoroutine(this.WaitAnim(0.533f, () =>
            {
                this.starsGo.SetActive(true);
                this.animStars[0].gameObject.SetActive(true);
                this.animStars[0].speed = 0;
                //this.animStars[0].Play("uipopup_complete_star");

                StartCoroutine(this.WaitAnim(0.25f, () =>
                {
                    this.animStars[1].gameObject.SetActive(true);
                    this.animStars[1].speed = 0;

                    StartCoroutine(this.WaitAnim(0.25f, () =>
                    {
                        this.animStars[2].gameObject.SetActive(true);
                        this.animStars[2].speed = 0;
                    }));
                }));

            }));
        });
    }

    IEnumerator WaitAnim(float t, System.Action onComplete)
    {
        yield return new WaitForSeconds(t);
        onComplete();
    }
}