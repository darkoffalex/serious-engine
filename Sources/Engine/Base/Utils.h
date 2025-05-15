#pragma once

namespace Utils
{
	/**
	 * Load data from file as text.
	 * 
	 * @param path Full file path
	 * @returns String with full text
	 */
	std::string LoadFileAsText(const std::string& path);

	/**
	 * Load data from file as binary.
	 * 
	 * @param path Full file path
	 * @returns Vector of bytes
	 */
	std::vector<unsigned char> LoadFileAsBytes(const std::string& path);
}