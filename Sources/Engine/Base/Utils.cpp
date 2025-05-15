#include "stdh.h"

#include <Engine/Base/Utils.h>

namespace Utils
{
    /**
     * Load data from file as text.
     *
     * @param path Full file path
     * @returns String with full text
     */
	std::string LoadFileAsText(const std::string& path)
	{
        // Open file
        std::ifstream is(path, std::ios::in);
        if (!is.good()) {
            throw std::runtime_error("Can't open file \"" + path + "\"");
        }

        // Read as text
        std::stringstream ss;
        ss << is.rdbuf();
        if (!is.good() && !is.eof()) {
            throw std::runtime_error("Failed to read file \"" + path + "\"");
        }
        is.close();

        // Return text
        return ss.str();
	}

    /**
     * Load data from file as binary.
     *
     * @param path Full file path
     * @returns Vector of bytes
     */
	std::vector<unsigned char> LoadFileAsBytes(const std::string& path)
	{
        // Open file
        std::ifstream is(path, std::ios::binary | std::ios::in | std::ios::ate);
        if (!is.good()) {
            throw std::runtime_error("Can't open file \"" + path + "\"");
        }

        // Get file size
        const auto size = static_cast<std::streamsize>(is.tellg());
        if (size < 0) {
            is.close();
            throw std::runtime_error("Failed to determine size of file \"" + path + "\"");
        }

        // Reserve memory
        std::vector<unsigned char> result;
        result.resize(size);

        // Read to vector
        is.seekg(0, std::ios::beg);
        is.read(reinterpret_cast<char*>(result.data()), size);

        // Check reading is done
        if (is.gcount() != size) {
            is.close();
            throw std::runtime_error("Failed to read file \"" + path + "\" completely");
        }

        // Close file & return data
        is.close();
        return result;
	}
}