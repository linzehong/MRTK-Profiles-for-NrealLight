using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrackingState = Microsoft.MixedReality.Toolkit.TrackingState;

namespace HoloLab.MixedReality.Toolkit.NrealLight.Input
{
    [MixedRealityController(SupportedControllerType.GenericOpenVR,
        new[] { Handedness.Left,Handedness.Right },
        flags: MixedRealityControllerConfigurationFlags.UseCustomInteractionMappings)]
    public class NrealLightController : BaseController
    {
        public NrealLightController(
            TrackingState trackingState,
            Handedness controllerHandedness,
            IMixedRealityInputSource inputSource = null,
            MixedRealityInteractionMapping[] interactions = null) : base(trackingState, controllerHandedness,
            inputSource, interactions)
        {
            // lincon
            // AssignControllerMappings(DefaultInteractionsMy);
        }

        // lincon
        // 如果要强制设置映射则在构造函数中添加 AssignControllerMappings(DefaultInteractionsMy);
        // 修改了可视化配置的问题后，这里不需要了，还是通过可视化界面进行配置
        // public MixedRealityInteractionMapping[] DefaultInteractionsMy => new[]
        // {
        //     new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, new MixedRealityInputAction(4, "Pointer Pose", AxisType.SixDof)),
        //     new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, new MixedRealityInputAction(3, "Grip Pose", AxisType.SixDof)),
        //     new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, new MixedRealityInputAction(1, "Select", AxisType.Digital)),
        //     new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, new MixedRealityInputAction(7, "Grip Press", AxisType.SingleAxis)),
        //     new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger, new MixedRealityInputAction(13, "Index Finger Pose", AxisType.SixDof))
        // };
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
       {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger, MixedRealityInputAction.None)
        };

        public override bool IsInPointingPose => true;

        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        public override void SetupDefaultInteractions(Handedness controllerHandedness) => AssignControllerMappings(DefaultInteractions);

        public void UpdateController()
        {
            if (!Enabled) { return; }

            var controllerAnchor = NRInput.DomainHand == ControllerHandEnum.Left ? ControllerAnchorEnum.LeftModelAnchor : ControllerAnchorEnum.RightModelAnchor;
            var pointerAnchor = NRInput.RaycastMode == RaycastModeEnum.Gaze ? ControllerAnchorEnum.GazePoseTrackerAnchor : controllerAnchor;
            var controller = NRInput.AnchorsHelper.GetAnchor(pointerAnchor);

            // hand pose
            var lastState = TrackingState;
            TrackingState = NRInput.CheckControllerAvailable(NRInput.DomainHand) ? TrackingState.Tracked : TrackingState.NotTracked;
            if (lastState != TrackingState)
            {
                CoreServices.InputSystem?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            }
            if (TrackingState == TrackingState.Tracked)
            {
                CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, new MixedRealityPose(controller.position, controller.rotation));
            }

            // hand interaction
            if (Interactions == null)
            {
                Debug.LogError($"No interaction configuration for Nreal Light Controller Source");
                Enabled = false;
            }
            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.None:
                        break;
                    case DeviceInputType.SpatialPointer:
                        var pointer = new MixedRealityPose(controller.position, controller.rotation);
                        Interactions[i].PoseData = pointer;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, pointer);
                        }
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.TriggerPress:
                        Interactions[i].BoolData = NRInput.GetButton(ControllerButton.TRIGGER);

                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                }
            }
        }
    }
}
