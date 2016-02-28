using System;
using UnityEngine;

namespace VRStandardAssets.Utils
{
    // In order to interact with objects in the scene
    // this class casts a ray into the scene and if it finds
    // a VRInteractiveItem it exposes it for other classes to use.
    // This script should be generally be placed on the camera.
    public class VREyeRaycaster : MonoBehaviour
    {
        public event Action<RaycastHit> OnRaycasthit;                   // This event is called every frame that the user's gaze is over a collider.


        [SerializeField] protected Transform m_Camera;
        [SerializeField] protected LayerMask m_ExclusionLayers;           // Layers to exclude from the raycast.
        [SerializeField] protected Reticle m_Reticle;                     // The reticle, if applicable.
        [SerializeField] protected VRInput m_VrInput;                     // Used to call input based events on the current VRInteractiveItem.
        [SerializeField] protected bool m_ShowDebugRay;                   // Optionally show the debug ray.
        [SerializeField] protected float m_DebugRayLength = 5f;           // Debug ray length.
        [SerializeField] protected float m_DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        [SerializeField] protected float m_RayLength = 500f;              // How far into the scene the ray is cast.

        
        protected VRInteractiveItem m_CurrentInteractible;                //The current interactive item
        protected VRInteractiveItem m_LastInteractible;                   //The last interactive item


        // Utility for other classes to get the current interactive item
        public VRInteractiveItem CurrentInteractible
        {
            get { return m_CurrentInteractible; }
        }

        
        protected void OnEnable()
        {
            m_VrInput.OnClick += HandleClick;
            m_VrInput.OnDoubleClick += HandleDoubleClick;
            m_VrInput.OnUp += HandleUp;
            m_VrInput.OnDown += HandleDown;
        }


        protected void OnDisable ()
        {
            m_VrInput.OnClick -= HandleClick;
            m_VrInput.OnDoubleClick -= HandleDoubleClick;
            m_VrInput.OnUp -= HandleUp;
            m_VrInput.OnDown -= HandleDown;
        }


        protected void Update()
        {
            EyeRaycast();
        }

      
        protected virtual void EyeRaycast()
        {
            // Show the debug ray if required
            if (m_ShowDebugRay)
            {
                Debug.DrawRay(m_Camera.position, m_Camera.forward * m_DebugRayLength, Color.blue, m_DebugRayDuration);
            }

            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(m_Camera.position, m_Camera.forward);
            RaycastHit hit;
            
            // Do the raycast forweards to see if we hit an interactive item
            if (Physics.Raycast(ray, out hit, m_RayLength, ~m_ExclusionLayers))
            {
                VRInteractiveItem interactible = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                m_CurrentInteractible = interactible;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactible && interactible != m_LastInteractible)
                    interactible.Over(); 

                // Deactive the last interactive item 
                if (interactible != m_LastInteractible)
                    DeactiveLastInteractible();

                m_LastInteractible = interactible;

                // Something was hit, set at the hit position.
                if (m_Reticle)
                    m_Reticle.SetPosition(hit);

                InvokeOnRaycastHitEvent(hit);
            }
            else
            {
                // Nothing was hit, deactive the last interactive item.
                DeactiveLastInteractible();
                m_CurrentInteractible = null;

                // Position the reticle at default distance.
                if (m_Reticle)
                    m_Reticle.SetPosition();
            }
        }


        protected void DeactiveLastInteractible()
        {
            if (m_LastInteractible == null)
                return;

            m_LastInteractible.Out();
            m_LastInteractible = null;
        }


        protected void HandleUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Up();
        }


        protected void HandleDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Down();
        }


        protected void HandleClick()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Click();
        }


        protected void HandleDoubleClick()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.DoubleClick();

        }

        // Added to handle setting an Event from a derived class.
        protected void InvokeOnRaycastHitEvent(RaycastHit hit)
        {
            if(OnRaycasthit != null) {
                OnRaycasthit(hit);
            }
        }
    }
}