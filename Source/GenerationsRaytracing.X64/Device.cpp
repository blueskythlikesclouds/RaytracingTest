#include "Device.h"

static ID3D12CommandQueue* createCommandQueue(ID3D12Device* device, D3D12_COMMAND_LIST_TYPE type)
{
    D3D12_COMMAND_QUEUE_DESC commandQueueDesc;
    
    commandQueueDesc.Type = type;
    commandQueueDesc.Priority = D3D12_COMMAND_QUEUE_PRIORITY_NORMAL;
    commandQueueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    commandQueueDesc.NodeMask = 0;

    ID3D12CommandQueue* commandQueue = nullptr;
    device->CreateCommandQueue(&commandQueueDesc, IID_PPV_ARGS(&commandQueue));

    assert(commandQueue);
    return commandQueue;
}

static class MessageCallback : public nvrhi::IMessageCallback
{
public:
    void message(nvrhi::MessageSeverity severity, const char* messageText) override
    {
        constexpr const char* SEVERITY_TEXT[] =
        {
            "Info",
            "Warning",
            "Error",
            "Fatal"
        };

        printf("%s: %s\n", SEVERITY_TEXT[(int)severity], messageText);
        assert(severity != nvrhi::MessageSeverity::Error && severity != nvrhi::MessageSeverity::Fatal);
    }
} messageCallback;

class MemoryAllocator : public nvrhi::d3d12::IMemoryAllocator
{
protected:
    ComPtr<D3D12MA::Allocator> allocator;

public:
    MemoryAllocator(ID3D12Device* device, IDXGIAdapter* dxgiAdapter)
    {
        D3D12MA::ALLOCATOR_DESC desc{};
        desc.Flags = D3D12MA::ALLOCATOR_FLAG_SINGLETHREADED | D3D12MA::ALLOCATOR_FLAG_DEFAULT_POOLS_NOT_ZEROED;
        desc.pDevice = device;
        desc.pAdapter = dxgiAdapter;

        D3D12MA::CreateAllocator(&desc, allocator.GetAddressOf());
    }

    HRESULT createResource(const D3D12_HEAP_PROPERTIES* heapProperties, D3D12_HEAP_FLAGS heapFlags,
        const D3D12_RESOURCE_DESC* desc, D3D12_RESOURCE_STATES initialResourceState,
        const D3D12_CLEAR_VALUE* optimizedClearValue, const IID& riid, void** resource, IUnknown** allocation) override
    {
        D3D12MA::ALLOCATION_DESC allocationDesc{};
        allocationDesc.HeapType = heapProperties->Type;

        return allocator->CreateResource(
            &allocationDesc,
            desc,
            initialResourceState,
            optimizedClearValue,
            reinterpret_cast<D3D12MA::Allocation**>(allocation),
            riid,
            resource);
    }
};

Device::Device()
{
#ifdef _DEBUG
    ComPtr<ID3D12Debug> debugInterface;
    D3D12GetDebugInterface(IID_PPV_ARGS(debugInterface.GetAddressOf()));
    assert(debugInterface);

    debugInterface->EnableDebugLayer();
#endif

    CreateDXGIFactory2(
#ifdef _DEBUG
        DXGI_CREATE_FACTORY_DEBUG,
#else
        0,
#endif
        IID_PPV_ARGS(dxgiFactory.GetAddressOf()));

    assert(dxgiFactory);

    ComPtr<IDXGIAdapter> dxgiAdapter;
    dxgiFactory->EnumAdapters(0, dxgiAdapter.GetAddressOf());
    assert(dxgiAdapter);

    D3D12CreateDevice(dxgiAdapter.Get(), D3D_FEATURE_LEVEL_11_0, IID_PPV_ARGS(d3d12.device.GetAddressOf()));
    assert(d3d12.device);

#ifdef _DEBUG
    ComPtr<ID3D12InfoQueue> infoQueue;
    d3d12.device.As(&infoQueue);

    //infoQueue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_ERROR, true);
    //infoQueue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_CORRUPTION, true);
    //infoQueue->SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY_WARNING, false);

    // Disable messages we're aware of and okay with
    D3D12_MESSAGE_ID ids[] =
    {
        D3D12_MESSAGE_ID_CLEARRENDERTARGETVIEW_MISMATCHINGCLEARVALUE
    };

    D3D12_INFO_QUEUE_FILTER filter{};
    filter.DenyList.NumIDs = _countof(ids);
    filter.DenyList.pIDList = ids;

    infoQueue->AddStorageFilterEntries(&filter);
#endif

    d3d12.graphicsCommandQueue.Attach(createCommandQueue(d3d12.device.Get(), D3D12_COMMAND_LIST_TYPE_DIRECT));
    memoryAllocator = std::make_unique<MemoryAllocator>(d3d12.device.Get(), dxgiAdapter.Get());

    nvrhi::d3d12::DeviceDesc deviceDesc;
    deviceDesc.pDevice = d3d12.device.Get();
    deviceDesc.pGraphicsCommandQueue = d3d12.graphicsCommandQueue.Get();
    deviceDesc.messageCallback = &messageCallback;
    deviceDesc.memoryAllocator = memoryAllocator.get();

    nvrhi = nvrhi::d3d12::createDevice(deviceDesc);
    assert(nvrhi);

#ifdef _DEBUG
    nvrhi = nvrhi::validation::createValidationLayer(nvrhi);
    assert(nvrhi);
#endif
}

Device::~Device()
{
    nvrhi->waitForIdle();
}