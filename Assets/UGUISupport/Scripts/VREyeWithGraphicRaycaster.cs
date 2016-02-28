using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VRStandardAssets.Utils {
  // In order to interact with objects in the scene
  // this class casts a ray into the scene and if it finds
  // a VRInteractiveItem it exposes it for other classes to use.
  // This script should be generally be placed on the camera.
  public class VREyeWithGraphicRaycaster : VREyeRaycaster {
    [SerializeField] protected EventSystem m_EventSystem;
    public List<GameObject> m_Selected;

    protected Camera m_CameraObject;

    protected new void OnEnable() {
      m_CameraObject = m_Camera.GetComponent<Camera>();
      base.OnEnable();    
    }

    protected override void EyeRaycast() {
      // Show the debug ray if required
      if(m_ShowDebugRay) {
        Debug.DrawRay(m_Camera.position, m_Camera.forward * m_DebugRayLength, Color.blue, m_DebugRayDuration);
      }

      EyeRaycastHit raycastHit;

      // Do the raycast forwards to see if we hit an interactive item
      if(GetInteractiveItem(out raycastHit)) {
        //VRInteractiveItem interactible = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
        VRInteractiveItem interactible = raycastHit.item;
        m_CurrentInteractible = interactible;

        // If we hit an interactive item and it's not the same as the last interactive item, then call Over
        if(interactible && interactible != m_LastInteractible) {
          interactible.Over();
        }

        // Deactive the last interactive item 
        if(interactible != m_LastInteractible)
          DeactiveLastInteractible();

        m_LastInteractible = interactible;

        // Something was hit, set at the hit position.
        if(m_Reticle)
          m_Reticle.SetPosition(raycastHit.point, raycastHit.distance, raycastHit.normal);

        if(raycastHit.hasHit) {
          InvokeOnRaycastHitEvent(raycastHit.hit);
        }
      }
      else {
        // Nothing was hit, deactive the last interactive item.
        DeactiveLastInteractible();
        m_CurrentInteractible = null;

        // Position the reticle at default distance.
        if(m_Reticle)
          m_Reticle.SetPosition();
      }
    }

    private bool GetInteractiveItem(out EyeRaycastHit raycastHit) {
      raycastHit = new EyeRaycastHit();
      raycastHit.distance = -1;

      EyeRaycastHit physicsRaycastClosestItem = GetPhysicsInteractiveItem();
      EyeRaycastHit canvasRaycastClosestItem = GetGraphicInteractiveItem();

      List<EyeRaycastHit> interactibleItems = new List<EyeRaycastHit>() { physicsRaycastClosestItem, canvasRaycastClosestItem };

      bool hasItems = interactibleItems.Count > 0;
      if(hasItems) {
        float closestDistance = m_RayLength;
        EyeRaycastHit closestRaycastHit = interactibleItems.FindLast(potentialClosestHit => {
          bool closer = (potentialClosestHit.distance < closestDistance) && (potentialClosestHit.distance >= 0.0f);
          if(closer) {
            closestDistance = potentialClosestHit.distance;
          }
          return closer;
        });

        raycastHit = closestRaycastHit;

        // Weird check in the event of CurvedUI Canvas's obscuring its child.
        if(raycastHit == physicsRaycastClosestItem && physicsRaycastClosestItem.gameObject != null) {
          
          Canvas curvedUICanvas = raycastHit.gameObject.GetComponent<Canvas>();
          if(curvedUICanvas != null &&
            canvasRaycastClosestItem.item != null &&
            canvasRaycastClosestItem.gameObject.transform.IsChildOf(curvedUICanvas.transform)) {
            EyeRaycastHit edgeCaseHit = canvasRaycastClosestItem;
            edgeCaseHit.distance = raycastHit.distance;
            Debug.Log("Edge case detected! " + raycastHit.gameObject, canvasRaycastClosestItem.gameObject);

            raycastHit = edgeCaseHit;
          }
        }
      }

      return hasItems;
    }

    private EyeRaycastHit GetPhysicsInteractiveItem() {
      EyeRaycastHit result = new EyeRaycastHit();
      result.distance = -1.0f;

      Ray ray = new Ray(m_Camera.position, m_Camera.forward);
      RaycastHit hit;

      // Do the raycast forwards to see if we hit an interactive item
      if(Physics.Raycast(ray, out hit, m_RayLength, ~m_ExclusionLayers)) {
        result.item = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
        result.point = hit.point;
        result.normal = hit.normal;
        result.distance = hit.distance;
        result.hit = hit;
        result.hasHit = true;
        result.gameObject = hit.collider.gameObject;
      }

      return result;
    }

    private EyeRaycastHit GetGraphicInteractiveItem() {
      EyeRaycastHit result = new EyeRaycastHit();
      result.distance = -1.0f;

      //EventSystem eventSystem = EventSystem.current; 

      PointerEventData eventData = new PointerEventData(m_EventSystem);
      //Vector2 position = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
      Vector2 position = m_CameraObject.WorldToScreenPoint(m_Camera.position + m_Camera.forward);
      //Debug.Log("Raycast Startpoint: " + position + " vs. " + cposition + " vs. " + wposition);

      eventData.position = position;

      List<RaycastResult> results = new List<RaycastResult>();
      m_EventSystem.RaycastAll(eventData, results);

      // Check to see if we have an interactive item.
      if(results.Count > 0) {
        // Make sure we don't have any duplicates.
        List<GameObject> resultsObjects = new List<GameObject>();
        results.RemoveAll(potentialDuplicate => {
          bool alreadyContains = resultsObjects.Contains(potentialDuplicate.gameObject);
          if(!alreadyContains) {
            resultsObjects.Add(potentialDuplicate.gameObject);
          }
          return alreadyContains;
        });

        /* If so, grab the first one (since it will be the closest item) and remove everything else (since we don't care 
         * about regular UI items if we have an interactive item). This may look super strange, but there's no sense in doubling
         * back through all of the objects and call GetComponent again (which is expensive) if there is a VRInteractiveItem on 
         * one of them, so I just cache the first one that comes by. */
        GameObject firstInteractiveObject = null;
        bool interactiveItemExists = results.Exists(raycastResult => {
          VRInteractiveItem item = raycastResult.gameObject.GetComponent<VRInteractiveItem>();
          if(item != null && firstInteractiveObject == null) {
            firstInteractiveObject = raycastResult.gameObject;
          }
          return item != null;
        });

        if(interactiveItemExists) {
          results.RemoveAll(currentResult => {
            return currentResult.gameObject != firstInteractiveObject;
          });
        }
      }

      // If we have any results at this point, just grab the first one, and let's go!
      if(results.Count > 0) {
        GameObject resultObject = results[0].gameObject;
        result.item = resultObject.GetComponent<VRInteractiveItem>();
        Vector3 worldPosition = m_CameraObject.ScreenToWorldPoint(new Vector3(results[0].screenPosition.x, results[0].screenPosition.y, results[0].distance));

        result.point = worldPosition;
        result.normal = -resultObject.transform.forward;
        result.distance = results[0].distance;
        result.gameObject = results[0].gameObject;
      }

      return result;
    }
  }
}