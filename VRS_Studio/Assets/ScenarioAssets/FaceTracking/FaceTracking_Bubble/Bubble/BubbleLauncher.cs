using UnityEngine;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using Wave.Essence.Hand.StaticGesture;

namespace HTC.FaceTracking.Interaction
{
    public class BubbleLauncher : MonoBehaviour
    {
        public static bool isLeftHandBubbleLaunch = false;
        public static bool isRightHandBubbleLaunch = false;

        [SerializeField] private Collider[] handColliders_Right;
        [SerializeField] private Collider[] handColliders_Left;

        [SerializeField] private Transform launchPoint_Right;
        [SerializeField] private Transform launchPoint_Left;

        [SerializeField] private ParticleSystem bubbleParticle_Right;
        [SerializeField] private ParticleSystem bubbleParticle_Left;

        [SerializeField] private float offset = 0f;
        [SerializeField] private bool launchInEditor = false;

        private JointPose jointPos;
        private Vector3 targetDir = new Vector3();
        private Vector3 newDir = new Vector3();
        private Transform cam;
        private ParticleSystem.EmissionModule emissionModule_Right;
        private ParticleSystem.EmissionModule emissionModule_Left;

        private void Awake()
        {
            emissionModule_Right = bubbleParticle_Right.emission;
            emissionModule_Left = bubbleParticle_Left.emission;
        }

        private void OnEnable()
        {
            cam = Camera.main.transform;
        }

        private void Update()
        {
            LaunchBubble(true);
            LaunchBubble(false);
        }

        private void LaunchBubble(bool isLeftHand)
        {
            bool isOKGesture = false;
            if (isLeftHand)
            {
                VivePose.TryGetHandJointPose(HandRole.LeftHand, HandJointName.ThumbProximal, out jointPos);
                isOKGesture = ViveInput.GetPress(HandRole.LeftHand, ControllerButton.Ok);
            }
            else
            {
                VivePose.TryGetHandJointPose(HandRole.RightHand, HandJointName.ThumbProximal, out jointPos);
                isOKGesture = ViveInput.GetPress(HandRole.RightHand, ControllerButton.Ok);
            }

            Transform rigRoot = cam.transform.parent.parent;
            Vector3 jointWorldPos = rigRoot.TransformPoint(jointPos.pose.pos);

            bool enableToLaunch = isOKGesture;

            if (enableToLaunch || launchInEditor)
            {
                SwitchColliders(false, isLeftHand);
                targetDir = (jointWorldPos - cam.position).normalized;

                if (isLeftHand)
                {
                    newDir = Vector3.RotateTowards(launchPoint_Left.forward, targetDir, 1, 0.0f);
                    launchPoint_Left.rotation = Quaternion.LookRotation(newDir);
                    launchPoint_Left.position = jointWorldPos + targetDir * offset;
                    emissionModule_Left.enabled = true;
                    isLeftHandBubbleLaunch = true;
                }
                else
                {
                    newDir = Vector3.RotateTowards(launchPoint_Right.forward, targetDir, 1, 0.0f);
                    launchPoint_Right.rotation = Quaternion.LookRotation(newDir);
                    launchPoint_Right.position = jointWorldPos + targetDir * offset;
                    emissionModule_Right.enabled = true;
                    isRightHandBubbleLaunch = true;
                }
            }
            else
            {
                if (isLeftHand)
                {
                    isLeftHandBubbleLaunch = false;
                    emissionModule_Left.enabled = false;
                    SwitchColliders(true, true);
                }
                else
                {
                    isRightHandBubbleLaunch = false;
                    emissionModule_Right.enabled = false;
                    SwitchColliders(true, false);
                }
            }
        }

        public void SwitchColliders(bool value, bool isLeftHand)
        {
            if (isLeftHand)
            {
                foreach (Collider collider in handColliders_Left)
                {
                    collider.enabled = value;
                }
            }
            else
            {
                foreach (Collider collider in handColliders_Right)
                {
                    collider.enabled = value;
                }
            }
        }
    }
}