#include "texture.hpp"

static GLenum ImageTypeFormat(int format)
{
    switch(format) {
        case 1:
            return GL_RED;
            break;
        case 3:
            return GL_RGB;
            break;
        case 4:
            return GL_RGBA;
            break;
    }

    return GL_NONE;
}

Texture2D::Texture2D(const std::string file, const TextureType type, bool flip, const GLenum gl_format, Filter filter, Wrap wrap)
{
    _stbi_set_flip_vertically_on_load(flip);

    int width, height, n_channel;
    unsigned char *image = _stbi_load(file.c_str(), &width, &height, &n_channel);

    if(!image) {
        _stbi_image_free(image);
        std::cout << "ERROR::LOAD::TEXTURE_2D" << std::endl;
        this->err = true;
        return;
    }

    this->width = width;
    this->height = height;
    this->format = ImageTypeFormat(n_channel);

    glGenTextures(1, &this->texture);
    glBindTexture(GL_TEXTURE_2D, this->texture);

    GLuint _filter;
    GLuint _wrap;

    switch(wrap) {
        case Wrap::CLAMP_TO_BORDER:
            _wrap = GL_CLAMP_TO_BORDER;
            break;
        case Wrap::CLAMP_TO_EDGE:
            _wrap = GL_CLAMP_TO_EDGE;
            break;
        case Wrap::REPEAT:
        default:
            _wrap = GL_REPEAT;
            break;
    }

    switch(filter) {
        case Filter::NEAREST:
            _filter = GL_NEAREST;
            break;
        case Filter::LINEAR:
            _filter = GL_LINEAR;
            break;
        case Filter::MIPMAP:
            _filter = GL_NEAREST_MIPMAP_LINEAR;
            break;
    }

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, _wrap);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, _wrap);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, _filter);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, _filter);

    glTexImage2D(GL_TEXTURE_2D, 0, (gl_format != GL_NONE) ? gl_format : this->format, width, height, 0, this->format, GL_UNSIGNED_BYTE, image);
    glGenerateMipmap(GL_TEXTURE_2D);

    glBindTexture(GL_TEXTURE_2D, 0);

    std::cout << "LOAD::TEXTURE_2D <" << file << ">:<" << textureTypeMap[type] << ">" << std::endl;

    this->type = type;
    this->file = file;

    _stbi_image_free(image);
}

Texture2D::Texture2D(const int width, const int height, const TextureType type, const GLenum gl_internalformat, const GLenum gl_format)
{
    this->width = width;
    this->height = height;
    this->type = type;

    glGenTextures(1, &this->texture);
    glBindTexture(GL_TEXTURE_2D, this->texture);

    float border[] = { 1.0f, 1.0f, 1.0f, 1.0f };

    switch(type) {
        case TextureType::FRAMEBUFFER_DEPTH_MAPPING:
            glTexImage2D(GL_TEXTURE_2D, 0, gl_internalformat, width, height, 0, gl_format, GL_FLOAT, NULL);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);

            glTexParameterfv(GL_TEXTURE_2D, GL_TEXTURE_BORDER_COLOR, border);
            break;
        default:
            glTexImage2D(GL_TEXTURE_2D, 0, gl_internalformat, width, height, 0, gl_format, GL_UNSIGNED_BYTE, NULL);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            break;
    }

    glBindTexture(GL_TEXTURE_2D, 0);
}

Texture2D::Texture2D(const int width, const int height, unsigned int multisample) {
    this->width = width;
    this->height = height;
    this->type = TextureType::FRAMEBUFFER_MULTISAMPLE;

    glGenTextures(1, &this->texture);
    glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, this->texture);

    glTexImage2DMultisample(GL_TEXTURE_2D_MULTISAMPLE, multisample, GL_RGB, width, height, GL_TRUE);
    glBindTexture(GL_TEXTURE_2D_MULTISAMPLE, 0);
}

Texture2D::~Texture2D()
{
    glDeleteTextures(1, &this->texture);
}

void Texture2D::bind(GLenum _GL_TEXTURE)
{
    glActiveTexture(_GL_TEXTURE);
    glBindTexture(GL_TEXTURE_2D, this->texture);
}

TextureCube::TextureCube(std::vector<std::string> files, bool flip, Filter filter, Wrap wrap)
{
    _stbi_set_flip_vertically_on_load(flip);

    glGenTextures(1, &this->texture);
    glBindTexture(GL_TEXTURE_CUBE_MAP, this->texture);

    int width, height, n_channel;

    for (unsigned int i = 0; i < files.size(); ++i) {
        unsigned char *data = _stbi_load(files[i].c_str(), &width, &height, &n_channel, 0);

        if (!data) {
            std::cout << "ERROR::LOAD::CUBEMAP: " << files[i] << std::endl;
            _stbi_image_free(data);
            err = true;
        }

        this->format = ImageTypeFormat(n_channel);

        glTexImage2D(GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 0, this->format, width, height, 0, this->format, GL_UNSIGNED_BYTE, data);
        _stbi_image_free(data);

        std::cout << "LOAD::TEXTURE_CUBEMAP <" << files[i] << ">" << std::endl;
    }

    if(err) return;

    GLuint _filter;
    GLuint _wrap;

    switch(wrap) {
        case Wrap::CLAMP_TO_BORDER:
            _wrap = GL_CLAMP_TO_BORDER;
            break;
        case Wrap::CLAMP_TO_EDGE:
            _wrap = GL_CLAMP_TO_EDGE;
            break;
        case Wrap::REPEAT:
        default:
            _wrap = GL_REPEAT;
            break;
    }

    switch(filter) {
        case Filter::NEAREST:
            _filter = GL_NEAREST;
            break;
        case Filter::LINEAR:
            _filter = GL_LINEAR;
            break;
        case Filter::MIPMAP:
            _filter = GL_NEAREST_MIPMAP_LINEAR;
            break;
    }

    this->files = files;
    this->width = width;
    this->height = height;

    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MIN_FILTER, _wrap);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MAG_FILTER, _wrap);

    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_S, _filter);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_T, _filter);
    glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_R, _filter);
}

TextureCube::~TextureCube()
{
    glDeleteTextures(1, &this->texture);
}

void TextureCube::bind(GLenum _GL_TEXTURE)
{
    glActiveTexture(_GL_TEXTURE);
    glBindTexture(GL_TEXTURE_CUBE_MAP, this->texture);
}
