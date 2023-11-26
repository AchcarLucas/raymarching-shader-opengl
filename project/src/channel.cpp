#include "channel.hpp"

Channel::Channel(const std::string &file)
{
}

Channel::Channel(unsigned channel, TextureType type, Filter filter, Wrap wrap, bool v_flip)
{
    this->channel = channel;
    this->type = type;
    this->filter = filter;
    this->wrap = wrap;
    this->v_flip = v_flip;
}

Channel::~Channel()
{
    //dtor
}
