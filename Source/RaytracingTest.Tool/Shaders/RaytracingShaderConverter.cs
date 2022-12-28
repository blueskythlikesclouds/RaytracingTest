﻿using Microsoft.VisualBasic;
using RaytracingTest.Tool.Shaders.Constants;
using RaytracingTest.Tool.Shaders.Instructions;
using System.Xml.Linq;

namespace RaytracingTest.Tool.Shaders;

public static class RaytracingShaderConverter
{
    private static void WriteConstant(StringBuilder stringBuilder, string outName, Constant constant, ShaderType shaderType, ShaderMapping shaderMapping)
    {
        if (constant.Name == "g_MtxProjection")
        {
            if (constant.Size >= 1) stringBuilder.AppendFormat("\t{0}.g_MtxProjection[0] = payload.Depth > 0 ? float4(1, 0, 0, 0) : g_MtxProjection[0];\n", outName);
            if (constant.Size >= 2) stringBuilder.AppendFormat("\t{0}.g_MtxProjection[1] = payload.Depth > 0 ? float4(0, 1, 0, 0) : g_MtxProjection[1];\n", outName);
            if (constant.Size >= 3) stringBuilder.AppendFormat("\t{0}.g_MtxProjection[2] = payload.Depth > 0 ? float4(0, 0, 0, 0) : g_MtxProjection[2];\n", outName);
            if (constant.Size >= 4) stringBuilder.AppendFormat("\t{0}.g_MtxProjection[3] = payload.Depth > 0 ? float4(0, 0, 0, 1) : g_MtxProjection[3];\n", outName);
        }      
        else if (constant.Name == "g_MtxInvProjection")
        {
            if (constant.Size >= 1) stringBuilder.AppendFormat("\t{0}.g_MtxInvProjection[0] = payload.Depth > 0 ? float4(1, 0, 0, 0) : g_MtxInvProjection[0];\n", outName);
            if (constant.Size >= 2) stringBuilder.AppendFormat("\t{0}.g_MtxInvProjection[1] = payload.Depth > 0 ? float4(0, 1, 0, 0) : g_MtxInvProjection[1];\n", outName);
            if (constant.Size >= 3) stringBuilder.AppendFormat("\t{0}.g_MtxInvProjection[2] = payload.Depth > 0 ? float4(0, 0, 0, 0) : g_MtxInvProjection[2];\n", outName);
            if (constant.Size >= 4) stringBuilder.AppendFormat("\t{0}.g_MtxInvProjection[3] = payload.Depth > 0 ? float4(0, 0, -Z_MAX, 1) : g_MtxInvProjection[3];\n", outName);
        }
        else if (constant.Name == "g_MtxView")
        {
            if (constant.Size >= 1) stringBuilder.AppendFormat("\t{0}.g_MtxView[0] = payload.Depth > 0 ? float4(1, 0, 0, 0) : g_MtxView[0];\n", outName);
            if (constant.Size >= 2) stringBuilder.AppendFormat("\t{0}.g_MtxView[1] = payload.Depth > 0 ? float4(0, 1, 0, 0) : g_MtxView[1];\n", outName);
            if (constant.Size >= 3) stringBuilder.AppendFormat("\t{0}.g_MtxView[2] = payload.Depth > 0 ? float4(0, 0, 0, 0) : g_MtxView[2];\n", outName);
            if (constant.Size >= 4) stringBuilder.AppendFormat("\t{0}.g_MtxView[3] = payload.Depth > 0 ? float4(0, 0, -RayTCurrent(), 1) : g_MtxView[3];\n", outName);
        }
        else if (constant.Size == 4)
        {
            stringBuilder.AppendFormat("\t{0}.{1}[0] = float4(1, 0, 0, 0);\n", outName, constant.Name);
            stringBuilder.AppendFormat("\t{0}.{1}[1] = float4(0, 1, 0, 0);\n", outName, constant.Name);
            stringBuilder.AppendFormat("\t{0}.{1}[2] = float4(0, 0, 1, 0);\n", outName, constant.Name);
            stringBuilder.AppendFormat("\t{0}.{1}[3] = float4(0, 0, 0, 1);\n", outName, constant.Name);
        }
        else if (constant.Name == "g_MtxPalette")
        {
            stringBuilder.AppendFormat("\t{0}.{1}[0] = float4(1, 0, 0, 0);\n", outName, constant.Name);
            stringBuilder.AppendFormat("\t{0}.{1}[1] = float4(0, 1, 0, 0);\n", outName, constant.Name);
            stringBuilder.AppendFormat("\t{0}.{1}[2] = float4(0, 0, 1, 0);\n", outName, constant.Name);
        }
        else if (constant.Size <= 1)
        {
            stringBuilder.AppendFormat("\t{0}.{1} = ", outName, constant.Name);

            if (shaderMapping.CanMapFloat4(constant.Register, shaderType))
            {
                stringBuilder.AppendFormat("material.Parameters[{0}]", shaderMapping.MapFloat4(constant.Register, shaderType));
            }
            else
            {
                switch (constant.Name)
                {
                    case "g_EyePosition":
                        stringBuilder.Append("payload.Depth > 0 ? float4(WorldRayOrigin(), 0) : g_EyePosition");
                        break;

                    case "g_EyeDirection":
                        stringBuilder.Append("payload.Depth > 0 ? float4(WorldRayDirection(), 0) : g_EyeDirection");
                        break;

                    case "g_ViewportSize":
                        stringBuilder.Append("float4(DispatchRaysDimensions().xy, 1.0 / (float2)DispatchRaysDimensions().xy)");
                        break;

                    case "mrgGIAtlasParam":
                        stringBuilder.Append("float4(1, 1, 0, 0)");
                        break;

                    case "mrgHasBone":
                        stringBuilder.Append("false");
                        break;

                    case "g_ShadowMapSampler":
                    case "g_VerticalShadowMapSampler":
                    case "g_DepthSampler":
                        stringBuilder.Append("1");
                        break;

                    case "mrgTexcoordOffset":
                    case "mrgTexcoordIndex":
                    case "g_aLightField":
                    case "g_FramebufferSampler":
                    case "g_ReflectionMapSampler":
                    case "g_ReflectionMapSampler2":
                    case "g_ShadowMapParams":
                        stringBuilder.Append("0");
                        break;

                    default:
                        stringBuilder.Append(constant.Name);
                        break;
                }
            }

            stringBuilder.AppendLine(";");
        }
    }

