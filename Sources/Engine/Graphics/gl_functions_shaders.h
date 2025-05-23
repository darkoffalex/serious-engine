DLLFUNCTION(OGL, GLuint, glCreateShader, (GLenum shaderType), 0, 1);

DLLFUNCTION(OGL, void, glDeleteShader, (GLuint shader), 0, 1);

DLLFUNCTION(OGL, void, glShaderSource, (GLuint shader, GLsizei count, const GLchar** string, const GLint* length), 0, 1);

DLLFUNCTION(OGL, void, glCompileShader, (GLuint shader), 0, 1);

DLLFUNCTION(OGL, void, glGetShaderiv, (GLuint shader, GLenum pname, GLint* params), 0, 1);

DLLFUNCTION(OGL, void, glGetShaderInfoLog, (GLuint shader, GLsizei maxLength, GLsizei* length, GLchar* infoLog), 0, 1);

DLLFUNCTION(OGL, GLuint, glCreateProgram, (void), 0, 1);

DLLFUNCTION(OGL, void, glDeleteProgram, (GLuint program), 0, 1);

DLLFUNCTION(OGL, void, glAttachShader, (GLuint program, GLuint shader), 0, 1);

DLLFUNCTION(OGL, void, glLinkProgram, (GLuint program), 0, 1);

DLLFUNCTION(OGL, void, glUseProgram, (GLuint program), 0, 1);

DLLFUNCTION(OGL, void, glGetProgramiv, (GLuint program, GLenum pname, GLint* params), 0, 1);

DLLFUNCTION(OGL, void, glGetProgramInfoLog, (GLuint program, GLsizei maxLength, GLsizei* length, GLchar* infoLog), 0, 1);

DLLFUNCTION(OGL, GLint, glGetUniformLocation, (GLuint program, const GLchar* name), 0, 1);

DLLFUNCTION(OGL, void, glUniformMatrix4fv, (GLint location, GLsizei count, GLboolean transpose, const GLfloat* value), 0, 1);

DLLFUNCTION(OGL, void, glUniform1i, (GLint location, GLint v0), 0, 1);

DLLFUNCTION(OGL, void, glUniform1iv, (GLint location, GLsizei count, const GLint* value), 0, 1);

DLLFUNCTION(OGL, void, glUniform1fv, (GLint location, GLsizei count, const GLfloat* value), 0, 1);

DLLFUNCTION(OGL, void, glUniform2fv, (GLint location, GLsizei count, const GLfloat* value), 0, 1);

DLLFUNCTION(OGL, void, glUniform3fv, (GLint location, GLsizei count, const GLfloat* value), 0, 1);

DLLFUNCTION(OGL, void, glUniform4fv, (GLint location, GLsizei count, const GLfloat* value), 0, 1);

DLLFUNCTION(OGL, void, glUniformMatrix3fv, (GLint location, GLsizei count, GLboolean transpose, const GLfloat* value), 0, 1);

DLLFUNCTION(OGL, void, glGenBuffers, (GLsizei n, GLuint* buffers), 0, 1);

DLLFUNCTION(OGL, void, glBindBuffer, (GLenum target, GLuint buffer), 0, 1);

DLLFUNCTION(OGL, void, glBufferData, (GLenum target, GLsizeiptr size, const void* data, GLenum usage), 0, 1);

DLLFUNCTION(OGL, void, glBindBufferBase, (GLenum target, GLuint index, GLuint buffer), 0, 1);

DLLFUNCTION(OGL, void, glDeleteBuffers, (GLsizei n, const GLuint* buffers), 0, 1);

DLLFUNCTION(OGL, void, glBufferSubData, (GLenum target, GLintptr offset, GLsizeiptr size, const void* data), 0, 1);