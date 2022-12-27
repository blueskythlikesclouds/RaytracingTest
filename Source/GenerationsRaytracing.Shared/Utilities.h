#pragma once

#include <vector>

template<typename T>
inline size_t vectorByteSize(const std::vector<T>& vector)
{
    return vector.size() * sizeof(T);
}

template<typename T>
inline size_t vectorByteOffset(const std::vector<T>& vector, size_t index)
{
    return index * sizeof(T);
}

template<typename T>
inline size_t vectorByteStride(const std::vector<T>& vector)
{
    return sizeof(T);
}