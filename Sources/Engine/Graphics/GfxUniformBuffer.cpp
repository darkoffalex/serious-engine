#include "StdH.h"
#include <Engine/Graphics/GfxUniformBuffer.h>
#include <Engine/Graphics/GfxLibrary.h>

CGfxUniformBuffer::CGfxUniformBuffer()
	: ubo_uId(0)
	, ubo_uBinding(0)
	, ubo_lSize(0)
	, ubo_eUsage(0)
{}

CGfxUniformBuffer::CGfxUniformBuffer(SLONG lSize, UINT uBinding, UINT eUsage)
	: ubo_uId(0)
	, ubo_uBinding(uBinding)
	, ubo_lSize(lSize)
	, ubo_eUsage(eUsage)
{
	gfxGenBuffers(1, &ubo_uId);
	gfxBindBuffer(GL_UNIFORM_BUFFER, ubo_uId);
	gfxBufferData(GL_UNIFORM_BUFFER, ubo_lSize, nullptr, GL_STATIC_DRAW);
	gfxBindBufferBase(GL_UNIFORM_BUFFER, ubo_uBinding, ubo_uId);
	gfxBindBuffer(GL_UNIFORM_BUFFER, 0);
}

CGfxUniformBuffer::CGfxUniformBuffer(CGfxUniformBuffer&& other) noexcept
	: CGfxUniformBuffer()
{
	std::swap(ubo_uId, other.ubo_uId);
	std::swap(ubo_uBinding, other.ubo_uBinding);
	std::swap(ubo_lSize, other.ubo_lSize);
	std::swap(ubo_eUsage, other.ubo_eUsage);
}

CGfxUniformBuffer::~CGfxUniformBuffer()
{
	gfxDeleteBuffers(1, &ubo_uId);
}

CGfxUniformBuffer& CGfxUniformBuffer::operator=(CGfxUniformBuffer&& other) noexcept
{
	std::swap(ubo_uId, other.ubo_uId);
	std::swap(ubo_uBinding, other.ubo_uBinding);
	std::swap(ubo_lSize, other.ubo_lSize);
	std::swap(ubo_eUsage, other.ubo_eUsage);
	return *this;
}

UINT CGfxUniformBuffer::Id() const
{
	return ubo_uId;
}

UINT CGfxUniformBuffer::Binding() const
{
	return ubo_uBinding;
}

UINT CGfxUniformBuffer::Usage() const
{
	return ubo_eUsage;
}

SLONG CGfxUniformBuffer::Size() const
{
	return ubo_lSize;
}