    private static void WriteConstants(StringBuilder stringBuilder, string outName, Shader shader, ShaderMapping shaderMapping)
    {
        foreach (var constant in shader.Constants)
        {
            if (constant.Type != ConstantType.Sampler)
                WriteConstant(stringBuilder, outName, constant, shader.Type, shaderMapping);
        }
    }

    private static bool IsOutputColorClamp(Shader shader, Argument argument)
    {
        int register = int.Parse(argument.Token.AsSpan()[1..]);

        if (!shader.Definitions.TryGetValue(register, out string definition))
            return false;

        var split = definition.Split(',', StringSplitOptions.TrimEntries);
        string value;

        switch (argument.Swizzle.GetSwizzle(0))
        {
            case Swizzle.X: value = split[0]; break;
            case Swizzle.Y: value = split[1]; break;
            case Swizzle.Z: value = split[2]; break;
            case Swizzle.W: value = split[3]; break;
            default: return false;
        }

        return value == "4";
    }

    private static void WriteShaderFunction(StringBuilder stringBuilder, string functionName, Shader shader, ShaderMapping shaderMapping)
    {
        string inName = shader.Type == ShaderType.Pixel ? "ps" : "ia";
        string outName = shader.Type == ShaderType.Pixel ? "om" : "vs";

        string inNameUpperCase = inName.ToUpperInvariant();
        string outNameUpperCase = outName.ToUpperInvariant();

        var variableMap = new Dictionary<string, string>();

        stringBuilder.AppendFormat("struct {1}_{0}Params\n{{\n", inNameUpperCase, functionName);

        foreach (var (semantic, name) in shader.InSemantics)
        {
            stringBuilder.AppendFormat("\tfloat4 {0}; // {1}\n", name, semantic);
            variableMap.Add(name, $"{inName}Params.{name}");
        }

        foreach (var constant in shader.Constants)
        {
            char token = 'c';

            if (constant.Type == ConstantType.Sampler && constant.Name.StartsWith("g_"))
            {
                constant.Type = ConstantType.Float4;
                token = 's';
            }

            switch (constant.Type)
            {
                case ConstantType.Float4:
                    stringBuilder.AppendFormat("\tfloat4 {0}", constant.Name);

                    if (constant.Size > 1)
                    {
                        stringBuilder.AppendFormat("[{0}]", constant.Size);

                        for (int j = 0; j < constant.Size; j++)
                            variableMap.Add($"{token}{(constant.Register + j)}", $"{inName}Params.{constant.Name}[{j}]");
                    }

                    else
                        variableMap.Add($"{token}{constant.Register}", $"{inName}Params.{constant.Name}");

                    stringBuilder.AppendFormat(";\n");
                    break;

                case ConstantType.Bool:
                    stringBuilder.AppendFormat("\tbool {0};\n", constant.Name);
                    variableMap.Add($"b{constant.Register}", $"{inName}Params.{constant.Name}");
                    break;
            }
        }

        if (shader.Samplers.Count > 0)
        {
            foreach (var constant in shader.Constants)
            {
                if (constant.Type != ConstantType.Sampler)
                    continue;

                for (int j = 0; j < constant.Size; j++)
                {
                    string token = $"s{(constant.Register + j)}";
                    string name = constant.Name;

                    if (j > 0)
                        name += $"{j}";

                    stringBuilder.AppendFormat("\tTexture{0}<float4> {1};\n", shader.Samplers[token], name);
                    variableMap.Add(token, $"{inName}Params.{name}");
                }
            }
        }

        stringBuilder.AppendLine("};\n");     
        
        stringBuilder.AppendFormat("struct {1}_{0}Params\n{{\n", outNameUpperCase, functionName);

        if (shader.Type == ShaderType.Pixel)
        {
            stringBuilder.AppendLine("\tfloat4 oC0; // COLOR0");
            stringBuilder.AppendLine("\tfloat4 oC1; // COLOR1");
            stringBuilder.AppendLine("\tfloat4 oC2; // COLOR2");
            stringBuilder.AppendLine("\tfloat4 oC3; // COLOR3");
            stringBuilder.AppendLine("\tfloat oDepth; // DEPTH");

            variableMap.Add("oC0", $"{outName}Params.oC0");
            variableMap.Add("oC1", $"{outName}Params.oC1");
            variableMap.Add("oC2", $"{outName}Params.oC2");
            variableMap.Add("oC3", $"{outName}Params.oC3");
            variableMap.Add("oDepth", $"{outName}Params.oDepth");
        }
        else
        {
            foreach (var (semantic, name) in shader.OutSemantics)
            {
                stringBuilder.AppendFormat("\tfloat4 {0}; // {1}\n", name, semantic);
                variableMap.Add(name, $"{outName}Params.{name}");
            }
        }

        stringBuilder.AppendLine("};\n");

        stringBuilder.AppendLine("template<bool AllowTraceRay>");
        stringBuilder.AppendFormat("void {1}_{0}(inout Payload payload, inout float3 normal, inout {1}_{2}Params {3}Params, inout {1}_{4}Params {5}Params)\n{{\n",
            shader.Type == ShaderType.Pixel ? "PS" : "VS",
            functionName,
            inNameUpperCase,
            inName,
            outNameUpperCase,
            outName);

        if (shader.Definitions.Count > 0)
        {
            stringBuilder.AppendFormat("\tfloat4 C[{0}];\n\n", shader.Definitions.Max(x => x.Key) + 1);

            foreach (var definition in shader.Definitions)
                stringBuilder.AppendFormat("\tC[{0}] = float4({1});\n", definition.Key, definition.Value);

            stringBuilder.AppendLine();
        }

        if (shader.DefinitionsInt.Count > 0)
        {
            foreach (var definition in shader.DefinitionsInt)
                stringBuilder.AppendFormat("\tint4 i{0} = int4({1});\n", definition.Key, definition.Value);

            stringBuilder.AppendLine();
        }

        int registerCount = shader.Instructions
            .Where(x => x.Arguments != null)
            .SelectMany(x => x.Arguments)
            .Where(x => x.Token[0] == 'r')
            .Select(x => int.Parse(x.Token.AsSpan()[1..]) + 1)
            .Append(0)
            .Max();

        for (int j = 0; j < registerCount; j++)
            stringBuilder.AppendFormat("\tfloat4 r{0} = float4(0, 0, 0, 0);\n", j);

        if (shader.Instructions.Any(x => x.Arguments != null && x.Arguments.Any(y => y.Token[0] == 'a')))
            stringBuilder.AppendLine("\tuint4 a0 = uint4(0, 0, 0, 0);");

        stringBuilder.AppendLine();

        int indent = 1;

        bool seenDotProductWithGlobalLight = false;
        bool tracedGlobalIllumination = false;
        bool tracedShadow = false;
        bool tracedRefraction = false;
        bool tracedReflection = false;

        foreach (var instruction in shader.Instructions)
        {
            if (instruction.Arguments != null)
            {
                if (shader.Type == ShaderType.Pixel && instruction.OpCode == "min")
                {
                    if (IsOutputColorClamp(shader, instruction.Arguments[1]))
                    {
                        instruction.OpCode = "mov";
                        instruction.Arguments[1] = instruction.Arguments[2];
                    }
                    else if (IsOutputColorClamp(shader, instruction.Arguments[2]))
                    {
                        instruction.OpCode = "mov";
                        instruction.Arguments[2] = instruction.Arguments[1];
                    }
                }

                foreach (var argument in instruction.Arguments)
                {
                    if (variableMap.TryGetValue(argument.Token, out string constantName))
                        argument.Token = constantName;

                    else if (argument.Token[0] == 'c')
                        argument.Token = $"C[{argument.Token.Substring(1)}]";
                }

                if (shader.Type == ShaderType.Pixel && instruction.OpCode == "dp3")
                {
                    var globalLightArgument = instruction.Arguments.FirstOrDefault(x => x.Token.Contains("mrgGlobalLight_Direction"));

                    if (globalLightArgument != null)
                    {
                        if (!seenDotProductWithGlobalLight)
                        {
                            stringBuilder.AppendFormat("\tnormal = ({0}).xyz;\n", instruction.Arguments[1] == globalLightArgument ? instruction.Arguments[2] : instruction.Arguments[1]);
                            seenDotProductWithGlobalLight = true;
                        }

                        if (tracedGlobalIllumination || tracedReflection || tracedRefraction)
                        {
                            Console.Write(
                                "Shader uses raytraced constants before dot product with global light: {0}",
                                functionName);

                            if (tracedGlobalIllumination) Console.Write(", GI");
                            if (tracedReflection) Console.Write(", Reflection");
                            if (tracedRefraction) Console.Write(", Refraction");

                            Console.WriteLine();
                        }
                    }
                }

                if (shader.Type == ShaderType.Pixel &&
                    instruction.Arguments.Length == 3 &&
                    instruction.Arguments[2].Token.Contains(".g_") &&
                    instruction.Arguments[2].Token.Contains("Sampler"))
                {
                    instruction.OpCode = "mov";
                    instruction.Arguments[1]  = instruction.Arguments[2];
                }

                foreach (var argument in instruction.Arguments)
                {
                    if (argument.Token.Contains("g_aLightField"))
                    {
                        if (argument.Swizzle.GetSwizzle(0) == Swizzle.W)
                        {
                            if (!tracedShadow)
                            {
                                stringBuilder.AppendLine("\tfloat shadow = AllowTraceRay ? TraceShadow(payload.Random) : 1;");
                                tracedShadow = true;
                            }

                            argument.Token = "(shadow.xxxx)";
                        }
                        else
                        {
                            if (!tracedGlobalIllumination)
                            {
                                stringBuilder.AppendLine("\tfloat3 globalIllumination = AllowTraceRay ? TraceGlobalIllumination(payload, normal) : 0;");
                                tracedGlobalIllumination = true;
                            }

                            argument.Token = "float4(globalIllumination, 1.0)";
                        }
                    }
                    else if (argument.Token.Contains("g_ReflectionMap"))
                    {
                        if (!tracedReflection)
                        {
                            stringBuilder.AppendLine("\tfloat3 reflection = AllowTraceRay ? TraceReflection(payload, normal) : 0;");
                            tracedReflection = true;
                        }

                        argument.Token = "float4(reflection, 1.0)";
                    }
                    else if (argument.Token.Contains("g_FramebufferSampler"))
                    {
                        if (!tracedRefraction)
                        {
                            stringBuilder.AppendLine("\tfloat3 refraction = AllowTraceRay ? TraceRefraction(payload, normal) : 0;");
                            tracedRefraction = true;
                        }

                        argument.Token = "float4(refraction, 1.0)";
                    }
                }
            }

            string instrLine = instruction.Convert(true);

            if (instrLine.Contains('}')) --indent;

            for (int j = 0; j < indent; j++)
                stringBuilder.Append("\t");

            instrLine = instrLine.Replace("][", " + ");

            stringBuilder.AppendFormat("{0}\n", instrLine);

            if (instrLine.Contains('{')) ++indent;
        }

        if (!seenDotProductWithGlobalLight && (tracedGlobalIllumination || tracedReflection || tracedRefraction))
        {
            Console.Write("Shader uses raytraced constants despite not using global light: {0}", functionName);

            if (tracedGlobalIllumination) Console.Write(", GI");
            if (tracedReflection) Console.Write(", Reflection");
            if (tracedRefraction) Console.Write(", Refraction");

            Console.WriteLine();
        }

        stringBuilder.AppendLine("}\n");
    }

