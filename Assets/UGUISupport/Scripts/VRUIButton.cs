using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

namespace Assets.Testbed.Scripts {
  [RequireComponent(typeof(VRInteractiveItem))]
  public class VRUIButton: Button {
    private VRInteractiveItem _interactiveItem;

    protected override void Awake() {
      _interactiveItem = GetComponent<VRInteractiveItem>();
      _interactiveItem.OnClick += HandleVROnClick;
      _interactiveItem.OnOver += HandleVROnOver;

      base.Awake();
    }

    private void HandleVROnOver() {
      Debug.Log("Over!");
    }

    private void HandleVROnClick() {
      onClick.Invoke();
    }
  }
}
