using UnityEngine;
using UnityEngine.EventSystems;

namespace VRStandardAssets.Utils {
  // This is to supplement the common information between RaycastHit and RaycastResult
  public struct EyeRaycastHit {
    public VRInteractiveItem item;
    public Vector3 point;
    public float distance;
    public Vector3 normal;
    public RaycastHit hit; // store the hit, if there is one.
    public bool hasHit;
    public GameObject gameObject;

    public static bool operator ==(EyeRaycastHit left, EyeRaycastHit right) {
      return left.Equals(right);
    }

    public static bool operator !=(EyeRaycastHit left, EyeRaycastHit right) {
      return !left.Equals(right);
    }

    public override bool Equals(object obj) {
      if(obj.GetType() == typeof(EyeRaycastHit)) {
        EyeRaycastHit other = (EyeRaycastHit)obj;

        return this.item == other.item &&
        this.point == other.point &&
        this.distance == other.distance &&
        this.normal == other.normal &&
        this.hasHit == other.hasHit;
      }
      return base.Equals(obj);
    }

    public override int GetHashCode() {
      return base.GetHashCode();
    }
  }
}
