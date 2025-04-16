#ifndef SE_INCL_GFXSHADER_H
#define SE_INCL_GFXSHADER_H
#ifdef PRAGMA_ONCE
#pragma once
#endif

class ENGINE_API CGfxShader
{
public:
	CGfxShader();
	explicit CGfxShader(const std::unordered_map<UINT, std::string>& shaderSources);
	CGfxShader(const CGfxShader& other) = delete;
	CGfxShader(CGfxShader&& other) noexcept;
	~CGfxShader();

	CGfxShader& operator=(const CGfxShader& other) = delete;
	CGfxShader& operator=(CGfxShader&& other) noexcept;

	UINT Id() const;
	std::vector<INT32> UniformIds(const std::vector<std::string>& names) const;

private:
	static UINT ComplieShaderSource(UINT type, const std::string& shaderSource);

protected:
	UINT sha_uiId;
};

#endif  /* include-once check. */