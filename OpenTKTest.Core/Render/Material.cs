﻿#define CACHELOCATIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKTest.Core.Render
{
    public class Material
    {
        private int shaderProgram;
        private Dictionary<string, int> locations = new Dictionary<string, int>();

        public Material()
        {
            shaderProgram = GL.CreateProgram();
        }

        public void Attach(Shader shader)
        {
            GL.AttachShader(shaderProgram, shader.glShader);
        }

        public void Link()
        {
            GL.LinkProgram(shaderProgram);
            GL.GetProgram(shaderProgram, GetProgramParameterName.ActiveUniforms, out var uniformCount);
            Console.WriteLine("Shader uniforms");
            for (int i = 0; i < uniformCount; ++i)
            {
                StringBuilder uniformName = new StringBuilder();
                GL.GetActiveUniform(shaderProgram, i, 2048, out var length, out var size, out var type, uniformName);
                locations.Add(uniformName.ToString(), i);
                Console.WriteLine("\t" + uniformName.ToString() + " - " + i.ToString());
            }
        }

        public void Use()
        {
            GL.UseProgram(shaderProgram);
        }

        protected int GetVariableLocation(string variable)
        {
            if (locations.TryGetValue(variable, out var locationCached))
                return locationCached;
            return -1;
        }

        public void SetVariable(string variable, bool value)
        {
            GL.ProgramUniform1(shaderProgram, GetVariableLocation(variable), (value) ? 1 : 0);
        }

        public void SetVariable(string variable, float value)
        {
            GL.ProgramUniform1(shaderProgram, GetVariableLocation(variable), value);
        }
        public void SetVariable(string variable, int value)
        {
            GL.ProgramUniform1(shaderProgram, GetVariableLocation(variable), value);
        }
        public void SetVariable(string variable, Vector2 value)
        {
            GL.ProgramUniform2(shaderProgram, GetVariableLocation(variable), value.X, value.Y);
        }
        public void SetVariable(string variable, Vector3 value)
        {
            GL.ProgramUniform3(shaderProgram, GetVariableLocation(variable), value.X, value.Y, value.Z);
        }
        public void SetVariable(string variable, Color4 value)
        {
            GL.ProgramUniform4(shaderProgram, GetVariableLocation(variable), value.R, value.G, value.B, value.A);
        }
        public void SetVariable(string variable, Matrix4 value)
        {
            GL.ProgramUniformMatrix4(shaderProgram, GetVariableLocation(variable), false, ref value);
        }
    }
    
    public class MaterialBuilder
    {
        private Material instance;
        public MaterialBuilder Build()
        {
            instance = new Material();
            return this;
        }

        public MaterialBuilder Attach(Shader shader)
        {
            instance.Attach(shader);
            return this;
        }

        public MaterialBuilder Link()
        {
            instance.Link();
            return this;
        }

        public Material GetMaterial()
        {
            return instance;
        }
    }

}