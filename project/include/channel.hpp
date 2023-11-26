#ifndef CHANNEL_HPP
#define CHANNEL_HPP

#include "texture.hpp"

class Channel
{
    public:
        Channel(const std::string &file);
        Channel(unsigned channel, TextureType type, Filter filter, Wrap wrap, bool v_flip = true);

        virtual ~Channel();

    protected:

        unsigned channel;
        TextureType type;

        Filter filter;
        Wrap wrap;

        bool v_flip;

    private:
};

#endif // CHANNEL_HPP