    private static void WriteRaytracingFunction(StringBuilder stringBuilder, string functionName, string shaderType, Shader vertexShader, Shader pixelShader, ShaderMapping shaderMapping)
    {
        stringBuilder.AppendFormat("[shader(\"{0}\")]\n", shaderType);
        stringBuilder.AppendFormat("void {0}_{1}(inout Payload payload : SV_RayPayload, Attributes attributes : SV_IntersectionAttributes)\n{{\n", functionName, shaderType);

        stringBuilder.AppendLine("\tVertex vertex = GetVertex(attributes);");
        stringBuilder.AppendLine("\tMaterial material = GetMaterial();");
        stringBuilder.AppendLine("\tGeometry geometry = GetGeometry();");

        stringBuilder.AppendFormat("\t{0}_IAParams iaParams = ({0}_IAParams)0;\n", functionName);

        foreach (var (semantic, name) in vertexShader.InSemantics)
        {
            switch (semantic)
            {
                case "POSITION":
                    stringBuilder.AppendFormat("\tiaParams.{0} = float4(GetPosition(), 1.0);\n", name);
                    break;

                case "NORMAL":
                    stringBuilder.AppendFormat("\tiaParams.{0} = float4(vertex.Normal, 0.0);\n", name);
                    break;

                case "TANGENT":
                    stringBuilder.AppendFormat("\tiaParams.{0} = float4(vertex.Tangent, 0.0);\n", name);
                    break;

                case "BINORMAL":
                    stringBuilder.AppendFormat("\tiaParams.{0} = float4(vertex.Binormal, 0.0);\n", name);
                    break;

                case "TEXCOORD":
                case "TEXCOORD0":
                case "TEXCOORD1":
                case "TEXCOORD2":
                case "TEXCOORD3":
                    stringBuilder.AppendFormat("\tiaParams.{0} = float4(vertex.TexCoord, 0.0, 0.0);\n", name);
                    break;

                case "COLOR":
                    stringBuilder.AppendFormat("\tiaParams.{0} = vertex.Color;\n", name);
                    break;
            }
        }

        bool isClosestHit = shaderType == "closesthit";

        WriteConstants(stringBuilder, "iaParams", vertexShader, shaderMapping);

        stringBuilder.AppendFormat("\n\t{0}_VSParams vsParams = ({0}_VSParams)0;\n", functionName);

        stringBuilder.AppendFormat("\t{0}_VS<{1}>(payload, vertex.Normal, iaParams, vsParams);\n\n", functionName, isClosestHit ? "true" : "false");

        stringBuilder.AppendFormat("\t{0}_PSParams psParams = ({0}_PSParams)0;\n", functionName);

        foreach (var (semantic, name) in pixelShader.InSemantics)
        {
            if (name == "vPos")
                stringBuilder.AppendFormat("\tpsParams.{0}.xy = (float2)DispatchRaysIndex().xy;\n", name);

            else if (vertexShader.OutSemantics.TryGetValue(semantic, out string targetName))
                stringBuilder.AppendFormat("\tpsParams.{0} = vsParams.{1};\n", name, targetName);
        }

        WriteConstants(stringBuilder, "psParams", pixelShader, shaderMapping);

        foreach (var constant in pixelShader.Constants)
        {
            if (constant.Type != ConstantType.Sampler)
                continue;

            for (int i = 0; i < constant.Size; i++)
            {
                int register = constant.Register + i;
                string token = $"s{register}";
                string name = constant.Name;

                if (i > 0)
                    name += $"{i}";

                stringBuilder.AppendFormat("\tpsParams.{0} = g_BindlessTexture{1}[NonUniformResourceIndex(material.TextureIndices[{2}])];\n",
                    name,
                    pixelShader.Samplers[token],
                    shaderMapping.CanMapSampler(register) ? shaderMapping.MapSampler(register) : 0);
            }
        }

        stringBuilder.AppendFormat("\n\t{0}_OMParams omParams = ({0}_OMParams)0;\n", functionName);

        stringBuilder.AppendFormat("\t{0}_PS<{1}>(payload, vertex.Normal, psParams, omParams);\n\n", functionName, isClosestHit ? "true" : "false");

        if (shaderType == "closesthit")
        {
            stringBuilder.Append("""
                payload.Color = omParams.oC0.rgb;
                payload.T = RayTCurrent();
            }


            """);
        }
        else if (shaderType == "anyhit")
        {
            stringBuilder.Append(""" 
                if (geometry.PunchThrough != 0)
                {
                    if (omParams.oC0.w < 0.5)
                        IgnoreHit();
                }
                else 
                {
                    if (omParams.oC0.w < NextRandom(payload.Random))
                        IgnoreHit();
                }
            }


            """);
        }
    }

    public static void WriteAllFunctions(StringBuilder stringBuilder, string functionName, Shader vertexShader, Shader pixelShader, ShaderMapping shaderMapping)
    {
        stringBuilder.AppendFormat("//\n// {0}\n//\n\n", functionName);

        WriteShaderFunction(stringBuilder, functionName, vertexShader, shaderMapping);
        WriteShaderFunction(stringBuilder, functionName, pixelShader, shaderMapping);

        WriteRaytracingFunction(stringBuilder, functionName, "closesthit", vertexShader, pixelShader, shaderMapping);
        WriteRaytracingFunction(stringBuilder, functionName, "anyhit", vertexShader, pixelShader, shaderMapping);
    }

    public static StringBuilder BeginShaderConversion()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("""
            #define _rep(x, y) for (int i##y = 0; i##y < x; ++i##y)
            #define rep(x) _rep(x, __LINE__)

            #include "ShaderLibrary.hlsli"


            """);

        return stringBuilder;
    }
}