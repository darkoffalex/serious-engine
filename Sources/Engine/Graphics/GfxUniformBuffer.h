#ifndef SE_INCL_GFXUNIFORM_H
#define SE_INCL_GFXUNIFORM_H
#ifdef PRAGMA_ONCE
#pragma once
#endif

class ENGINE_API CGfxUniformBuffer
{
public:
	CGfxUniformBuffer();
	explicit CGfxUniformBuffer(SLONG lSize, UINT uBinding, UINT eUsage);
	CGfxUniformBuffer(const CGfxUniformBuffer& other) = delete;
	CGfxUniformBuffer(CGfxUniformBuffer&& other) noexcept;
	~CGfxUniformBuffer();

	CGfxUniformBuffer& operator=(const CGfxUniformBuffer& other) = delete;
	CGfxUniformBuffer& operator=(CGfxUniformBuffer&& other) noexcept;

	UINT Id() const;
	UINT Binding() const;
	UINT Usage() const;
	SLONG Size() const;

	void UpdateBufferData(SLONG lOffset, SLONG lSize, void* data);

protected:
	UINT ubo_uId;
	UINT ubo_uBinding;
	SLONG ubo_lSize;
	UINT ubo_eUsage;
};

#endif  /* include-once check. */