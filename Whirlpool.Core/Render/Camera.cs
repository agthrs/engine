﻿using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Whirlpool.Core.Render
{
    public class Camera
    {
        Vector3 position = new Vector3(0, 0, 1.0f);
        Vector3 cameraFront = new Vector3(0, 0, -1.0f);
        Vector3 cameraUp = new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        /// View matrix for camera matrix calculations.
        /// </summary>
        Matrix4 view
        {
            get
            {
                return Matrix4.LookAt(position, position + cameraFront, cameraUp);
            }
        }

        /// <summary>
        /// Projection matrix for camera matrix calculations.
        /// </summary>
        Matrix4 projection
        {
            get
            {
                return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), windowRatio, 0.1f, 100.0f);
            }
        }

        /// <summary>
        /// The ratio for window width to window height.
        /// </summary>
        public float windowRatio
        {
            get
            {
                int[] viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);
                return (float)viewport[2] / viewport[3];
            }
        }

        /// <summary>
        /// ViewProjection matrix for camera matrix calculations.
        /// </summary>
        public Matrix4 vp
        {
            get
            {
                return view * projection;
            }
        }

        /// <summary>
        /// Update the camera.
        /// </summary>
        public void Update() { }
    }
}
