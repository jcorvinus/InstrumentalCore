using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Core;
using Instrumental.Core.Math;

namespace Instrumental.Interaction.Solvers
{
    public class KabschSolver
    {
        Vector3[] QuatBasis = new Vector3[3];
        Vector3[] DataCovariance = new Vector3[3];
        Quaternion OptimalRotation = Quaternion.identity;
        public float scaleRatio = 1f;
        public Matrix4x4 SolveKabsch(List<Vect3> inPoints, List<Vector4> refPoints, bool solveRotation = true, bool solveScale = false)
        {
            if (inPoints.Count != refPoints.Count) { return Matrix4x4.identity; }

            //Calculate the centroid offset and construct the centroid-shifted point matrices
            Vector3 inCentroid = Vector3.zero; Vector3 refCentroid = Vector3.zero;
            float inTotal = 0f, refTotal = 0f;
            for (int i = 0; i < inPoints.Count; i++)
            {
                inCentroid += new Vector3(inPoints[i].x, inPoints[i].y, inPoints[i].z) * refPoints[i].w;
                inTotal += refPoints[i].w;
                refCentroid += new Vector3(refPoints[i].x, refPoints[i].y, refPoints[i].z) * refPoints[i].w;
                refTotal += refPoints[i].w;
            }
            inCentroid /= inTotal;
            refCentroid /= refTotal;

            //Calculate the scale ratio
            if (solveScale)
            {
                float inScale = 0f, refScale = 0f;
                for (int i = 0; i < inPoints.Count; i++)
                {
                    inScale += (new Vector3(inPoints[i].x, inPoints[i].y, inPoints[i].z) - inCentroid).magnitude;
                    refScale += (new Vector3(refPoints[i].x, refPoints[i].y, refPoints[i].z) - refCentroid).magnitude;
                }
                scaleRatio = (refScale / inScale);
            }

            //Calculate the 3x3 covariance matrix, and the optimal rotation
            if (solveRotation)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Solve Optimal Rotation");
                extractRotation(TransposeMultSubtract(inPoints, refPoints, inCentroid, refCentroid, DataCovariance), ref OptimalRotation);
                UnityEngine.Profiling.Profiler.EndSample();
            }

            return Matrix4x4.TRS(refCentroid, Quaternion.identity, Vector3.one * scaleRatio) *
                   Matrix4x4.TRS(Vector3.zero, OptimalRotation, Vector3.one) *
                   Matrix4x4.TRS(-inCentroid, Quaternion.identity, Vector3.one);
        }

        //https://animation.rwth-aachen.de/media/papers/2016-MIG-StableRotation.pdf
        //Iteratively apply torque to the basis using Cross products (in place of SVD)
        void extractRotation(Vector3[] A, ref Quaternion q)
        {
            for (int iter = 0; iter < 9; iter++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Iterate Quaternion");
                q.FillMatrixFromQuaternion(ref QuatBasis);
                Vector3 omega = (Vector3.Cross(QuatBasis[0], A[0]) +
                                 Vector3.Cross(QuatBasis[1], A[1]) +
                                 Vector3.Cross(QuatBasis[2], A[2])) *
                 (1f / Mathf.Abs(Vector3.Dot(QuatBasis[0], A[0]) +
                                 Vector3.Dot(QuatBasis[1], A[1]) +
                                 Vector3.Dot(QuatBasis[2], A[2]) + 0.000000001f));

                float w = omega.magnitude;
                if (w < 0.000000001f)
                    break;
                q = Quaternion.AngleAxis(w * Mathf.Rad2Deg, omega / w) * q;
                q = Quaternion.Lerp(q, q, 0f); //Normalizes the Quaternion; critical for error suppression
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        //Calculate Covariance Matrices --------------------------------------------------
        public static Vector3[] TransposeMultSubtract(List<Vect3> vec1, List<Vector4> vec2, Vector3 vec1Centroid, Vector3 vec2Centroid, Vector3[] covariance)
        {
            UnityEngine.Profiling.Profiler.BeginSample("Calculate Covariance Matrix");
            for (int i = 0; i < 3; i++)
            { //i is the row in this matrix
                covariance[i] = Vector3.zero;
            }

            for (int k = 0; k < vec1.Count; k++)
            {//k is the column in this matrix
                Vector3 left = ((Vector3)vec1[k] - vec1Centroid) * vec2[k].w;
                Vector3 right = (new Vector3(vec2[k].x, vec2[k].y, vec2[k].z) - vec2Centroid) * Mathf.Abs(vec2[k].w);

                covariance[0][0] += left[0] * right[0];
                covariance[1][0] += left[1] * right[0];
                covariance[2][0] += left[2] * right[0];
                covariance[0][1] += left[0] * right[1];
                covariance[1][1] += left[1] * right[1];
                covariance[2][1] += left[2] * right[1];
                covariance[0][2] += left[0] * right[2];
                covariance[1][2] += left[1] * right[2];
                covariance[2][2] += left[2] * right[2];
            }

            UnityEngine.Profiling.Profiler.EndSample();
            return covariance;
        }
        public static Vector3[] TransposeMultSubtract(Vector3[] vec1, Vector3[] vec2, ref Vector3[] covariance)
        {
            for (int i = 0; i < 3; i++) covariance[i] = Vector3.zero;

            for (int k = 0; k < vec1.Length; k++)
            {//k is the column in this matrix
                Vector3 left = vec1[k];
                Vector3 right = vec2[k];

                covariance[0][0] += left[0] * right[0];
                covariance[1][0] += left[1] * right[0];
                covariance[2][0] += left[2] * right[0];
                covariance[0][1] += left[0] * right[1];
                covariance[1][1] += left[1] * right[1];
                covariance[2][1] += left[2] * right[1];
                covariance[0][2] += left[0] * right[2];
                covariance[1][2] += left[1] * right[2];
                covariance[2][2] += left[2] * right[2];
            }
            return covariance;
        }
    }

    public static class FromMatrixExtension
    {
        public static Vector3 GetVector3(this Matrix4x4 m) { return m.GetColumn(3); }
        public static Quaternion GetQuaternion(this Matrix4x4 m)
        {
            if (m.GetColumn(2) == m.GetColumn(1)) { return Quaternion.identity; }
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }
        public static void FillMatrixFromQuaternion(this Quaternion q, ref Vector3[] covariance)
        {
            covariance[0] = q * Vector3.right;
            covariance[1] = q * Vector3.up;
            covariance[2] = q * Vector3.forward;
        }
        public static Matrix4x4 Lerp(Matrix4x4 a, Matrix4x4 b, float alpha)
        {
            return Matrix4x4.TRS(Vector3.Lerp(a.GetVector3(), b.GetVector3(), alpha), Quaternion.Slerp(a.GetQuaternion(), b.GetQuaternion(), alpha), Vector3.one);
        }
    }
}