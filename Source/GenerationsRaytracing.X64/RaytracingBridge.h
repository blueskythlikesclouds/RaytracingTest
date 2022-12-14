#pragma once

#include "DescriptorTableAllocator.h"
#include "Model.h"
#include "Instance.h"
#include "Material.h"

class Upscaler;
struct Device;
struct Bridge;

struct RTConstants
{
    float prevProj[4][4]{};
    float prevView[4][4]{};

    float jitterX = 0.0f;
    float jitterY = 0.0f;
    int currentFrame = 0;
    uint8_t padding0[4];

    float environmentColor[3]{};
};

struct RaytracingBridge
{
    nvrhi::ShaderLibraryHandle shaderLibrary;
    nvrhi::ShaderLibraryHandle shaderLibraryAutoGenerated;
    ShaderMapping shaderMapping;
    nvrhi::TextureHandle blueNoise;

    nvrhi::BindingLayoutHandle bindingLayout;
    nvrhi::BindingLayoutHandle bindlessLayout;

    RTConstants rtConstants{};
    nvrhi::BufferHandle rtConstantBuffer;

    DescriptorTableAllocator descriptorTableManager;
    nvrhi::DescriptorTableHandle descriptorTable;
    std::vector<nvrhi::BindingSetItem> bindingSetItems;

    std::vector<Material::GPU> materialBufferVec;
    nvrhi::BufferHandle materialBuffer;

    std::vector<Mesh::GPU> meshBufferVec;
    nvrhi::BufferHandle meshBuffer;

    std::vector<Instance::GPU> instanceBufferVec;
    nvrhi::BufferHandle instanceBuffer;
    std::vector<nvrhi::rt::InstanceDesc> instanceDescs;

    nvrhi::TextureHandle output;
    std::unique_ptr<Upscaler> upscaler;

    nvrhi::SamplerHandle linearRepeatSampler;

    nvrhi::rt::PipelineHandle pipeline;
    nvrhi::rt::ShaderTableHandle shaderTable;

    ankerl::unordered_dense::map<unsigned int, Material> materials;
    ankerl::unordered_dense::map<unsigned int, Model> models;
    std::vector<Instance> instances;

    nvrhi::ShaderHandle skinningShader;
    nvrhi::BindingLayoutHandle skinningBindingLayout;
    nvrhi::BufferHandle skinningConstantBuffer;
    nvrhi::ComputePipelineHandle skinningPipeline;

    nvrhi::SamplerHandle pointClampSampler;
    nvrhi::BindingLayoutHandle copyBindingLayout;
    nvrhi::ShaderHandle copyVertexShader;
    nvrhi::ShaderHandle copyPixelShader;
    nvrhi::FramebufferHandle copyFramebuffer;
    nvrhi::GraphicsPipelineHandle copyPipeline;
    nvrhi::DrawArguments copyDrawArguments;

    std::vector<std::pair<unsigned int, unsigned int>> pendingReleases;

    RaytracingBridge(const Device& device, const std::string& directoryPath);
    ~RaytracingBridge();

    void procMsgCreateMesh(Bridge& bridge);
    void procMsgCreateModel(Bridge& bridge);
    void procMsgCreateInstance(Bridge& bridge);
    void procMsgCreateMaterial(Bridge& bridge);
    void procMsgReleaseSingleElement(Bridge& bridge);
    void procMsgNotifySceneTraversed(Bridge& bridge);
};
