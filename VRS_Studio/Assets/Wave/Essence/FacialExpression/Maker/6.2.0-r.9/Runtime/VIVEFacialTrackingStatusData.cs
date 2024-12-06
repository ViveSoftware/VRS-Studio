using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


namespace Wave.Essence.FacialExpression.Maker
{
    [System.Serializable]
    public class JointInfo
    {
        public string Name;
        public Vector3 LocalPos;
        public Vector3 LocalRotation;
        public Vector3 LocalScale;
        public List<float> BlendShapes = new List<float>();
        public JointInfo(string _Name, Vector3 _Pos, Vector3 _Rotation, Vector3 _Scale, SkinnedMeshRenderer _SMR)
        {
            Name = _Name;
            LocalPos = _Pos;
            LocalRotation = _Rotation;
            LocalScale = _Scale;
			if (_SMR != null)
            {
				//Debug.Log("JointInfo() 999: blencshapecount:" + _SMR.sharedMesh.blendShapeCount);
				for (int i = 0; i < _SMR.sharedMesh.blendShapeCount; i++)
                {
                    BlendShapes.Add(_SMR.GetBlendShapeWeight(i));
                }
            }
			//else Debug.Log("JointInfo() _SMR is null "+ _Name);
		}

        public static bool Match(JointInfo _InfoA, JointInfo _InfoB)
        {

			//Debug.Log("Match(6) 999: unknown ACount: " + _InfoA.BlendShapes.Count + ", BCount: " + _InfoB.BlendShapes.Count);
			if (Vector3.Distance(_InfoA.LocalPos, _InfoB.LocalPos) >= 0.0000001f)
            {
				//Debug.Log("Match(6) 999: false LocalPos");
				return false;
            }
            if (Vector3.Distance(_InfoA.LocalRotation, _InfoB.LocalRotation) >= 0.0000001f)
            {
				//Debug.Log("Match(6) 999: false LocalRotation** aRotate: "+ _InfoA.LocalRotation+"bRotate: "+ _InfoB.LocalRotation);
				return false;
            }
            if (Vector3.Distance(_InfoA.LocalScale, _InfoB.LocalScale) >= 0.0000001f)
            {
				//Debug.Log("Match(6) 999: false LocalScale");
				return false;
            }
            if (_InfoA.BlendShapes.Count > 0)
            {
                if (_InfoA.BlendShapes.Count > 0 != _InfoB.BlendShapes.Count > 0)
                {
                    //Debug.LogError("It seems the target has change!");
                }
                bool _Match = true;
                for (int i = 0; i < _InfoA.BlendShapes.Count; i++)
                {
                    if (_InfoA.BlendShapes[i] != _InfoB.BlendShapes[i])
                    {
						//Debug.Log("Match(6) 999: false ACount: " + _InfoA.BlendShapes.Count + ", BCount: " + _InfoB.BlendShapes.Count);
						_Match = false;
                        break;
                    }
                }
                if (!_Match)
                {
					//Debug.Log("Match(6) 999: false ");
					return false;
                }
            }
			//Debug.Log("Match(6) 999: true(no JointGapInfo) ");
			return true;
        }
    }

    public static class ComponentExtensions
    {
        public static T GetCopyOf<T>(this T comp, T other) where T : Component
        {
			//Debug.Log("ComponentExtensions GetCopyOf(6) 999:  ");
			Type type = comp.GetType();
            Type othersType = other.GetType();
            if (type != othersType)
            {
                Debug.LogError($"The type \"{type.AssemblyQualifiedName}\" of \"{comp}\" does not match the type \"{othersType.AssemblyQualifiedName}\" of \"{other}\"!");
                return null;
            }

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
            PropertyInfo[] pinfos = type.GetProperties(flags);

            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch
                    {
                        /*
                         * In case of NotImplementedException being thrown.
                         * For some reason specifying that exception didn't seem to catch it,
                         * so I didn't catch anything specific.
                         */
                    }
                }
            }

            FieldInfo[] finfos = type.GetFields(flags);

            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }
    }
}
