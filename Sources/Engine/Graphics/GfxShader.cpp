#include "StdH.h"
#include <Engine/Graphics/GfxShader.h>
#include <Engine/Graphics/GfxLibrary.h>

CGfxShader::CGfxShader()
	: sha_uiId(0)
{}

CGfxShader::CGfxShader(const std::unordered_map<UINT, std::string>& shaderSources)
	: sha_uiId(0)
{
	sha_uiId = gfxCreateProgram();
	std::vector<UINT> shaderIds;

	try
	{
		if(shaderSources.empty()) throw std::runtime_error("Shader stage source list is empty");

		for (auto& entry : shaderSources)
		{
			INDEX id = ComplieShaderSource(entry.first, entry.second);
			gfxAttachShader(sha_uiId, id);
			shaderIds.push_back(id);
		}
	}
	catch (std::exception& ex)
	{
		for (const auto& id : shaderIds) gfxDeleteShader(id);
		gfxDeleteProgram(sha_uiId);
		sha_uiId = 0;

		throw std::runtime_error(ex.what());
	}

	gfxLinkProgram(sha_uiId);
	for (const auto& id : shaderIds) gfxDeleteShader(id);

	INT success;
	gfxGetShaderiv(sha_uiId, GL_LINK_STATUS, &success);

	if (!success)
	{
		std::string msg;
		INT msgLen = 0;

		gfxGetProgramiv(sha_uiId, GL_INFO_LOG_LENGTH, &msgLen);
		msg.resize(msgLen);
		gfxGetShaderInfoLog(sha_uiId, msgLen, nullptr, (CHAR*)msg.data());

		gfxDeleteProgram(sha_uiId);
		sha_uiId = 0;

		std::stringstream ss;
		ss << "Failed to link shader program:" << msg << std::endl;
		throw std::runtime_error(ss.str());
	}
}

CGfxShader::CGfxShader(CGfxShader&& other) noexcept
	: CGfxShader()
{
	std::swap(sha_uiId, other.sha_uiId);
}

CGfxShader::~CGfxShader()
{
	if (sha_uiId) gfxDeleteProgram(sha_uiId);
	sha_uiId = 0;
}

CGfxShader& CGfxShader::operator=(CGfxShader&& other) noexcept
{
	if (&other == this) return *this;
	std::swap(sha_uiId, other.sha_uiId);
	return *this;
}

UINT CGfxShader::Id() const
{
	return sha_uiId;
}

std::vector<int> CGfxShader::UniformIds(const std::vector<std::string>& names) const
{
	std::vector<int> ids(names.size());

	for (size_t i = 0; i < names.size(); i++)
	{
		ids[i] = gfxGetUniformLocation(sha_uiId, names[i].c_str());
	}

	return ids;
}

UINT CGfxShader::ComplieShaderSource(UINT type, const std::string& shaderSource)
{
	const UINT id = gfxCreateShader(type);
	const CHAR* sourceCstr = shaderSource.c_str();
	gfxShaderSource(id, 1, &sourceCstr, nullptr);
	gfxCompileShader(id);

	INT success;
	gfxGetShaderiv(id, GL_COMPILE_STATUS, &success);
	if (!success)
	{
		std::string msg;
		INT msgLen = 0;

		gfxGetShaderiv(id, GL_INFO_LOG_LENGTH, &msgLen);
		msg.resize(msgLen);
		gfxGetShaderInfoLog(id, msgLen, nullptr, (CHAR*)msg.data());

		std::stringstream ss;
		ss << "Shader compilation error ";
		ss << "(type - " << type << "): " << msg << std::endl;
		throw std::runtime_error(ss.str());
	}

	return id;
}
