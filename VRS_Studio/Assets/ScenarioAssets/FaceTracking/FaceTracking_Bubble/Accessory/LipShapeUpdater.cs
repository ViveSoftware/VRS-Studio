using System;
using UnityEngine;
using Wave.Essence.LipExpression;
using TMPro;

namespace HTC.FaceTracking.Interaction
{
    public class LipShapeUpdater : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI debugText;

        public static LipExp CurrentLipShape = LipExp.Max;
        public static Vector3 lipPosition = Vector3.zero;
        public static Vector3 lipForward = Vector3.zero;

        private static float lipMaxAngle = 38f;
        private static float lipMaxDistance = 0.38f;
        private static Transform cam;

        private void Update()
        {
            if (cam == null) cam = Camera.main.transform;
            lipForward = cam.forward;
            lipPosition = cam.position - (cam.up * 0.1f);
            UpdateLipShape();
        }

        private void UpdateLipShape()
        {
            float maxWeight = 0;
            float secondWeight = 0;
            float thirdWeight = 0;
            LipExp secLip = LipExp.Max;
            LipExp thirdLip = LipExp.Max;

            foreach (object value in Enum.GetValues(typeof(LipExp)))
            {
                LipExp lipExp = (LipExp)value;
                float weight = LipExpManager.Instance.GetLipExpression(lipExp) * 100f;
                if (maxWeight < weight)
                {
                    if (weight >= 20) CurrentLipShape = lipExp;
                    else CurrentLipShape = LipExp.Max;
                    maxWeight = weight;
                }
                else if (secondWeight < weight)
                {
                    secondWeight = weight;
                    secLip = lipExp;
                }
                else if (thirdWeight < weight)
                {
                    thirdWeight = weight;
                    thirdLip = lipExp;
                }
            }

            if (debugText)
            {
                debugText.text = $"{CurrentLipShape} {maxWeight}\n" +
                                 $"{secLip} {secondWeight}\n" +
                                 $"{thirdLip} {thirdWeight}\n";
            }
        }

        public static bool IsNearByLip(Vector3 targetPosition)
        {
            if (cam == null) return false;

            bool result = false;

            Vector3 direction = targetPosition - lipPosition;
            float angle = Vector3.Angle(direction, cam.forward);

            if (angle < lipMaxAngle / 2f)
            {
                if (Vector3.Distance(targetPosition, lipPosition) < lipMaxDistance)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}