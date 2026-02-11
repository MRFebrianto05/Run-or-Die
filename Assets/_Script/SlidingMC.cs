// using System;
// using System.Collections;
// using UnityEngine;

// public class SlidingMC : MonoBehaviour
// {
//     public  MovementMC MMC;
//     public Rigidbody2D rb;
//     public BoxCollider2D boxCollider;
//     public float slideDuration = 1.0f;
//     public float slideSpeed = 12f;

//     public Vector2 slidingOffset = new Vector2(0f, -0.25f);
//     public GameObject slidingObject;

//     private bool isSliding = false;



//     void Awake()
//     {


//         if (slidingObject != null) slidingObject.SetActive(false);
//     }

//     void Start()
//     {
        
//     }

//     void Update()
//     {
        
//     }

//         private void Slide()
//     {
//         isSliding  = true;
        
//         boxCollider.size = slidingSize;
//         boxCollider.offset = slidingOffset;

//         if (standingObject != null) standingObject.SetActive(false);
//         if (slidingObject != null) slidingObject.SetActive(true);

//         rb.linearVelocity = new Vector2(rb.linearVelocity.y, slideSpeed);

//         StartCoroutine (StopSlide);
//     }

//     private void StartCoroutine(Func<IEnumerator> stopSlide)
//     {
//         throw new NotImplementedException();
//     }

//     IEnumerator StopSlide()
//     {
//         yield return new WaitForSeconds(slideDuration);

//         boxCollider.size = standingSize;
//         boxCollider.offset = standingOffset;

//         if (standingObject != null) standingObject.SetActive(true);
//         if (slidingObject != null) slidingObject.SetActive(false);

//         isSliding = false;
        
//     }
// }
